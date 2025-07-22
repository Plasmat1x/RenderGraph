using Directx12Impl.Managers;

using GraphicsAPI;
using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System.Runtime.InteropServices;

namespace Directx12Impl;
public class DX12GraphicsDevice: IGraphicsDevice
{
  private readonly D3D12 p_d3d12;
  private readonly DXGI p_dxgi;

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

  private ComPtr<ID3D12DescriptorHeap> p_rtvHeap;
  private ComPtr<ID3D12DescriptorHeap> p_dsvHeap;
  private ComPtr<ID3D12DescriptorHeap> p_cbvSrvUavHeap;
  private ComPtr<ID3D12DescriptorHeap> p_samplerHeap;

  private DescriptorAllocator p_rtvAllocator;
  private DescriptorAllocator p_dsvAllocator;
  private DescriptorAllocator p_cbvSrvUavAllocator;
  private DescriptorAllocator p_samplerAllocator;

  private FrameFenceManager p_frameManager;
  private const int p_frameCount = 3;

  private DX12PipelineStateCache p_pipelineStateCache;
  private DX12RootSignatureCache p_rootSignatureCache;

  private DeviceCapabilities p_capabilities;
  private bool p_disposed;

  public DX12GraphicsDevice(bool _enableDebugLayer = false)
  {
    p_d3d12 = D3D12.GetApi();
    p_dxgi = DXGI.GetApi();
  }

  public string Name { get; private set; }

  public API API => API.DirectX12;

  public DeviceCapabilities Capabilities => p_capabilities;

  public ITexture CreateTexture(TextureDescription _desc)
  {
    return new DX12Texture(p_device, p_d3d12, _desc, ReleaseDescriptor);
  }

  public IBuffer CreateBuffer(BufferDescription _desc)
  {
    return new DX12Buffer(p_device, p_d3d12, _desc, ReleaseDescriptor);
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
    return new DX12RenderState(p_device, p_d3d12, _description, _pipelineDescription, p_rootSignatureCache, p_pipelineStateCache);
  }

  public ISampler CreateSampler(SamplerDescription _desc)
  {
    var descriptor = p_samplerAllocator.Allocate();
    return new DX12Sampler(p_device, _desc, descriptor, ReleaseDescriptor);
  }

  public CommandBuffer CreateCommandBuffer()
  {
    return CreateCommandBuffer(CommandBufferType.Direct);
  }

  public CommandBuffer CreateCommandBuffer(CommandBufferType _type)
  {
    return new DX12CommandBuffer(p_device, p_d3d12, _type);
  }

  public IFence CreateFence(ulong _initialValue = 0)
  {
    return new DX12Fence(p_device, _initialValue);
  }

  public unsafe void ExecuteCommandBuffer(CommandBuffer _commandBuffer)
  {
    if(_commandBuffer is not DX12CommandBuffer dx12CommandBuffer)
      throw new ArgumentException("Invalid command buffer type");

    var commandList = dx12CommandBuffer.GetCommandList();
    ID3D12CommandList* lists = (ID3D12CommandList*)commandList;

    var queue = _commandBuffer.Type switch
    {
      CommandBufferType.Compute => p_computeQueue,
      CommandBufferType.Copy => p_copyQueue,
      _ => p_directQueue
    };

    queue.ExecuteCommandLists(1, &lists);
  }

  public unsafe void ExecuteCommandBuffers(CommandBuffer[] _commandBuffers)
  {
    if(_commandBuffers == null || _commandBuffers.Length == 0)
      return;

    var commandLists = stackalloc ID3D12CommandList*[_commandBuffers.Length];

    for(int i = 0; i < _commandBuffers.Length; i++)
    {
      if(_commandBuffers[i] is not DX12CommandBuffer dx12CommandBuffer)
        throw new ArgumentException($"Invalid command buffer type at index {i}");

      commandLists[i] = (ID3D12CommandList*)dx12CommandBuffer.GetCommandList();
    }

    p_directQueue.ExecuteCommandLists((uint)_commandBuffers.Length, commandLists);
  }

  public void WaitForGPU()
  {
    p_frameManager.WaitForGPU(p_directQueue);
  }

  public void WaitForFence(IFence _fence)
  {
    throw new NotImplementedException();
  }

  public ISwapChain CreateSwapChain(SwapChainDescription _desc)
  {
    throw new NotImplementedException("Swap chain creation requires window handle");
  }

  public void Present()
  {
    throw new NotImplementedException("Present is handled by swap chain");
  }

  public void BeginFrame()
  {
    p_frameManager.WaitForPreviousFrame();
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

  public uint GetFormatBytesPerPixel(TextureFormat _format) => DX12Helpers.GetFormatSize(DX12Helpers.ConvertFormat(_format));

  public SampleCountFlags GetSupportedSampleCounts(TextureFormat _textureFormat)
  {
    throw new NotImplementedException();
  }

  public ulong GetAvailableMemory()
  {
    throw new NotImplementedException();
  }

  public unsafe void Dispose()
  {
    if(p_disposed)
      return;

    WaitForGPU();

    p_frameManager?.Dispose();
    p_pipelineStateCache?.Dispose();
    p_rootSignatureCache?.Dispose();

    p_rtvHeap.Dispose();
    p_dsvHeap.Dispose();
    p_cbvSrvUavHeap.Dispose();
    p_samplerHeap.Dispose();

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
    CreateDescriptorHeaps();

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
        SilkMarshal.GuidPtrOf<IDXGIFactory4>(),
        (void**)&factory);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create DXGI factory: {hr}");

    p_dxgiFactory = factory;
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

  private unsafe void CreateDescriptorHeaps()
  {
    p_rtvDescriptorSize = p_device.GetDescriptorHandleIncrementSize(DescriptorHeapType.Rtv);
    p_dsvDescriptorSize = p_device.GetDescriptorHandleIncrementSize(DescriptorHeapType.Dsv);
    p_cbvSrvUavDescriptorSize = p_device.GetDescriptorHandleIncrementSize(DescriptorHeapType.CbvSrvUav);
    p_samplerDescriptorSize = p_device.GetDescriptorHandleIncrementSize(DescriptorHeapType.Sampler);

    var heapDesc = new DescriptorHeapDesc
    {
      Type = DescriptorHeapType.Rtv,
      NumDescriptors = 64,
      Flags = DescriptorHeapFlags.None,
      NodeMask = 0
    };

    ID3D12DescriptorHeap* rtvHeap;
    HResult hr = p_device.CreateDescriptorHeap(
        &heapDesc,
        SilkMarshal.GuidPtrOf<ID3D12DescriptorHeap>(),
        (void**)&rtvHeap);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create RTV heap: {hr}");

    p_rtvHeap = rtvHeap;
    p_rtvAllocator = new DescriptorAllocator(p_rtvHeap, p_rtvDescriptorSize, 64);

    heapDesc.Type = DescriptorHeapType.Dsv;
    heapDesc.NumDescriptors = 32;

    ID3D12DescriptorHeap* dsvHeap;
    hr = p_device.CreateDescriptorHeap(
        &heapDesc,
        SilkMarshal.GuidPtrOf<ID3D12DescriptorHeap>(),
        (void**)&dsvHeap);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create DSV heap: {hr}");

    p_dsvHeap = dsvHeap;
    p_dsvAllocator = new DescriptorAllocator(p_dsvHeap, p_dsvDescriptorSize, 32);

    heapDesc.Type = DescriptorHeapType.CbvSrvUav;
    heapDesc.NumDescriptors = 1024;
    heapDesc.Flags = DescriptorHeapFlags.ShaderVisible;

    ID3D12DescriptorHeap* cbvSrvUavHeap;
    hr = p_device.CreateDescriptorHeap(
        &heapDesc,
        SilkMarshal.GuidPtrOf<ID3D12DescriptorHeap>(),
        (void**)&cbvSrvUavHeap);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create CBV/SRV/UAV heap: {hr}");

    p_cbvSrvUavHeap = cbvSrvUavHeap;
    p_cbvSrvUavAllocator = new DescriptorAllocator(p_cbvSrvUavHeap, p_cbvSrvUavDescriptorSize, 1024);

    heapDesc.Type = DescriptorHeapType.Sampler;
    heapDesc.NumDescriptors = 64;

    ID3D12DescriptorHeap* samplerHeap;
    hr = p_device.CreateDescriptorHeap(
        &heapDesc,
        SilkMarshal.GuidPtrOf<ID3D12DescriptorHeap>(),
        (void**)&samplerHeap);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create sampler heap: {hr}");

    p_samplerHeap = samplerHeap;
    p_samplerAllocator = new DescriptorAllocator(p_samplerHeap, p_samplerDescriptorSize, 64);
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
    p_frameManager = new FrameFenceManager(p_device, p_frameCount);
  }

  private void WaitForFenceValue(ulong _fenceValue)
  {
    throw new NotImplementedException();
    //p_frameManager.Wait(_fenceValue);
  }

  private ComPtr<ID3D12Device> GetID3D12Device()
  {
    return p_device;
  }
}
