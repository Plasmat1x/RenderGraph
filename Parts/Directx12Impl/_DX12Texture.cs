using GraphicsAPI;
using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace Directx12Impl;

public class _DX12Texture: ITexture
{
  public class TextureUploadHelper
  {
    private readonly ComPtr<ID3D12Device> p_device;
    private readonly D3D12 p_d3d12;

    public TextureUploadHelper(ComPtr<ID3D12Device> _device, D3D12 _d3d12)
    {
      p_d3d12 = _d3d12;
      p_device = _device;
    }

    public unsafe void UploadTextureData<T>(
      _DX12Texture _texture,
      T[] _data,
      uint _mipLevels,
      uint _arraySize,
      ComPtr<ID3D12GraphicsCommandList> _commandList) where T : unmanaged
    {
      var subresource = _texture.GetSubresourceIndex(_mipLevels, _arraySize);
      var footprints = new PlacedSubresourceFootprint[1];
      var numRows = new uint[1];
      var rowSizeInBytes = new ulong[1];
      ulong totalSize;

      var desc = _texture.p_resource.GetDesc();
      _texture.p_device.GetCopyableFootprints(
        &desc,
        subresource,
        1,
        0,
        footprints,
        numRows,
        rowSizeInBytes,
        &totalSize);

      var uploadBufferDesc = new BufferDescription
      {
        Name = $"{_texture.Name}_Upload",
        Size = totalSize,
        Usage = ResourceUsage.Dynamic,
        BindFlags = BindFlags.None,
      };

      using var uploadBuffer = new DX12Buffer(_texture.p_device, _texture.p_d3d12, uploadBufferDesc, null);
      var mappedPtr = uploadBuffer.Map(GraphicsAPI.Enums.MapMode.Write);
      var dataSize = (ulong)(_data.Length * sizeof(T));

      fixed (T* pData = _data)
      {
        if(footprints[0].Footprint.RowPitch == rowSizeInBytes[0])
          Buffer.MemoryCopy(pData, mappedPtr.ToPointer(), totalSize, dataSize);
        else
        {
          byte* pSrc = (byte*)pData;
          byte* pDst = (byte*)mappedPtr.ToPointer();
          var rowSize = rowSizeInBytes[0];
          var rowPitch = footprints[0].Footprint.RowPitch;

          for(uint row = 0; row < numRows[0]; row++)
          {
            Buffer.MemoryCopy(pSrc, pDst, rowPitch, rowSize);
            pSrc += rowSize;
            pDst += rowPitch;
          }
        }
      }

      uploadBuffer.Unmap();

      var barrier = new ResourceBarrier
      {
        Type = ResourceBarrierType.Transition,
        Flags = ResourceBarrierFlags.None,
      };
      barrier.Anonymous.Transition.PResource = _texture.p_resource;
      barrier.Anonymous.Transition.StateBefore = _texture.p_currentState;
      barrier.Anonymous.Transition.StateAfter = ResourceStates.CopyDest;
      barrier.Anonymous.Transition.Subresource = subresource;


      _commandList.ResourceBarrier(1, &barrier);

      var srcLocation = new TextureCopyLocation
      {
        PResource = uploadBuffer.GetResource(),
        Type = TextureCopyType.PlacedFootprint,
      };
      srcLocation.Anonymous.PlacedFootprint = footprints[0];

      var dstLocation = new TextureCopyLocation
      {
        PResource = _texture.p_resource,
        Type = TextureCopyType.SubresourceIndex,
      };
      dstLocation.Anonymous.SubresourceIndex = subresource;

      _commandList.CopyTextureRegion(&dstLocation, 0, 0, 0, &srcLocation, (Silk.NET.Direct3D12.Box*)null);

      barrier.Anonymous.Transition.StateBefore = ResourceStates.CopyDest;
      barrier.Anonymous.Transition.StateAfter = _texture.p_currentState;

      _commandList.ResourceBarrier(1, &barrier);
    }
  }

  private struct TextureViewKey: IEquatable<TextureViewKey>
  {
    public TextureViewType ViewType;
    public _TextureViewDescription Description;

    public TextureViewKey(TextureViewType _viewType, _TextureViewDescription _desc)
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

  private readonly D3D12 p_d3d12;
  private ComPtr<ID3D12Resource> p_resource;
  private TextureDescription p_description;
  private Format p_dxgiFormat;
  private ResourceDesc p_resourceDesc;
  private ResourceStates p_currentState;
  private ComPtr<ID3D12Device> p_device;
  private readonly Dictionary<TextureViewKey, _DX12TextureView> p_views = [];
  private readonly DX12DescriptorHeapManager p_descriptorManager;
  private bool p_disposed;

  public _DX12Texture(ComPtr<ID3D12Device> _device, 
    D3D12? _d3d12, 
    TextureDescription _desc,
    DX12DescriptorHeapManager _descriptorManager)
  {
    p_description = _desc ?? throw new ArgumentNullException(nameof(_desc));
    p_device = _device;
    p_d3d12 = _d3d12;
    p_descriptorManager = _descriptorManager ?? throw new ArgumentNullException(nameof(_descriptorManager));
    
    p_dxgiFormat = DX12Helpers.ConvertFormat(p_description.Format);
    p_currentState = ResourceStates.Present;

    CreateResource();

    p_resource.AddRef();
  }

  public _DX12Texture(ComPtr<ID3D12Device> _device,
    D3D12?_d3d12,
    ComPtr<ID3D12Resource> _resource,
    TextureDescription _desc,
    DX12DescriptorHeapManager _descriptorManager)
  {
    p_description = _desc ?? throw new ArgumentNullException(nameof(_desc));
    p_device = _device;
    p_d3d12 = _d3d12;
    p_descriptorManager = _descriptorManager ?? throw new ArgumentNullException(nameof(_descriptorManager));

    p_dxgiFormat = DX12Helpers.ConvertFormat(p_description.Format);
    p_currentState = ResourceStates.Present;

    p_resource = _resource;
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

  public ITextureView CreateView(_TextureViewDescription _desc)
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

    var view =  CreateViewInternal(viewType, _desc);

    p_views.Add(key,view);

    return view;
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

  public ITextureView GetDefaultDepthStencilView() => CreateView(new _TextureViewDescription
  {
    Format = p_description.Format,
    MostDetailedMip = 0,
    MipLevels = 1,
    FirstArraySlice = 0,
    ArraySize = 1
  });

  public ITextureView GetDefaultRenderTargetView() => CreateView(new _TextureViewDescription
  {
    Format = p_description.Format,
    MostDetailedMip = 0,
    MipLevels = 1,
    FirstArraySlice = 0,
    ArraySize = 1
  });

  public ITextureView GetDefaultShaderResourceView() => CreateView(new _TextureViewDescription
  {
    Format = p_description.Format,
    MostDetailedMip = 0,
    MipLevels = p_description.MipLevels,
    FirstArraySlice = 0,
    ArraySize = p_description.ArraySize
  });

  public ITextureView GetDefaultUnorderedAccessView() => CreateView(new _TextureViewDescription
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

  private unsafe _DX12TextureView CreateViewInternal(TextureViewType _viewType, _TextureViewDescription _description)
  {
    DescriptorAllocation allocation = null;

    switch(_viewType)
    {
      case TextureViewType.ShaderResource:
      {
        allocation = p_descriptorManager.AllocateCBVSRVUAV();
        var srvDesc = new ShaderResourceViewDesc
        {
          Format = p_dxgiFormat,
          ViewDimension = GetSRVDimension(),
          Shader4ComponentMapping = 0 // TODO: implement shader mapping 
        };
        FillSRVDescription(ref srvDesc, _description);
        p_device.CreateShaderResourceView(p_resource, &srvDesc, allocation.CpuHandle);
      }
      break;

      case TextureViewType.RenderTarget:
      {
        allocation = p_descriptorManager.AllocateRTV();
        var rtvDesc = new RenderTargetViewDesc
        {
          Format = p_dxgiFormat,
          ViewDimension = GetRTVDimension()
        };

        FillRTVDescription(ref rtvDesc, _description);

        p_device.CreateRenderTargetView(p_resource, &rtvDesc, allocation.CpuHandle);
      }
      break;

      case TextureViewType.DepthStencil:
      {
        allocation = p_descriptorManager.AllocateDSV();

        var dsvDesc = new DepthStencilViewDesc
        {
          Format = GetDepthStencilFormat(p_dxgiFormat),
          ViewDimension = GetDSVDimension(),
          Flags = DsvFlags.None
        };

        FillDSVDescription(ref dsvDesc, _description);

        p_device.CreateDepthStencilView(p_resource, &dsvDesc, allocation.CpuHandle);
      }
      break;

      case TextureViewType.UnorderedAccess:
      {
        allocation = p_descriptorManager.AllocateCBVSRVUAV();
        var uavDesc = new UnorderedAccessViewDesc
        {
          Format = p_dxgiFormat,
          ViewDimension = GetUAVDimension()
        };

        // Заполняем специфичные для dimension поля
        FillUAVDescription(ref uavDesc, _description);

        p_device.CreateUnorderedAccessView(p_resource, (ID3D12Resource*)null, &uavDesc, allocation.CpuHandle);
      }
      break;

      default:
        throw new ArgumentException($"Unsupported view type: {_viewType}");
    }

    return new _DX12TextureView(this, _viewType, _description, allocation);
  }

  private SrvDimension GetSRVDimension()
  {
    if(p_description.ArraySize > 1)
    {
      if(p_description.Depth > 1)
        throw new NotSupportedException("3D texture arrays are not supported");

      return (p_description.MiscFlags & ResourceMiscFlags.TextureCube) != 0
          ? SrvDimension.Texturecubearray
          : SrvDimension.Texture2Darray;
    }

    if(p_description.Depth > 1)
      return SrvDimension.Texture3D;
    else if(p_description.Height > 1)
      return SrvDimension.Texture2D;
    else
      return SrvDimension.Texture1D;
  }

  private RtvDimension GetRTVDimension()
  {
    if(p_description.ArraySize > 1)
      return RtvDimension.Texture2Darray;
    else if(p_description.Height > 1)
      return RtvDimension.Texture2D;
    else
      return RtvDimension.Texture1D;
  }

  private DsvDimension GetDSVDimension()
  {
    if(p_description.ArraySize > 1)
      return DsvDimension.Texture2Darray;
    else
      return DsvDimension.Texture2D;
  }

  private UavDimension GetUAVDimension()
  {
    if(p_description.ArraySize > 1)
      return UavDimension.Texture2Darray;
    else if(p_description.Depth > 1)
      return UavDimension.Texture3D;
    else if(p_description.Height > 1)
      return UavDimension.Texture2D;
    else
      return UavDimension.Texture1D;
  }

  private void FillSRVDescription(ref ShaderResourceViewDesc _desc, _TextureViewDescription _viewDesc)
  {
    switch(_desc.ViewDimension)
    {
      case SrvDimension.Texture2D:
        _desc.Anonymous.Texture2D.MostDetailedMip = _viewDesc.MostDetailedMip;
        _desc.Anonymous.Texture2D.MipLevels = _viewDesc.MipLevels;
        _desc.Anonymous.Texture2D.PlaneSlice = 0;
        _desc.Anonymous.Texture2D.ResourceMinLODClamp = 0;
        break;

      case SrvDimension.Texture2Darray:
        _desc.Anonymous.Texture2DArray.MostDetailedMip = _viewDesc.MostDetailedMip;
        _desc.Anonymous.Texture2DArray.MipLevels = _viewDesc.MipLevels;
        _desc.Anonymous.Texture2DArray.FirstArraySlice = _viewDesc.FirstArraySlice;
        _desc.Anonymous.Texture2DArray.ArraySize = _viewDesc.ArraySize;
        _desc.Anonymous.Texture2DArray.PlaneSlice = 0;
        _desc.Anonymous.Texture2DArray.ResourceMinLODClamp = 0;
        break;

        // TODO: Добавить другие dimensions
    }
  }

  private void FillRTVDescription(ref RenderTargetViewDesc _desc, _TextureViewDescription _viewDesc)
  {
    switch(_desc.ViewDimension)
    {
      case RtvDimension.Texture2D:
        _desc.Anonymous.Texture2D.MipSlice = _viewDesc.MostDetailedMip;
        _desc.Anonymous.Texture2D.PlaneSlice = 0;
        break;

        // TODO: Добавить другие dimensions
    }
  }

  private void FillDSVDescription(ref DepthStencilViewDesc _desc, _TextureViewDescription _viewDesc)
  {
    switch(_desc.ViewDimension)
    {
      case DsvDimension.Texture2D:
        _desc.Anonymous.Texture2D.MipSlice = _viewDesc.MostDetailedMip;
        break;

        // TODO: Добавить другие dimensions
    }
  }

  private void FillUAVDescription(ref UnorderedAccessViewDesc _desc, _TextureViewDescription _viewDesc)
  {
    switch(_desc.ViewDimension)
    {
      case UavDimension.Texture2D:
        _desc.Anonymous.Texture2D.MipSlice = _viewDesc.MostDetailedMip;
        _desc.Anonymous.Texture2D.PlaneSlice = 0;
        break;

        // TODO: Добавить другие dimensions
    }
  }

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(_DX12Texture));
  }
}