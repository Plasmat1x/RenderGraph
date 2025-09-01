using Directx12Impl.Extensions;
using Directx12Impl.Parts.Managers;
using Directx12Impl.Parts.Utils;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;
using Resources.Extensions;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System.Numerics;

namespace Directx12Impl;

/// <summary>
/// Обновленный DX12Texture, наследующийся от DX12Resource
/// </summary>
public unsafe class DX12Texture: DX12Resource, ITexture
{
  private readonly D3D12 p_d3d12;
  private readonly TextureDescription p_description;
  private readonly DX12DescriptorHeapManager p_descriptorManager;
  private readonly DX12GraphicsDevice p_parentDevice;
  private readonly Dictionary<TextureViewKey, DX12TextureView> p_views = new();

  public DX12Texture(ID3D12Device* _device, D3D12 _d3d12, TextureDescription _description, DX12DescriptorHeapManager _descriptorManager, DX12GraphicsDevice _parentDevice)
    : base(_device, _description.Name)
  {
    p_d3d12 = _d3d12;
    p_description = _description;
    p_descriptorManager = _descriptorManager;
    p_parentDevice = _parentDevice;

    CreateTextureResource();

  }

  public DX12Texture(ID3D12Device* _device, D3D12 _d3d12, ComPtr<ID3D12Resource> _resource, TextureDescription _description, DX12DescriptorHeapManager _descriptorManager, DX12GraphicsDevice _parentDevice)
  : base(_device, _description.Name)
  {
    p_d3d12 = _d3d12;
    p_description = _description;
    p_descriptorManager = _descriptorManager;
    p_parentDevice = _parentDevice;
    p_resource = _resource;
  }

  // === ITexture implementation ===
  public TextureDescription Description => p_description;
  public override ResourceType ResourceType => GetTextureResourceType();
  public uint Width => p_description.Width;
  public uint Height => p_description.Height;
  public uint Depth => p_description.Depth;
  public uint MipLevels => p_description.MipLevels;
  public uint ArraySize => p_description.ArraySize;
  public TextureFormat Format => p_description.Format;
  public uint SampleCount => p_description.SampleCount;

  public ITextureView CreateView(TextureViewDescription _description)
  {
    var key = new TextureViewKey(_description.ViewType, _description);

    if(!p_views.TryGetValue(key, out var view))
    {
      view = new DX12TextureView(this, _description, p_descriptorManager);
      p_views[key] = view;
    }

    return view;
  }

  public ITextureView GetDefaultShaderResourceView()
  {
    return CreateView(new TextureViewDescription
    {
      ViewType = TextureViewType.ShaderResource,
      Format = p_description.Format,
      MostDetailedMip = 0,
      MipLevels = p_description.MipLevels,
      FirstArraySlice = 0,
      ArraySize = p_description.ArraySize
    });
  }

  public ITextureView GetDefaultRenderTargetView()
  {
    return CreateView(new TextureViewDescription
    {
      ViewType = TextureViewType.RenderTarget,
      Format = p_description.Format,
      MostDetailedMip = 0,
      MipLevels = 1,
      FirstArraySlice = 0,
      ArraySize = p_description.ArraySize
    });
  }

  public ITextureView GetDefaultDepthStencilView()
  {
    return CreateView(new TextureViewDescription
    {
      ViewType = TextureViewType.DepthStencil,
      Format = p_description.Format,
      MostDetailedMip = 0,
      MipLevels = 1,
      FirstArraySlice = 0,
      ArraySize = p_description.ArraySize
    });
  }

  public ITextureView GetDefaultUnorderedAccessView()
  {
    return CreateView(new TextureViewDescription
    {
      ViewType = TextureViewType.UnorderedAccess,
      Format = p_description.Format,
      MostDetailedMip = 0,
      MipLevels = 1,
      FirstArraySlice = 0,
      ArraySize = p_description.ArraySize
    });
  }

  /// <summary>
  /// Установить данные в конкретный регион текстуры
  /// </summary>
  public void SetDataRegion<T>(T[] _data, uint _mipLevel, uint _arraySlice,
      uint _x, uint _y, uint _z, uint _width, uint _height, uint _depth = 1) where T : unmanaged
  {
    if(_data == null || _data.Length == 0)
      throw new ArgumentException("Data cannot be null or empty");

    ValidateSubresource(_mipLevel, _arraySlice);
    ValidateRegion(_mipLevel, _x, _y, _z, _width, _height, _depth);

    p_parentDevice.UploadTextureDataRegion(this, _data, _mipLevel, _arraySlice,
        _x, _y, _z, _width, _height, _depth);
  }

  public void SetData<T>(T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    if(_data == null || _data.Length == 0)
      throw new ArgumentException("Data cannot be null or empty");

    ValidateSubresource(_mipLevel, _arraySlice);

    var expectedSize = CalculateSubresourceSize(_mipLevel, _arraySlice);
    var actualSize = (ulong)(_data.Length * sizeof(T));

    if(actualSize < expectedSize)
    {
      throw new ArgumentException(
          $"Data size ({actualSize}) is less than expected ({expectedSize}) for mip {_mipLevel}, array {_arraySlice}");
    }

    p_parentDevice.UploadTextureData(this, _data, _mipLevel, _arraySlice);
  }

  public T[] GetData<T>(uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    ValidateSubresource(_mipLevel, _arraySlice);

    var subresourceSize = CalculateSubresourceSize(_mipLevel, _arraySlice);
    var elementSize = (ulong)sizeof(T);
    var elementCount = subresourceSize / elementSize;

    if(subresourceSize % elementSize != 0)
    {
      throw new InvalidOperationException(
          $"Subresource size ({subresourceSize}) is not aligned to element size ({elementSize})");
    }

    var result = new T[elementCount];
    p_parentDevice.ReadbackTextureData(this, result, _mipLevel, _arraySlice);

    return result;
  }

  public uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice, uint _planeSlice = 0)
  {
    return _mipLevel + _arraySlice * p_description.MipLevels + _planeSlice * p_description.MipLevels * p_description.ArraySize;
  }

  public uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice)
  {
    return GetSubresourceIndex(_mipLevel, _arraySlice, 0);
  }

  public void GenerateMips()
  {
    if(p_description.MipLevels <= 1)
      return;

    if((p_description.BindFlags & BindFlags.UnorderedAccess) == 0)
    {
      throw new InvalidOperationException(
          "Texture must have UnorderedAccess bind flag for mip generation");
    }

    p_parentDevice.GenerateTextureMips(this);
  }

  public override ulong GetMemorySize()
  {
    return p_description.GetMemorySize();
  }

  // === DX12-specific overrides ===
  public override bool SupportsState(ResourceStates _state)
  {
    var bindFlags = p_description.BindFlags;

    return _state switch
    {
      ResourceStates.RenderTarget => (bindFlags & BindFlags.RenderTarget) != 0,
      ResourceStates.DepthWrite or ResourceStates.DepthRead => (bindFlags & BindFlags.DepthStencil) != 0,
      ResourceStates.AllShaderResource => (bindFlags & BindFlags.ShaderResource) != 0,
      ResourceStates.UnorderedAccess => (bindFlags & BindFlags.UnorderedAccess) != 0,
      ResourceStates.CopyDest or ResourceStates.CopySource => true,
      _ => true
    };
  }

  private void CreateTextureResource()
  {
    //TODO: Delete After fix
    Console.WriteLine($"CreateTextureResource called for: {p_description.Name}");
    Console.WriteLine($"  Usage: {p_description.Usage}");
    Console.WriteLine($"  TextureUsage: {p_description.TextureUsage}");
    Console.WriteLine($"  BindFlags: {p_description.BindFlags}");
    Console.WriteLine($"  CPUAccessFlags: {p_description.CPUAccessFlags}");
    Console.WriteLine($"  MiscFlags: {p_description.MiscFlags}");

    var textureLayout = p_description.Usage == ResourceUsage.Staging
      ? TextureLayout.LayoutRowMajor
      : TextureLayout.LayoutUnknown;

    var resourceDesc = new ResourceDesc
    {
      Dimension = GetResourceDimension(),
      Alignment = 0,
      Width = p_description.Width,
      Height = p_description.Height,
      DepthOrArraySize = (ushort)(p_description.Depth > 1 ? p_description.Depth : p_description.ArraySize),
      MipLevels = (ushort)p_description.MipLevels,
      Format = p_description.Format.Convert(),
      SampleDesc = new SampleDesc((uint)p_description.SampleCount, 0),
      Layout = textureLayout,
      Flags = GetResourceFlags()
    };

    var heapType = p_description.Usage switch
    {
      ResourceUsage.Default => HeapType.Default,
      ResourceUsage.Immutable => HeapType.Default,
      ResourceUsage.Dynamic => HeapType.Upload,
      ResourceUsage.Staging => HeapType.Readback,
      _ => HeapType.Default
    };

    var heapProperties = new HeapProperties
    {
      Type = heapType,
      CPUPageProperty = CpuPageProperty.Unknown,
      MemoryPoolPreference = MemoryPool.Unknown
    };

    var initialState = GetInitialResourceState();

    ID3D12Resource* pResource = null;
    var riid = ID3D12Resource.Guid;

    HResult hr = p_device->CreateCommittedResource(
      &heapProperties,
      HeapFlags.None,
      &resourceDesc,
      initialState,
      null,
      &riid,
      (void**)&pResource);

    //TODO: Temp
    if(!hr.IsSuccess)
    {
      var errorMsg = $"Failed to create texture resource '{p_description.Name}' " +
           $"(Usage: {p_description.Usage}, " +
           $"Size: {p_description.Width}x{p_description.Height}x{p_description.Depth}, " +
           $"Format: {p_description.Format}, " +
           $"HeapType: {heapType}, " +
           $"Layout: {textureLayout}, " +
           $"Flags: {resourceDesc.Flags}) " +
           $"HRESULT: 0x{hr:X8}";

      DX12Helpers.ThrowIfFailed(hr, errorMsg);
    }


    p_resource = pResource;
    p_currentState = initialState;

    if(!string.IsNullOrEmpty(p_name))
    {
      DX12Helpers.SetResourceName(p_resource, p_name);
    }
  }

  private Silk.NET.Direct3D12.ResourceDimension GetResourceDimension()
  {
    if(p_description.Depth > 1)
      return Silk.NET.Direct3D12.ResourceDimension.Texture3D;
    else if(p_description.Height > 1)
      return Silk.NET.Direct3D12.ResourceDimension.Texture2D;
    else
      return Silk.NET.Direct3D12.ResourceDimension.Texture1D;
  }

  private ResourceFlags GetResourceFlags()
  {
    if(p_description.Usage == ResourceUsage.Staging)
      return ResourceFlags.None;

    ResourceFlags flags = ResourceFlags.None;

    if((p_description.BindFlags & BindFlags.RenderTarget) != 0)
      flags |= ResourceFlags.AllowRenderTarget;

    if((p_description.BindFlags & BindFlags.DepthStencil) != 0)
      flags |= ResourceFlags.AllowDepthStencil;

    if((p_description.BindFlags & BindFlags.UnorderedAccess) != 0)
      flags |= ResourceFlags.AllowUnorderedAccess;

    if((p_description.BindFlags & BindFlags.ShaderResource) == 0)
      flags |= ResourceFlags.DenyShaderResource;

    return flags;
  }

  private ResourceStates GetInitialResourceState()
  {
    if(p_description.Usage == ResourceUsage.Staging)
      return ResourceStates.CopyDest;

    if((p_description.BindFlags & BindFlags.DepthStencil) != 0)
      return ResourceStates.DepthWrite;
    else if((p_description.BindFlags & BindFlags.RenderTarget) != 0)
      return ResourceStates.RenderTarget;
    else
      return ResourceStates.Common;
  }

  private ResourceType GetTextureResourceType()
  {
    if((p_description.MiscFlags & ResourceMiscFlags.TextureCube) != 0)
    {
      return p_description.ArraySize > 6 ? ResourceType.TextureCubeArray : ResourceType.TextureCube;
    }

    if(p_description.Depth > 1)
      return ResourceType.Texture3D;
    else if(p_description.Height > 1)
      return p_description.ArraySize > 1 ? ResourceType.Texture2DArray : ResourceType.Texture2D;
    else
      return p_description.ArraySize > 1 ? ResourceType.Texture1DArray : ResourceType.Texture1D;
  }

  /// <summary>
  /// Очистить текстуру определенным цветом
  /// </summary>
  public void Clear(Vector4 _clearColor, uint _mipLevel = 0, uint _arraySlice = 0)
  {
    var pixelData = CreateClearPixelData(_clearColor);
    var width = Math.Max(1u, p_description.Width >> (int)_mipLevel);
    var height = Math.Max(1u, p_description.Height >> (int)_mipLevel);
    var depth = Math.Max(1u, p_description.Depth >> (int)_mipLevel);

    var totalPixels = width * height * depth;
    var clearData = new byte[totalPixels * (ulong)pixelData.Length];

    for(ulong i = 0; i < totalPixels; i++)
    {
      Array.Copy(pixelData, 0, clearData, (long)(i * (ulong)pixelData.Length), pixelData.Length);
    }

    SetData(clearData, _mipLevel, _arraySlice);
  }

  public override void Dispose()
  {
    if(!p_disposed)
    {
      foreach(var view in p_views.Values)
      {
        view.Dispose();
      }
      p_views.Clear();
    }

    base.Dispose();
  }

  // <summary>
  /// Внутренний метод для установки данных в регион
  /// </summary>
  internal void SetDataRegionInternal(
      ID3D12GraphicsCommandList* _commandList,
      void* _data,
      int _dataSize,
      uint _mipLevel,
      uint _arraySlice,
      uint _x, uint _y, uint _z,
      uint _width, uint _height, uint _depth)
  {
    var uploadManager = p_parentDevice.GetUploadManager();
    var subresource = GetSubresourceIndex(_mipLevel, _arraySlice);

    var regionDesc = new ResourceDesc
    {
      Dimension = Silk.NET.Direct3D12.ResourceDimension.Texture2D,
      Width = _width,
      Height = _height,
      DepthOrArraySize = (ushort)_depth,
      MipLevels = 1,
      Format = p_description.Format.Convert(),
      SampleDesc = new SampleDesc(1, 0),
      Layout = TextureLayout.LayoutUnknown,
      Flags = ResourceFlags.None
    };

    PlacedSubresourceFootprint* layouts = stackalloc PlacedSubresourceFootprint[1];
    ulong totalSize;

    p_device->GetCopyableFootprints(
        &regionDesc,
        0,
        1,
        0,
        layouts,
        null,
        null,
        &totalSize);

    var barrier = new ResourceBarrier
    {
      Type = ResourceBarrierType.Transition,
      Transition = new ResourceTransitionBarrier
      {
        PResource = p_resource,
        StateBefore = p_currentState,
        StateAfter = ResourceStates.CopyDest,
        Subresource = subresource
      }
    };
    _commandList->ResourceBarrier(1, &barrier);

    uploadManager.UploadTextureDataRegion(
        _commandList,
        p_resource,
        subresource,
        _data,
        (ulong)_dataSize,
        _x, _y, _z,
        layouts[0].Footprint.RowPitch);

    barrier.Transition.StateBefore = ResourceStates.CopyDest;
    barrier.Transition.StateAfter = p_currentState;
    _commandList->ResourceBarrier(1, &barrier);
  }

  /// <summary>
  /// Читать данные из staging текстуры
  /// </summary>
  internal void ReadDataFromStaging<T>(T[] _result, uint _mipLevel, uint _arraySlice) where T : unmanaged
  {
    if(p_description.Usage != ResourceUsage.Staging)
      throw new InvalidOperationException("ReadDataFromStaging only works with staging textures");

    var mappedPtr = MapTextureForReading(_mipLevel, _arraySlice);

    try
    {
      fixed(T* resultPtr = _result)
      {
        var copySize = _result.Length * sizeof(T);
        Buffer.MemoryCopy(mappedPtr.ToPointer(), resultPtr, copySize, copySize);
      }
    }
    finally
    {
      UnmapTexture(_mipLevel, _arraySlice);
    }
  }

  /// <summary>
  /// Map текстуру для чтения (только для staging)
  /// </summary>
  private IntPtr MapTextureForReading(uint _mipLevel, uint _arraySlice)
  {
    if(p_description.Usage != ResourceUsage.Staging)
      throw new InvalidOperationException("Only staging textures can be mapped");

    var subresource = GetSubresourceIndex(_mipLevel, _arraySlice);
    void* mappedData;
    var range = new Silk.NET.Direct3D12.Range { Begin = 0, End = 0 };

    var hr = p_resource->Map(subresource, &range, &mappedData);
    DX12Helpers.ThrowIfFailed(hr, "Failed to map texture for reading");

    return new IntPtr(mappedData);
  }

  /// <summary>
  /// Unmap текстуру
  /// </summary>
  private void UnmapTexture(uint _mipLevel, uint _arraySlice)
  {
    var subresource = GetSubresourceIndex(_mipLevel, _arraySlice);
    var range = new Silk.NET.Direct3D12.Range { Begin = 0, End = 0 };
    p_resource->Unmap(subresource, &range);
  }

  /// <summary>
  /// Внутренний метод для загрузки данных через command list
  /// </summary>
  internal void SetDataInternal(
      ID3D12GraphicsCommandList* _commandList,
      void* _data,
      int _dataSize,
      uint _mipLevel,
      uint _arraySlice)
  {
    var uploadManager = p_parentDevice.GetUploadManager();

    var subresource = GetSubresourceIndex(_mipLevel, _arraySlice);

    PlacedSubresourceFootprint* layouts = stackalloc PlacedSubresourceFootprint[1];
    ulong* rowSizes = stackalloc ulong[1];
    ulong totalSize;

    var desc = p_resource->GetDesc();
    p_device->GetCopyableFootprints(
        &desc,
        subresource,
        1,
        0,
        layouts,
        null,
        rowSizes,
        &totalSize);

    var expectedDataSize = CalculateSubresourceSize(_mipLevel, _arraySlice);

    if((ulong)_dataSize < expectedDataSize)
      throw new ArgumentException($"Data size ({_dataSize}) is less than required ({expectedDataSize})");

    var barrier = new ResourceBarrier
    {
      Type = ResourceBarrierType.Transition,
      Transition = new ResourceTransitionBarrier
      {
        PResource = p_resource,
        StateBefore = p_currentState,
        StateAfter = ResourceStates.CopyDest,
        Subresource = subresource
      }
    };
    _commandList->ResourceBarrier(1, &barrier);

    if((ulong)_dataSize == totalSize)
    {
      uploadManager.UploadTextureData(
          _commandList,
          p_resource,
          subresource,
          _data,
          (ulong)_dataSize,
          layouts[0].Footprint.RowPitch,
          0);
    }
    else
    {
      //TODO: Fix upload

      //UploadTextureDataWithPadding(
      //    _commandList,
      //    uploadManager,
      //    subresource,
      //    _data,
      //    _dataSize,
      //    layouts[0]);

      uploadManager.UploadTextureData(
        _commandList,
        p_resource,
        subresource,
        _data,
        (ulong)_dataSize,
        layouts[0].Footprint.RowPitch,
        0);
    }

    barrier.Transition.StateBefore = ResourceStates.CopyDest;
    barrier.Transition.StateAfter = p_currentState;
    _commandList->ResourceBarrier(1, &barrier);
  }

  /// <summary>
  /// Загрузка данных текстуры с добавлением padding'а для row pitch
  /// </summary>
  private void UploadTextureDataWithPadding(
      ID3D12GraphicsCommandList* _commandList,
      DX12UploadHeapManager _uploadManager,
      uint _subresource,
      void* _sourceData,
      int _sourceDataSize,
      PlacedSubresourceFootprint _layout)
  {
    var footprint = _layout.Footprint;
    var mipWidth = footprint.Width;
    var mipHeight = footprint.Height;
    var rowPitch = footprint.RowPitch;

    var formatSize = p_description.Format.GetFormatSize();
    var sourceRowSize = mipWidth * formatSize;

    if(sourceRowSize == rowPitch)
    {
      _uploadManager.UploadTextureData(
          _commandList,
          p_resource,
          _subresource,
          _sourceData,
          (ulong)_sourceDataSize,
          rowPitch,
          0);
    }
    else
    {
      var paddedSize = rowPitch * mipHeight;
      var uploadBuffer = _uploadManager.AcquireBuffer(paddedSize);

      try
      {
        var srcPtr = (byte*)_sourceData;
        var mappedPtr = (byte*)uploadBuffer.MappedData;

        for(uint row = 0; row < mipHeight; row++)
        {
          var srcOffset = row * sourceRowSize;
          var dstOffset = row * rowPitch;

          Buffer.MemoryCopy(
              srcPtr + srcOffset,
              mappedPtr + dstOffset,
              sourceRowSize,
              sourceRowSize);

          if(rowPitch > sourceRowSize)
          {
            var paddingSize = rowPitch - sourceRowSize;
            var pDest = new byte[paddingSize];
            Buffer.MemoryCopy(
                mappedPtr + dstOffset + sourceRowSize,
                &pDest,
                paddingSize,
                paddingSize);
          }
        }

        var destLocation = new TextureCopyLocation
        {
          PResource = p_resource,
          Type = TextureCopyType.SubresourceIndex,
          SubresourceIndex = _subresource
        };

        var srcLocation = new TextureCopyLocation
        {
          PResource = uploadBuffer.Resource,
          Type = TextureCopyType.PlacedFootprint,
          PlacedFootprint = _layout
        };

        _commandList->CopyTextureRegion(&destLocation, 0, 0, 0, &srcLocation, null);
      }
      finally
      {
        _uploadManager.ReleaseBuffer(uploadBuffer);
      }
    }
  }

  private void ValidateSubresource(uint _mipLevel, uint _arraySlice)
  {
    if(_mipLevel >= p_description.MipLevels)
      throw new ArgumentException($"Mip level {_mipLevel} exceeds texture mip count {p_description.MipLevels}");

    if(_arraySlice >= p_description.ArraySize)
      throw new ArgumentException($"Array slice {_arraySlice} exceeds texture array size {p_description.ArraySize}");
  }

  private void ValidateRegion(uint _mipLevel, uint _x, uint _y, uint _z, uint _width, uint _height, uint _depth)
  {
    var mipWidth = Math.Max(1u, p_description.Width >> (int)_mipLevel);
    var mipHeight = Math.Max(1u, p_description.Height >> (int)_mipLevel);
    var mipDepth = Math.Max(1u, p_description.Depth >> (int)_mipLevel);

    if(_x + _width > mipWidth)
      throw new ArgumentException($"Region extends beyond texture width at mip level {_mipLevel}");

    if(_y + _height > mipHeight)
      throw new ArgumentException($"Region extends beyond texture height at mip level {_mipLevel}");

    if(_z + _depth > mipDepth)
      throw new ArgumentException($"Region extends beyond texture depth at mip level {_mipLevel}");
  }
  private ulong CalculateSubresourceSize(uint _mipLevel, uint _arraySlice)
  {
    var width = Math.Max(1u, p_description.Width >> (int)_mipLevel);
    var height = Math.Max(1u, p_description.Height >> (int)_mipLevel);
    var depth = Math.Max(1u, p_description.Depth >> (int)_mipLevel);

    var formatSize = p_description.Format.GetFormatSize();
    return width * height * depth * formatSize;
  }

  private byte[] CreateClearPixelData(Vector4 _clearColor) => p_description.Format switch
  {
    TextureFormat.R8G8B8A8_UNORM => ClearColorValues(_clearColor),
    TextureFormat.R32G32B32A32_FLOAT => ClearColorBitConverter(_clearColor),
    _ => throw new ArgumentException($"Clear not supported for format: {p_description.Format}")
  };

  private byte[] ClearColorBitConverter(Vector4 _clearColor)
  {
    var result = new byte[16];
    BitConverter.GetBytes(_clearColor.X).CopyTo(result, 0);
    BitConverter.GetBytes(_clearColor.Y).CopyTo(result, 4);
    BitConverter.GetBytes(_clearColor.Z).CopyTo(result, 8);
    BitConverter.GetBytes(_clearColor.W).CopyTo(result, 12);
    return result;
  }

  private byte[] ClearColorValues(Vector4 _clearColor) => [
      (byte)(_clearColor.X * 255f),
      (byte)(_clearColor.Y * 255f),
      (byte)(_clearColor.Z * 255f),
      (byte)(_clearColor.W * 255f)
  ];

  private struct TextureViewKey: IEquatable<TextureViewKey>
  {
    public TextureViewType ViewType;
    public TextureViewDescription Description;

    public TextureViewKey(TextureViewType _viewType, TextureViewDescription _description)
    {
      ViewType = _viewType;
      Description = _description;
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
}
