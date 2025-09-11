using Directx12Impl.Parts;
using Directx12Impl.Parts.Managers;
using Directx12Impl.Parts.Utils;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Directx12Impl;
public unsafe class DX12SwapChain: ISwapChain
{
  private readonly ID3D12Device* p_device;
  private readonly IDXGIFactory4* p_dxgiFactory;
  private readonly ID3D12CommandQueue* p_directQueue;
  private readonly DX12DescriptorHeapManager p_descriptorManager;
  private readonly DX12GraphicsDevice p_parentDevice;
  private SwapChainDescription p_description;
  private readonly D3D12 p_d3d12;
  private readonly IntPtr p_windowHandle;
  private readonly bool p_debugMode;

  private IDXGISwapChain3* p_swapChain;
  private DX12Texture[] p_backBuffers;
  private DX12DescriptorAllocation[] p_rtvAllocations;
  private uint p_currentBackBufferIndex;
  private bool p_disposed;
  private bool p_isOccluded;
  private uint p_presentCount;

  public DX12SwapChain(
      ID3D12Device* _device,
      IDXGIFactory4* _dxgiFactory,
      ID3D12CommandQueue* _directQueue,
      DX12DescriptorHeapManager _descriptorManager,
      DX12GraphicsDevice _parentDevice,
      SwapChainDescription _description,
      IntPtr _windowHandle,
      D3D12 _d3d12,
      bool _debugMode)
  {
    p_device = _device;
    p_dxgiFactory = _dxgiFactory;
    p_directQueue = _directQueue;
    p_descriptorManager = _descriptorManager ?? throw new ArgumentNullException(nameof(_descriptorManager));
    p_parentDevice = _parentDevice;
    p_description = _description ?? throw new ArgumentNullException(nameof(_description));
    p_d3d12 = _d3d12;
    p_debugMode = _debugMode;

    if(_windowHandle == IntPtr.Zero)
      throw new ArgumentException("Window handle is required", nameof(_windowHandle));

    if(p_debugMode)
    {
      var validation = DX12SwapChainValidator.ValidateDescription(p_description);
      validation.LogResults();

      if(!validation.IsValid)
        throw new ArgumentException("Invalid SwapChain description");
    }

    CreateSwapChain(_windowHandle);
    CreateBackBufferResources();

    if(p_debugMode)
    {
      DX12SwapChainDebugger.LogSwapChainInfo(p_swapChain);
    }
  }

  public event Action<uint> OnPresent;
  public event Action<uint, uint> OnResize;

  public ResourceType resourceType => ResourceType.SwapChain;
  public SwapChainDescription Description => p_description;
  public uint CurrentBackBufferIndex => p_currentBackBufferIndex;
  public uint PresentCount => p_presentCount;
  public bool IsOccluded => p_isOccluded;

  public ITexture GetBackBuffer(uint _index)
  {
    if(_index >= p_description.BufferCount)
      throw new ArgumentOutOfRangeException(nameof(_index));

    return p_backBuffers[_index];
  }

  public ITexture GetCurrentBackBuffer() => GetBackBuffer(p_currentBackBufferIndex);

  public ITextureView GetBackBufferRTV(uint _index)
  {
    if(_index >= p_description.BufferCount)
      throw new ArgumentOutOfRangeException(nameof(_index));

    // Создаем view обертку для RTV
    var viewDesc = new TextureViewDescription
    {
      Format = p_description.Format,
      MostDetailedMip = 0,
      MipLevels = 1,
      FirstArraySlice = 0,
      ArraySize = 1
    };

    return new DX12TextureView(
        p_backBuffers[_index],
        TextureViewType.RenderTarget,
        viewDesc,
        p_rtvAllocations[_index]);
  }

  public void Present(uint _syncInterval = 0)
  {
    var flags = _syncInterval == 0 ? 
               (uint)PresentFlags.PresentAllowTearing : 0u;


    //var currentBuffer = p_backBuffers[p_currentBackBufferIndex];
    //if(currentBuffer.GetCurrentState() != ResourceStates.Present)
    //{
    //  // Нужен command list для transition
    //  var commandList = p_parentDevice.GetImmediateContext();
    //  DX12ResourceStateHelper.TransitionResource(
    //      commandList,
    //      currentBuffer,
    //      ResourceStates.Present);
    //  p_parentDevice.ExecuteImmediateContext();
    //}

    HResult hr = p_swapChain->Present(_syncInterval, flags);

    switch((UInt32)hr.Value)
    {
      case 0x887A0001: // DXGI_ERROR_INVALID_CALL
        throw new InvalidOperationException("Invalid call to Present - check render target states");

      case 0x887A0002: // DXGI_ERROR_NOT_FOUND
        throw new InvalidOperationException("SwapChain not found - may have been lost");

      case 0x887A0005: // DXGI_ERROR_DEVICE_REMOVED
        throw new InvalidOperationException("Device removed - need to recreate device and swapchain");

      case 0x887A0006: // DXGI_ERROR_DEVICE_HUNG
        throw new InvalidOperationException("Device hung - need to reset");

      case 0x887A0007: // DXGI_ERROR_DEVICE_RESET
        throw new InvalidOperationException("Device reset - need to recreate resources");

      case 0x887A000A: // DXGI_STATUS_OCCLUDED
        p_isOccluded = true;
        if(p_debugMode)
          Console.WriteLine("[SwapChain] Window is occluded, skipping Present");
        break;

      default:
        p_isOccluded = false;
        if(hr.IsFailure)
          throw new InvalidOperationException($"Failed to present: {hr}");
        break;
    }

    if(hr.IsSuccess && !p_isOccluded)
    {
      p_currentBackBufferIndex = p_swapChain->GetCurrentBackBufferIndex();
      p_presentCount++;

      if(p_debugMode && p_presentCount % 60 == 0)
        Console.WriteLine($"[SwapChain] Presented {p_presentCount} frames");

      OnPresent?.Invoke(p_presentCount);
    }
  }

  public void Resize(uint _width, uint _height)
  {
    if(_width == 0 || _height == 0)
    {
      if(p_debugMode)
        Console.WriteLine("[SwapChain] Ignoring resize to zero dimensions");
      return;
    }

    if(_width == p_description.Width && _height == p_description.Height)
    {
      if(p_debugMode)
        Console.WriteLine("[SwapChain] Ignoring resize to same dimensions");
      return;
    }

    if(p_debugMode)
      Console.WriteLine($"[SwapChain] Resizing from {p_description.Width}x{p_description.Height} to {_width}x{_height}");


    p_parentDevice.WaitForGPU();

    ReleaseBackBufferResources();

    HResult hr = p_swapChain->ResizeBuffers(
        p_description.BufferCount,
        _width,
        _height,
        ConvertFormat(p_description.Format),
        ConvertSwapChainFlags(p_description.Flags));

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to resize swap chain: {hr}");

    var oldWidth = p_description.Width;
    var oldHeight = p_description.Height;
    p_description.Width = _width;
    p_description.Height = _height;

    CreateBackBufferResources();

    p_currentBackBufferIndex = p_swapChain->GetCurrentBackBufferIndex();

    if(p_debugMode)
    {
      Console.WriteLine($"[SwapChain] Resize completed successfully");
      DX12SwapChainDebugger.LogSwapChainInfo(p_swapChain);
    }

    OnResize?.Invoke(_width, _height);
  }

  public void SetFullscreenState(bool _fullscreen, IMonitor _monitor = null)
  {
    IDXGIOutput* output = null;

    if(_fullscreen && _monitor is DX12Monitor dx12Monitor)
    {
      output = dx12Monitor.GetNativeOutput();
    }

    HResult hr = p_swapChain->SetFullscreenState(_fullscreen, output);

    if(hr.IsFailure && hr != new HResult(unchecked((int)0x887A0007)))
    {
      if(p_debugMode)
        Console.WriteLine($"[SwapChain] Failed to set fullscreen state: {hr}");
      throw new InvalidOperationException($"Failed to set fullscreen state: {hr}");
    }

    if(p_debugMode)
      Console.WriteLine($"[SwapChain] Fullscreen state set to: {_fullscreen}");
  }

  public bool IsFullscreen()
  {
    int isFullscreen;
    p_swapChain->GetFullscreenState(&isFullscreen, null);
    return isFullscreen != 0;
  }

  public IntPtr GetNativeHandle()
  {
    return (IntPtr)p_swapChain;
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    ReleaseBackBufferResources();

    if(p_swapChain != null)
    {
      try
      {
        p_swapChain->SetFullscreenState(false, null);
      }
      catch (Exception ex)
      {
          if(p_debugMode)
            Console.WriteLine($"[SwapChain] Warning: Failed to exit fullscreen: {ex.Message}");
      }

      p_swapChain->Release();
      p_swapChain = null;
    }

    p_disposed = true;
  }

  private void ReleaseBackBufferResources()
  {
    if(p_backBuffers != null)
    {
      foreach(var buffer in p_backBuffers)
      {
        buffer?.Dispose();
      }
      p_backBuffers = null;
    }

    if(p_rtvAllocations != null)
    {
      foreach(var allocation in p_rtvAllocations)
      {
        allocation?.Dispose();
      }
      p_rtvAllocations = null;
    }
  }

  private Format ConvertFormat(TextureFormat _format)
  {
    return _format switch
    {
      TextureFormat.R8G8B8A8_UNORM => Format.FormatR8G8B8A8Unorm,
      TextureFormat.R8G8B8A8_UNORM_SRGB => Format.FormatR8G8B8A8UnormSrgb,
      TextureFormat.R10G10B10A2_UNORM => Format.FormatR10G10B10A2Unorm,
      TextureFormat.R16G16B16A16_FLOAT => Format.FormatR16G16B16A16Float,
      _ => Format.FormatR8G8B8A8Unorm
    };
  }

  private Silk.NET.DXGI.SwapEffect ConvertSwapEffect(GraphicsAPI.Enums.SwapEffect _effect)
  {
    return _effect switch
    {
      GraphicsAPI.Enums.SwapEffect.Discard => Silk.NET.DXGI.SwapEffect.Discard,
      GraphicsAPI.Enums.SwapEffect.Sequential => Silk.NET.DXGI.SwapEffect.Sequential,
      GraphicsAPI.Enums.SwapEffect.FlipSequential => Silk.NET.DXGI.SwapEffect.FlipSequential,
      GraphicsAPI.Enums.SwapEffect.FlipDiscard => Silk.NET.DXGI.SwapEffect.FlipDiscard,
      _ => Silk.NET.DXGI.SwapEffect.FlipDiscard
    };
  }

  private Silk.NET.DXGI.AlphaMode ConvertAlphaMode(GraphicsAPI.Enums.AlphaMode _alphaMode)
  {
    return _alphaMode switch
    {
      GraphicsAPI.Enums.AlphaMode.Unspecified => Silk.NET.DXGI.AlphaMode.Unspecified,
      GraphicsAPI.Enums.AlphaMode.Premultiplied => Silk.NET.DXGI.AlphaMode.Premultiplied,
      GraphicsAPI.Enums.AlphaMode.Straight => Silk.NET.DXGI.AlphaMode.Straight,
      GraphicsAPI.Enums.AlphaMode.Ignore => Silk.NET.DXGI.AlphaMode.Ignore,
      _ => Silk.NET.DXGI.AlphaMode.Unspecified
    };
  }

  private Scaling ConverScaling(ScalingMode _scalingMode)
  {
    return _scalingMode switch
    {
      ScalingMode.None => Scaling.None,
      ScalingMode.Stretch => Scaling.Stretch,
      ScalingMode.AspectRatioStretch => Scaling.AspectRatioStretch,
      _ => Scaling.None
    };
  }

  private uint ConvertSwapChainFlags(SwapChainFlags _flags)
  {
    uint result = 0;

    if((_flags & SwapChainFlags.AllowModeSwitch) != 0)
      result |= (uint)Silk.NET.DXGI.SwapChainFlag.AllowModeSwitch;

    if((_flags & SwapChainFlags.AllowTearing) != 0)
      result |= (uint)Silk.NET.DXGI.SwapChainFlag.AllowTearing;

    return result;
  }

  private void CreateBackBufferResources()
  {
    p_backBuffers = new DX12Texture[p_description.BufferCount];
    p_rtvAllocations = new DX12DescriptorAllocation[p_description.BufferCount];

    for(uint i = 0; i < p_description.BufferCount; i++)
    {
      ID3D12Resource* backBuffer;
      var riid = ID3D12Resource.Guid;
      HResult hr = p_swapChain->GetBuffer(i, &riid, (void**)&backBuffer);

      if(hr.IsFailure)
        throw new InvalidOperationException($"Failed to get back buffer {i}: {hr}");

      var textureDesc = new TextureDescription
      {
        Name = $"SwapChain_BackBuffer_{i}",
        Width = p_description.Width,
        Height = p_description.Height,
        Depth = 1,
        MipLevels = 1,
        ArraySize = 1,
        Format = p_description.Format,
        SampleCount = p_description.SampleCount,
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.RenderTarget,
        TextureUsage = TextureUsage.RenderTarget | TextureUsage.BackBuffer
      };

      p_backBuffers[i] = new DX12Texture(
          p_device,
          p_d3d12,
          backBuffer,
          textureDesc,
          p_descriptorManager,
          p_parentDevice);

      if(p_debugMode)
        Console.WriteLine($"[SwapChain] BackBuffer {i} state: {p_backBuffers[i].GetCurrentState()}");

      p_rtvAllocations[i] = p_descriptorManager.AllocateRTV();

      var rtvDesc = new RenderTargetViewDesc
      {
        Format = ConvertFormat(p_description.Format),
        ViewDimension = RtvDimension.Texture2D
      };
      rtvDesc.Anonymous.Texture2D.MipSlice = 0;
      rtvDesc.Anonymous.Texture2D.PlaneSlice = 0;

      p_device->CreateRenderTargetView(backBuffer, &rtvDesc, p_rtvAllocations[i].CpuHandle);
    }
  }

  private bool SupportsVariableRefreshRate()
  {
    return (p_description.Flags & SwapChainFlags.AllowTearing) != 0;
  }

  private void CreateSwapChain(IntPtr _windowHandle)
  {
    var swapChainDesc = new SwapChainDesc1
    {
      Width = p_description.Width,
      Height = p_description.Height,
      Format = ConvertFormat(p_description.Format),
      Stereo = false,
      SampleDesc = new SampleDesc
      {
        Count = p_description.SampleCount,
        Quality = p_description.SampleQuality,
      },
      BufferUsage = DXGI.UsageRenderTargetOutput,
      BufferCount = p_description.BufferCount,
      Scaling = Scaling.None,
      SwapEffect = ConvertSwapEffect(p_description.SwapEffect),
      AlphaMode = ConvertAlphaMode(p_description.AlphaMode),
      Flags = ConvertSwapChainFlags(p_description.Flags)
    };

    if(p_debugMode)
    {
      if(DX12SwapChainDebugger.ValidateSwapChainSettings(&swapChainDesc, out var issues))
      {
        Console.WriteLine("[SwapChain] SwapChain settings validation passed");
      }
      else
      {
        Console.WriteLine("[SwapChain] SwapChain settings validation issues:");
        foreach(var issue in issues)
          Console.WriteLine($"[SwapChain]   - {issue}");
      }
    }

    IDXGISwapChain1* swapChain1;
    HResult hr = p_dxgiFactory->CreateSwapChainForHwnd(
        (IUnknown*)p_directQueue,
        _windowHandle,
        &swapChainDesc,
        null, // pFullscreenDesc
        null, // pRestrictToOutput
        &swapChain1);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create swap chain: {hr}");

    p_dxgiFactory->MakeWindowAssociation(_windowHandle, (uint)WindowAssociationFlags.WindowAssociationFlagNoAltEnter);

    IDXGISwapChain3* swapChain3;
    var guid = SilkMarshal.GuidPtrOf<IDXGISwapChain3>();
    hr = swapChain1->QueryInterface(guid, (void**)&swapChain3);
    swapChain1->Release();

    if(hr.IsFailure)
      throw new InvalidOperationException("Failed to get IDXGISwapChain3 interface");

    p_swapChain = swapChain3;
    p_currentBackBufferIndex = p_swapChain->GetCurrentBackBufferIndex();
  }
}
