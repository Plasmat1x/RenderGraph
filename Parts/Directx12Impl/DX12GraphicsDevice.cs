using Directx12Impl.Extensions;
using Directx12Impl.Parts;
using Directx12Impl.Parts.Managers;
using Directx12Impl.Parts.Structures;
using Directx12Impl.Parts.Utils;

using GraphicsAPI;
using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Directx12Impl;
public unsafe class DX12GraphicsDevice: IGraphicsDevice
{
  private readonly D3D12 p_d3d12;
  private readonly DXGI p_dxgi;
  private readonly Queue<CommandBuffer> p_submissionQueue = [];
  private readonly object p_submissionLock = new();

  private ComPtr<ID3D12Device> p_device;
  private ComPtr<IDXGIFactory4> p_dxgiFactory;
  private ComPtr<IDXGIAdapter1> p_adapter;

  private ComPtr<ID3D12CommandQueue> p_directQueue;
  private ComPtr<ID3D12CommandQueue> p_computeQueue;
  private ComPtr<ID3D12CommandQueue> p_copyQueue;

  private uint p_rtvDescriptorSize;
  private uint p_dsvDescriptorSize;
  private uint p_cbvSrvUavDescriptorSize;
  private uint p_samplerDescriptorSize;

  private readonly List<CommandBuffer> p_commandBuffers = [];

  private DX12DescriptorHeapManager p_descriptorManager;
  private DX12UploadHeapManager p_uploadManager;

  private DX12FrameFenceManager p_frameManager;
  private const int p_frameCount = 3;

  private DX12PipelineStateCache p_pipelineStateCache;
  private DX12RootSignatureCache p_rootSignatureCache;

  private DeviceCapabilities p_capabilities;
  private bool p_immediateSync;
  private bool p_disposed;

  public DX12GraphicsDevice(bool _enableDebugLayer = false)
  {
    p_d3d12 = D3D12.GetApi();
    p_dxgi = DXGI.GetApi();

    Initialize(_enableDebugLayer);
  }

  public string Name { get; private set; }

  public API API => API.DirectX12;

  public DeviceCapabilities Capabilities => p_capabilities;

  public ITexture CreateTexture(TextureDescription _desc)
  {
    return new DX12Texture(p_device, p_d3d12, _desc, p_descriptorManager, this);
  }

  public IBuffer CreateBuffer(BufferDescription _desc)
  {
    return new DX12Buffer(p_device, p_d3d12, _desc, p_descriptorManager, this);
  }

  public IShader CreateShader(ShaderDescription _desc)
  {
    return new DX12Shader(_desc);
  }

  public IRenderState CreateRenderState(RenderStateDescription _desc)
  {
    throw new InvalidOperationException("Use CreateRenderState overload with PipelineStateDescription for D3D12");
  }

  public IRenderState CreateRenderState(RenderStateDescription _description, PipelineStateDescription _pipelineDescription)
  {
    return new DX12RenderState(_description, _pipelineDescription, p_rootSignatureCache, p_pipelineStateCache);
  }

  public ISampler CreateSampler(SamplerDescription _desc)
  {
    var allocation = p_descriptorManager.AllocateSampler();
    return new DX12Sampler(p_device, _desc, allocation);
  }

  public CommandBuffer CreateCommandBuffer()
  {
    return CreateCommandBuffer(CommandBufferType.Direct, CommandBufferExecutionMode.Immediate);
  }

  public CommandBuffer CreateCommandBuffer(CommandBufferType _type)
  {
    return CreateCommandBuffer(_type, CommandBufferExecutionMode.Immediate);

  }

  public CommandBuffer CreateCommandBuffer(CommandBufferType _type, CommandBufferExecutionMode _mode = CommandBufferExecutionMode.Immediate)
  {
    var commandBuffer = new DX12CommandBuffer(p_device, p_d3d12, _type, _mode);

    lock(p_commandBuffers)
    {
      p_commandBuffers.Add(commandBuffer);
    }

    return commandBuffer;
  }

  // === Публичные методы для загрузки данных(скрывают command list) ===

  /// <summary>
  /// Загрузить данные в буфер (внутренний метод для DX12Buffer)
  /// </summary>
  internal void UploadBufferData<T>(DX12Buffer _buffer, T[] _data, ulong _offset = 0) where T : unmanaged
  {
    if(_data == null || _data.Length == 0)
      throw new ArgumentException("Data cannot be null or empty");

    var dataSize = (ulong)(_data.Length * sizeof(T));

    // Начинаем upload batch
    var commandList = BeginResourceUpload();

    try
    {
      fixed(T* pData = _data)
      {
        _buffer.SetDataInternal(commandList, pData, dataSize, _offset);
      }

      // Завершаем и ждем
      EndResourceUpload(true);
    }
    catch
    {
      // В случае ошибки все равно закрываем command list
      EndResourceUpload(false);
      throw;
    }
  }

  /// <summary>
  /// Загрузить данные в текстуру (внутренний метод для DX12Texture)
  /// </summary>
  internal void UploadTextureData<T>(DX12Texture _texture, T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    if(_data == null || _data.Length == 0)
      throw new ArgumentException("Data cannot be null or empty");

    var commandList = BeginResourceUpload();

    try
    {
      fixed(T* pData = _data)
      {
        _texture.SetDataInternal(commandList, pData, _data.Length * sizeof(T), _mipLevel, _arraySlice);
      }

      EndResourceUpload(true);
    }
    catch
    {
      EndResourceUpload(false);
      throw;
    }
  }

  /// <summary>
  /// Пакетная загрузка нескольких ресурсов
  /// </summary>
  public void BatchUploadResources(Action<IBatchUploader> uploadAction)
  {
    var commandList = BeginResourceUpload();
    var uploader = new DX12BatchUploader(this, commandList);

    try
    {
      uploadAction(uploader);
      EndResourceUpload(true);
    }
    catch
    {
      EndResourceUpload(false);
      throw;
    }
  }

  public unsafe ISwapChain CreateSwapChain(SwapChainDescription _desc, IntPtr _windowHandle)
  {
    return new DX12SwapChain(
    p_device,
    p_dxgiFactory,
    p_directQueue,
    p_descriptorManager,
    this,
    _desc,
    _windowHandle);
  }

  public IFence CreateFence(ulong _initialValue = 0)
  {
    return new DX12Fence(p_device, _initialValue);
  }

  public DX12DescriptorHeapManager GetDescriptorManager() => p_descriptorManager;

  public void WaitForGPU()
  {
    p_frameManager.WaitForGPU(p_directQueue);
  }

  public void WaitForFence(IFence _fence)
  {
    throw new NotImplementedException();
  }

  public void Present()
  {
    throw new NotImplementedException("Present is handled by swap chain");
  }

  public void BeginFrame()
  {
    p_frameManager.WaitForPreviousFrame();
    p_descriptorManager.ResetForNewFrame();
  }

  public void EndFrame()
  {
    p_frameManager.MoveToNextFrame();
  }

  public int GetCurrentFrameIndex() => p_frameManager.CurrentFrameIndex;

  public MemoryInfo GetMemoryInfo()
  {
    throw new NotImplementedException();
  }

  public ulong GetTotalMemory()
  {
    throw new NotImplementedException();
  }

  public bool SupportsFormat(TextureFormat _format, FormatUsage _usage)
  {
    throw new NotImplementedException();
  }

  public uint GetFormatBytesPerPixel(TextureFormat _format) => _format.Convert().GetFormatSize();

  public SampleCountFlags GetSupportedSampleCounts(TextureFormat _textureFormat)
  {
    throw new NotImplementedException();
  }

  public ulong GetAvailableMemory()
  {
    throw new NotImplementedException();
  }

  public void Submit(CommandBuffer _commandBuffer)
  {
    if(_commandBuffer == null)
      throw new ArgumentNullException(nameof(_commandBuffer));


    if(_commandBuffer is GenericCommandBuffer genericCmd)
    {
      genericCmd.Execute();
    }
  }

  public void Submit(CommandBuffer[] _commandBuffers)
  {
    if(_commandBuffers == null || _commandBuffers.Length == 0)
      return;

    foreach(var cmd in _commandBuffers)
    {
      Submit(cmd);
    }
  }

  public void Submit(CommandBuffer _commandBuffer, IFence _fence, ulong _fenceValue)
  {
    Submit(_commandBuffer);

    if(_fence is DX12Fence dx12Fence)
    {
      dx12Fence.Signal(_fenceValue);
    }
  }

  public Task SubmitAsync(CommandBuffer _commandBuffer)
  {
    return Task.Run(() => Submit(_commandBuffer));
  }

  public void Present(ISwapChain _swapChain)
  {
    if(_swapChain is DX12SwapChain dx12SwapChain)
    {
      dx12SwapChain.Present();
    }
  }

  public void SetDebugName(IResource _resource, string _name)
  {
    if(_resource is DX12Resource dx12Resource && !string.IsNullOrEmpty(_name))
    {
      DX12Helpers.SetResourceName(dx12Resource.GetResource(), _name);
    }
  }

  /// <summary>
  /// Выполнить командный буфер
  /// </summary>
  public void ExecuteCommandBuffer(CommandBuffer _commandBuffer)
  {
    if(_commandBuffer is not DX12CommandBuffer dx12Buffer)
      throw new ArgumentException("Invalid command buffer type");

    ExecuteCommandBuffers(new[] { dx12Buffer });
  }

  /// <summary>
  /// Выполнить несколько командных буферов
  /// </summary>
  public void ExecuteCommandBuffers(CommandBuffer[] _commandBuffers)
  {
    var dx12Buffers = new ID3D12CommandList*[_commandBuffers.Length];

    for(int i = 0; i < _commandBuffers.Length; i++)
    {
      if(_commandBuffers[i] is not DX12CommandBuffer dx12Buffer)
        throw new ArgumentException($"Invalid command buffer type at index {i}");

      if(dx12Buffer.ImmediateMode == CommandBufferExecutionMode.Deferred)
      {
        dx12Buffer.Execute();
      }

      dx12Buffers[i] = (ID3D12CommandList*)dx12Buffer.GetNativeCommandList();
    }

    var queue = GetQueueForCommandBuffer(_commandBuffers[0] as DX12CommandBuffer);

    fixed(ID3D12CommandList** ppCommandLists = dx12Buffers)
    {
      queue->ExecuteCommandLists((uint)dx12Buffers.Length, ppCommandLists);
    }

    if(p_immediateSync)
    {
      WaitForGPU();
    }
  }

  public void BeginEvent(string _name)
  {
    Console.WriteLine($"[DEBUG] Begin Event: {_name}");
  }

  public void EndEvent()
  {
    Console.WriteLine($"[DEBUG] End Event");
  }

  public void SetMarker(string _name)
  {
    Console.WriteLine($"[DEBUG] Marker: {_name}");
  }

  /// <summary>
  /// Установить режим немедленной синхронизации (для отладки)
  /// </summary>
  public void SetImmediateSync(bool _enable)
  {
    p_immediateSync = _enable;
  }

  void IGraphicsDevice.WaitForFenceValue(IFence _fence, ulong _value)
  {
    WaitForFenceValue(_fence, _value);
  }

  public unsafe void Dispose()
  {
    if(p_disposed)
      return;

    WaitForGPU();

    p_frameManager?.Dispose();
    p_pipelineStateCache?.Dispose();
    p_rootSignatureCache?.Dispose();

    p_descriptorManager?.Dispose();

    if(p_computeQueue.Handle != p_directQueue.Handle)
      p_computeQueue.Dispose();
    if(p_copyQueue.Handle != p_directQueue.Handle)
      p_copyQueue.Dispose();
    p_directQueue.Dispose();

    p_device.Dispose();
    p_adapter.Dispose();
    p_dxgiFactory.Dispose();

    p_disposed = true;
  }

  private void EndResourceUpload(bool v)
  {
    throw new NotImplementedException();
  }

  private ID3D12GraphicsCommandList* BeginResourceUpload()
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Получить upload manager (для внутреннего использования)
  /// </summary>
  internal DX12UploadHeapManager GetUploadManager()
  {
    return p_uploadManager;
  }

  private void ReleaseDescriptor(CpuDescriptorHandle _descriptor)
  {
    // Determine which allocator owns this descriptor and free it
  }

  private void Initialize(bool _enableDebugLayer)
  {

#if DEBUG
    if(_enableDebugLayer)
    {
      EnableDebugLayer();
    }
#endif

    CreateDXGIFactory();
    CreateDevice();
    CreateCommandQueues();

    p_descriptorManager = new(p_device);
    p_uploadManager = new(p_device);

    CreateFence();

    p_pipelineStateCache = new(p_device);
    p_rootSignatureCache = new(p_device, p_d3d12);

    QueryDeviceCapabilities();

    Name = GetAdapterDescription();
  }

  private unsafe void EnableDebugLayer()
  {
    ID3D12Debug* debugController;
    HResult hr = p_d3d12.GetDebugInterface(SilkMarshal.GuidPtrOf<ID3D12Debug>(), (void**)&debugController);

    if(hr.IsSuccess)
    {
      debugController->EnableDebugLayer();
      debugController->Release();

      ID3D12Debug1* debugController1;
      hr = p_d3d12.GetDebugInterface(SilkMarshal.GuidPtrOf<ID3D12Debug1>(), (void**)&debugController1);
      if(hr.IsSuccess)
      {
        debugController1->SetEnableGPUBasedValidation(true);
        debugController1->Release();
      }
    }
  }

  private unsafe void CreateDXGIFactory()
  {
    uint dxgiFactoryFlags = 0;

#if DEBUG
    dxgiFactoryFlags |= DXGI.CreateFactoryDebug;
#endif

    IDXGIFactory4* factory;
    HResult hr = p_dxgi.CreateDXGIFactory2(
        dxgiFactoryFlags,
        out p_dxgiFactory);

    if(hr.IsFailure)
    {
      throw new InvalidOperationException($"Failed to create DXGI factory: {hr}", Marshal.GetExceptionForHR(hr));
    }
  }

  private unsafe void CreateDevice()
  {
    IDXGIAdapter1* adapter = null;
    ID3D12Device* device = null;

    for(uint i = 0; ; i++)
    {
      IDXGIAdapter1* currentAdapter;
      HResult hr = p_dxgiFactory.EnumAdapters1(i, &currentAdapter);

      if(hr == 0x887A0002) //DXGI_ERROR_NOT_FOUND
        break;

      if(hr.IsFailure)
        continue;

      AdapterDesc1 desc;
      currentAdapter->GetDesc1(&desc);

      if((desc.Flags & (uint)AdapterFlag.Software) != 0)
      {
        currentAdapter->Release();
        continue;
      }

      hr = p_d3d12.CreateDevice(
          (IUnknown*)currentAdapter,
          D3DFeatureLevel.Level110,
          SilkMarshal.GuidPtrOf<ID3D12Device>(),
          (void**)&device);

      if(hr.IsSuccess)
      {
        adapter = currentAdapter;
        break;
      }

      currentAdapter->Release();
    }

    if(device == null)
    {
      IDXGIAdapter* warpAdapter;
      HResult hr = p_dxgiFactory.EnumWarpAdapter(SilkMarshal.GuidPtrOf<IDXGIAdapter>(), (void**)&warpAdapter);

      if(hr.IsSuccess)
      {
        hr = p_d3d12.CreateDevice(
            (IUnknown*)warpAdapter,
            D3DFeatureLevel.Level110,
            SilkMarshal.GuidPtrOf<ID3D12Device>(),
            (void**)&device);

        if(hr.IsFailure)
        {
          warpAdapter->Release();
          throw new InvalidOperationException($"Failed to create WARP device: {hr}");
        }

        adapter = (IDXGIAdapter1*)warpAdapter;
      }
      else
      {
        throw new InvalidOperationException("No suitable GPU adapter found");
      }
    }

    p_adapter = adapter;
    p_device = device;
  }

  private unsafe void CreateCommandQueues()
  {
    var queueDesc = new CommandQueueDesc
    {
      Type = CommandListType.Direct,
      Priority = (int)CommandQueuePriority.Normal,
      Flags = CommandQueueFlags.None,
      NodeMask = 0
    };

    ID3D12CommandQueue* directQueue;
    HResult hr = p_device.CreateCommandQueue(
        &queueDesc,
        SilkMarshal.GuidPtrOf<ID3D12CommandQueue>(),
        (void**)&directQueue);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create direct command queue: {hr}");

    p_directQueue = directQueue;

    queueDesc.Type = CommandListType.Compute;

    ID3D12CommandQueue* computeQueue;
    hr = p_device.CreateCommandQueue(
        &queueDesc,
        SilkMarshal.GuidPtrOf<ID3D12CommandQueue>(),
        (void**)&computeQueue);

    if(hr.IsSuccess)
      p_computeQueue = computeQueue;
    else
      p_computeQueue = p_directQueue;

    queueDesc.Type = CommandListType.Copy;

    ID3D12CommandQueue* copyQueue;
    hr = p_device.CreateCommandQueue(
        &queueDesc,
        SilkMarshal.GuidPtrOf<ID3D12CommandQueue>(),
        (void**)&copyQueue);

    if(hr.IsSuccess)
      p_copyQueue = copyQueue;
    else
      p_copyQueue = p_directQueue;
  }

  private void QueryDeviceCapabilities()
  {
    p_capabilities = new DeviceCapabilities
    {
      MaxTexture1DSize = D3D12.ReqTexture1DUDimension,
      MaxTexture2DSize = D3D12.ReqTexture2DUOrVDimension,
      MaxTexture3DSize = D3D12.ReqTexture3DUVOrWDimension,
      MaxTextureCubeSize = D3D12.ReqTexturecubeDimension,
      MaxTextureArrayLayers = D3D12.ReqTexture2DArrayAxisDimension,
      MaxColorAttachments = D3D12.SimultaneousRenderTargetCount,
      MaxVertexAttributes = D3D12.IAVertexInputResourceSlotCount,
      MaxVertexBuffers = D3D12.IAVertexInputResourceSlotCount,
      MaxUniformBufferBindings = 14,
      MaxStorageBufferBindings = 64,
      MaxSampledImageBindings = 128,
      MaxStorageImageBindings = 64,
      MaxSamplerBindings = D3D12.CommonshaderSamplerSlotCount,
      MaxComputeWorkGroupSize = D3D12.CSThreadGroupMaxX,
      MaxComputeWorkGroupInvocations = D3D12.CSThreadGroupMaxThreadsPerGroup,
      SupportsGeometryShader = true,
      SupportsTessellation = true,
      SupportsComputeShader = true,
      SupportsMultiDrawIndirect = true,
      SupportsDrawIndirect = true,
      SupportsDepthClamp = true,
      SupportsAnisotropicFiltering = true,
      SupportsTextureCompressionBC = true
    };
  }

  private unsafe string GetAdapterDescription()
  {
    if(p_adapter.Handle == null)
      return "Unknown Adapter";

    AdapterDesc1 desc;
    p_adapter.GetDesc1(&desc);

    return Marshal.PtrToStringUni((IntPtr)desc.Description) ?? "Unknown Adapter";
  }

  private void CreateFence()
  {
    p_frameManager = new DX12FrameFenceManager(p_device, p_frameCount);
  }

  private void WaitForFenceValue(IFence _fence, ulong _value)
  {
    if(_fence is DX12Fence dx12Fence)
    {
      dx12Fence.WaitForValue(_value);
    }
  }

  private ComPtr<ID3D12Device> GetID3D12Device()
  {
    return p_device;
  }

  /// <summary>
  /// Получить очередь для типа командного буфера
  /// </summary>
  private ID3D12CommandQueue* GetQueueForCommandBuffer(DX12CommandBuffer _buffer)
  {
    return _buffer.Type switch
    {
      CommandBufferType.Direct => p_directQueue,
      CommandBufferType.Compute => p_computeQueue,
      CommandBufferType.Copy => p_copyQueue,
      _ => p_directQueue
    };
  }
}
