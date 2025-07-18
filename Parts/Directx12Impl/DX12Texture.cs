using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Directx12Impl;

public class DX12Texture: ITexture
{
  private struct TextureViewKey: IEquatable<TextureViewKey>
  {
    public TextureViewType ViewType;
    public TextureViewDescription Description;

    public TextureViewKey(TextureViewType _viewType, TextureViewDescription _desc)
    {
      ViewType = _viewType;
      Description = _desc;
    }

    public bool Equals(TextureViewKey _other)
    {
      return ViewType == _other.ViewType &&
       Description.Format == _other.Description.Format &&
       Description.MostDetailedMip == _other.Description.MostDetailedMip &&
       Description.MipLevels == _other.Description.MipLevels &&
       Description.FirstArraySlice == _other.Description.FirstArraySlice &&
       Description.ArraySize == _other.Description.ArraySize;
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(ViewType, Description.Format, Description.MostDetailedMip, 
        Description.MipLevels, Description.FirstArraySlice, Description.ArraySize);
    }
  }

  private ComPtr<ID3D12Resource> p_resource;
  private TextureDescription p_description;
  private Format p_dxgiFormat;
  private ResourceDesc p_resourceDesc;
  private ResourceStates p_currentState;
  private ComPtr<ID3D12Device> p_device;
  private Dictionary<TextureViewKey, DX12TextureView> p_views = [];
  private Action<CpuDescriptorHandle> p_releaseDescriptorCallback;
  private bool p_disposed;

  public DX12Texture(ComPtr<ID3D12Device> _device, 
    D3D12 _d3d12, 
    TextureDescription _desc,
    Action<CpuDescriptorHandle> _releaseDescriptorCallback)
  {
    p_description = _desc;
    p_device = _device;
    p_releaseDescriptorCallback = _releaseDescriptorCallback;
    
    p_dxgiFormat = DX12Helpers.ConvertFormat(p_description.Format);
    p_currentState = ResourceStates.Present;

    p_resource.AddRef();
  }

  public TextureDescription Description => p_description;

  public uint Width => p_description.Width;

  public uint Height => p_description.Height;

  public uint Depth => p_description.Depth;

  public uint MipLevels => p_description.MipLevels;

  public uint ArraySize => p_description.ArraySize;

  public TextureFormat Format => p_description.Format;

  public uint SampleCount => p_description.SampleCount;

  public string Name => p_description.Name;

  public ResourceType ResourceType
  {
    get
    {
      if((p_description.MiscFlags & ResourceMiscFlags.TextureCube) != 0)
      {
        return p_description.ArraySize > 6
            ? ResourceType.TextureCubeArray
            : ResourceType.TextureCube;
      }

      if(p_description.Depth > 1)
      {
        return ResourceType.Texture3D;
      }
      else if(p_description.Height > 1)
      {
        return p_description.ArraySize > 1
            ? ResourceType.Texture2DArray
            : ResourceType.Texture2D;
      }
      else
      {
        return ResourceType.Texture1D;
      }
    }
  }


  public bool IsDisposed => throw new NotImplementedException();

  public ITextureView CreateView(TextureViewDescription _desc)
  {
    ThrowIfDisposed();

    TextureViewType viewType;
    if((_desc.BindFlags & BindFlags.ShaderResource) != 0)
      viewType = TextureViewType.ShaderResource;
    else if((_desc.BindFlags & BindFlags.RenderTarget) != 0)
      viewType = TextureViewType.RenderTarget;
    else if((_desc.BindFlags & BindFlags.DepthStencil) != 0)
      viewType = TextureViewType.DepthStencil;
    else if((_desc.BindFlags & BindFlags.UnorderedAccess) != 0)
      viewType = TextureViewType.UnorderedAccess;
    else
      throw new InvalidOperationException("Texture has no valid bind flags for view creation");

    var key = new TextureViewKey(viewType, _desc);

    if(p_views.TryGetValue(key, out var existingView))
      return existingView;

    throw new NotImplementedException("Texture view creation requires descriptor allocation");
  }

  public void GenerateMips()
  {
    ThrowIfDisposed();
    throw new NotImplementedException("Mipmap generation requires compute shader or command list");
  }

  public T[] GetData<T>(uint _mipLevel = 0, uint _arraySlice = 0) where T : struct
  {
    ThrowIfDisposed();
    throw new NotImplementedException("Texture data readback requires readback buffer and copy commands");
  }

  public ITextureView GetDefaultDepthStencilView() => CreateView(new TextureViewDescription
  {
    Format = p_description.Format,
    MostDetailedMip = 0,
    MipLevels = 1,
    FirstArraySlice = 0,
    ArraySize = 1
  });

  public ITextureView GetDefaultRenderTargetView() => CreateView(new TextureViewDescription
  {
    Format = p_description.Format,
    MostDetailedMip = 0,
    MipLevels = 1,
    FirstArraySlice = 0,
    ArraySize = 1
  });

  public ITextureView GetDefaultShaderResourceView() => CreateView(new TextureViewDescription
  {
    Format = p_description.Format,
    MostDetailedMip = 0,
    MipLevels = p_description.MipLevels,
    FirstArraySlice = 0,
    ArraySize = p_description.ArraySize
  });

  public ITextureView GetDefaultUnorderedAccessView() => CreateView(new TextureViewDescription
  {
    Format = p_description.Format,
    MostDetailedMip = 0,
    MipLevels = 1,
    FirstArraySlice = 0,
    ArraySize = 1
  });

  public ulong GetMemorySize()
  {
    return DX12Helpers.CalculateTextureSize(
    p_description.Width,
    p_description.Height,
    p_description.Depth,
    p_description.MipLevels,
    p_description.ArraySize,
    p_dxgiFormat);
  }

  public unsafe IntPtr GetNativeHandle()
  {
      ThrowIfDisposed();
      return (IntPtr)p_resource.Handle;
  }

  public uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice)
  {
    return _mipLevel + _arraySlice * p_description.MipLevels;

  }

  public void SetData<T>(T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : struct
  {
    ThrowIfDisposed();
    throw new NotImplementedException("Texture data upload requires upload buffer and copy commands");
  }

  public ComPtr<ID3D12Resource> GetResource()
  {
    ThrowIfDisposed();
    return p_resource;
  }

  public ResourceStates GetCurrentState() => p_currentState;

  public void SetCurrentState(ResourceStates _state) => p_currentState = _state;

  public void Dispose()
  {
    if(p_disposed)
      return;

    foreach(var view in p_views.Values)
      view.Dispose();
    p_views.Clear();

    p_resource.Dispose();

    p_disposed = true;
    GC.SuppressFinalize(this);
  }

  private Format GetTypelessFormat(Format _format) => _format switch
  {
    Silk.NET.DXGI.Format.FormatR32Float => Silk.NET.DXGI.Format.FormatR32Typeless,
    Silk.NET.DXGI.Format.FormatD24UnormS8Uint => Silk.NET.DXGI.Format.FormatR24G8Typeless,
    Silk.NET.DXGI.Format.FormatD16Unorm => Silk.NET.DXGI.Format.FormatR16Typeless,
    _ => _format,
  };

  private Format GetDepthStencilFormat(Format _format) => _format switch
  {
    Silk.NET.DXGI.Format.FormatR32Typeless => Silk.NET.DXGI.Format.FormatD32Float,
    Silk.NET.DXGI.Format.FormatR24G8Typeless => Silk.NET.DXGI.Format.FormatD24UnormS8Uint,
    Silk.NET.DXGI.Format.FormatR16Typeless => Silk.NET.DXGI.Format.FormatD16Unorm,
    _ => _format,
  };

  private unsafe void SetDebugName(string _name)
  {
    var nameBytes = System.Text.Encoding.Unicode.GetBytes(_name + "\0");
    fixed(byte* pName = nameBytes)
    {
      p_resource.SetName((char*)pName);
    }
  }

  private ResourceStates DetermineInitialState()
  {
    if((p_description.BindFlags & BindFlags.DepthStencil) != 0)
      return ResourceStates.DepthWrite;
    else if((p_description.BindFlags & BindFlags.RenderTarget) != 0)
      return ResourceStates.RenderTarget;
    else
      return ResourceStates.Common;
  }

  private ResourceDimension GetResourceDimension()
  {
    if(p_description.Depth > 1)
      return ResourceDimension.Texture3D;
    else if(p_description.Height > 1)
      return ResourceDimension.Texture2D;
    else
      return ResourceDimension.Texture1D;
  }

  private ResourceFlags ConvertToResourceFlags()
  {
    var flags = ResourceFlags.None;

    if((p_description.BindFlags & BindFlags.RenderTarget) != 0)
      flags |= ResourceFlags.AllowRenderTarget;

    if((p_description.BindFlags & BindFlags.DepthStencil) != 0)
      flags |= ResourceFlags.AllowDepthStencil;

    if((p_description.BindFlags & BindFlags.UnorderedAccess) != 0)
      flags |= ResourceFlags.AllowUnorderedAccess;

    if((p_description.BindFlags & BindFlags.DepthStencil) != 0 &&
        (p_description.BindFlags & BindFlags.ShaderResource) == 0)
    {
      flags |= ResourceFlags.DenyShaderResource;
    }

    return flags;
  }

  private unsafe void CreateResource()
  {
    var dimension = GetResourceDimension();
    var flags = ConvertToResourceFlags();

    p_currentState = DetermineInitialState();

    var resourceFormat = p_dxgiFormat;
    if((p_description.BindFlags & BindFlags.DepthStencil) != 0)
    {
      resourceFormat = GetTypelessFormat(p_dxgiFormat);
    }

    var resourceDesc = new ResourceDesc
    {
      Dimension = dimension,
      Alignment = 0,
      Width = p_description.Width,
      Height = p_description.Height,
      DepthOrArraySize = (ushort)(p_description.Depth > 1 ? p_description.Depth : p_description.ArraySize),
      MipLevels = (ushort)p_description.MipLevels,
      Format = resourceFormat,
      SampleDesc = new SampleDesc
      {
        Count = p_description.SampleCount,
        Quality = p_description.SampleQuality
      },
      Layout = TextureLayout.LayoutUnknown,
      Flags = flags
    };

    var heapProps = new HeapProperties
    {
      Type = HeapType.Default,
      CPUPageProperty = CpuPageProperty.Unknown,
      MemoryPoolPreference = MemoryPool.Unknown,
      CreationNodeMask = 1,
      VisibleNodeMask = 1
    };

    ClearValue* pClearValue = null;
    ClearValue clearValue = default;

    if((p_description.BindFlags & BindFlags.RenderTarget) != 0)
    {
      clearValue.Format = p_dxgiFormat;
      clearValue.Anonymous.Color[0] = 0.0f;
      clearValue.Anonymous.Color[1] = 0.0f;
      clearValue.Anonymous.Color[2] = 0.0f;
      clearValue.Anonymous.Color[3] = 1.0f;
      pClearValue = &clearValue;
    }
    else if((p_description.BindFlags & BindFlags.DepthStencil) != 0)
    {
      clearValue.Format = GetDepthStencilFormat(p_dxgiFormat);
      clearValue.DepthStencil.Depth = 1.0f;
      clearValue.DepthStencil.Stencil = 0;
      pClearValue = &clearValue;
    }

    ID3D12Resource* resource;
    HResult hr = p_device.CreateCommittedResource(
        &heapProps,
        HeapFlags.None,
        &resourceDesc,
        p_currentState,
        pClearValue,
        SilkMarshal.GuidPtrOf<ID3D12Resource>(),
        (void**)&resource);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create texture: {hr}");

    p_resource = resource;

    if(!string.IsNullOrEmpty(p_description.Name))
    {
      SetDebugName(p_description.Name);
    }
  }

  private void CreateDefaultViews()
  {
    throw new NotImplementedException();
  }

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(DX12Texture));
  }
}

