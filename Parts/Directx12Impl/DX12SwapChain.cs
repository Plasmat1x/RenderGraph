using Directx12Impl.Parts;
using Directx12Impl.Parts.Managers;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Directx12Impl;
public unsafe class DX12SwapChain: ISwapChain
{
  private readonly ID3D12Device* p_device;
  private readonly IDXGIFactory4* p_dxgiFactory;
  private readonly ID3D12CommandQueue* p_directQueue;
  private readonly DX12DescriptorHeapManager p_descriptorManager;
  private readonly DX12GraphicsDevice p_parentDevice;
  private readonly SwapChainDescription p_description;

  private IDXGISwapChain3* p_swapChain;
  private DX12Texture[] p_backBuffers;
  private DX12DescriptorAllocation[] p_rtvAllocations;
  private uint p_currentBackBufferIndex;
  private bool p_disposed;

  public SwapChainDescription Description => p_description;
  public uint CurrentBackBufferIndex => p_currentBackBufferIndex;

  public DX12SwapChain(
      ID3D12Device* _device,
      IDXGIFactory4* _dxgiFactory,
      ID3D12CommandQueue* _directQueue,
      DX12DescriptorHeapManager _descriptorManager,
      DX12GraphicsDevice _parentDevice,
      SwapChainDescription _description,
      IntPtr _windowHandle)
  {
    p_device = _device;
    p_dxgiFactory = _dxgiFactory;
    p_directQueue = _directQueue;
    p_descriptorManager = _descriptorManager ?? throw new ArgumentNullException(nameof(_descriptorManager));
    p_parentDevice = _parentDevice;
    p_description = _description ?? throw new ArgumentNullException(nameof(_description));

    if(_windowHandle == IntPtr.Zero)
      throw new ArgumentException("Window handle is required", nameof(_windowHandle));

    CreateSwapChain(_windowHandle);
    CreateBackBufferResources();
  }

  public ITexture GetBackBuffer(uint _index)
  {
    if(_index >= p_description.BufferCount)
      throw new ArgumentOutOfRangeException(nameof(_index));

    return p_backBuffers[_index];
  }

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
    var flags = _syncInterval == 0 ? (uint)PresentFlags.PresentAllowTearing : 0;
    HResult hr = p_swapChain->Present(_syncInterval, flags);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to present: {hr}");

    p_currentBackBufferIndex = p_swapChain->GetCurrentBackBufferIndex();
  }

  public void Resize(uint _width, uint _height)
  {
    if(_width == 0 || _height == 0)
      return;

    ReleaseBackBufferResources();

    HResult hr = p_swapChain->ResizeBuffers(
        p_description.BufferCount,
        _width,
        _height,
        ConvertFormat(p_description.Format),
        ConvertSwapChainFlags(p_description.Flags));

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to resize swap chain: {hr}");

    p_description.Width = _width;
    p_description.Height = _height;

    CreateBackBufferResources();
  }

  public void SetFullscreenState(bool _fullscreen, IMonitor _monitor = null)
  {
    IDXGIOutput* output = null;

    if(_fullscreen && _monitor != null)
    {
      // TODO: Получить IDXGIOutput из monitor
    }

    HResult hr = p_swapChain->SetFullscreenState(_fullscreen, output);

    if(hr.IsFailure && hr != new HResult(unchecked((int)0x887A0007))) // DXGI_ERROR_NOT_CURRENTLY_AVAILABLE
      throw new InvalidOperationException($"Failed to set fullscreen state: {hr}");
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
      p_swapChain->SetFullscreenState(false, null);
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
      HResult hr = p_swapChain->GetBuffer(i, SilkMarshal.GuidPtrOf<ID3D12Resource>(), (void**)&backBuffer);

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
        SampleCount = 1,
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.RenderTarget,
        TextureUsage = TextureUsage.RenderTarget | TextureUsage.BackBuffer
      };

      p_backBuffers[i] = new DX12Texture(
          p_device,
          null,
          backBuffer,
          textureDesc,
          p_descriptorManager,
          p_parentDevice);

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
        Count = 1,
        Quality = 0
      },
      BufferUsage = DXGI.UsageRenderTargetOutput,
      BufferCount = p_description.BufferCount,
      Scaling = Scaling.None,
      SwapEffect = ConvertSwapEffect(p_description.SwapEffect),
      AlphaMode = Silk.NET.DXGI.AlphaMode.Unspecified,
      Flags = ConvertSwapChainFlags(p_description.Flags)
    };

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
