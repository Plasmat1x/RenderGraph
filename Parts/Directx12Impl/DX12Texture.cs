using Directx12Impl.Extensions;
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

using static System.Runtime.InteropServices.JavaScript.JSType;

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

  public void SetData<T>(T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    if(_data == null || _data.Length == 0)
      throw new ArgumentException("Data cannot be null or empty");

    // Используем device для загрузки
    p_parentDevice.UploadTextureData(this, _data, _mipLevel, _arraySlice);
  }

  public T[] GetData<T>(uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    // Реализация чтения данных из текстуры
    throw new NotImplementedException("Texture data readback not implemented yet");
  }

  public uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice, uint _planeSlice = 0)
  {
    return _mipLevel + _arraySlice * p_description.MipLevels + _planeSlice * p_description.MipLevels * p_description.ArraySize;
  }

  public uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice)
  {
    throw new NotImplementedException();
  }

  public void GenerateMips()
  {
    // Реализация генерации мип-мапов
    throw new NotImplementedException("Mip generation not implemented yet");
  }

  public override ulong GetMemorySize()
  {
    return p_description.GetMemorySize();
  }

  // === DX12-specific overrides ===
  public override bool SupportsState(ResourceStates _state)
  {
    // Проверяем, поддерживает ли текстура указанное состояние
    var bindFlags = p_description.BindFlags;

    return _state switch
    {
      ResourceStates.RenderTarget => (bindFlags & BindFlags.RenderTarget) != 0,
      ResourceStates.DepthWrite or ResourceStates.DepthRead => (bindFlags & BindFlags.DepthStencil) != 0,
      ResourceStates.AllShaderResource => (bindFlags & BindFlags.ShaderResource) != 0,
      ResourceStates.UnorderedAccess => (bindFlags & BindFlags.UnorderedAccess) != 0,
      ResourceStates.CopyDest or ResourceStates.CopySource => true, // Все текстуры поддерживают копирование
      _ => true
    };
  }

  private void CreateTextureResource()
  {
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
      Layout = TextureLayout.LayoutUnknown,
      Flags = GetResourceFlags()
    };

    var heapProperties = new HeapProperties
    {
      Type = HeapType.Default,
      CPUPageProperty = CpuPageProperty.Unknown,
      MemoryPoolPreference = MemoryPool.Unknown
    };

    var initialState = GetInitialResourceState();

    fixed(ID3D12Resource** ppResource = &p_resource)
    fixed(Guid* pGuid = &ID3D12Resource.Guid)
    {
      HResult hr = p_device->CreateCommittedResource(
        &heapProperties,
        HeapFlags.None,
        &resourceDesc,
        initialState,
        null,
        pGuid,
        (void**)ppResource);


      DX12Helpers.ThrowIfFailed(hr, "Failed to create texture resource");
    }

    p_currentState = initialState;

    // Установим имя ресурса для отладки
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
    ResourceFlags flags = ResourceFlags.None;

    if((p_description.BindFlags & BindFlags.RenderTarget) != 0)
      flags |= ResourceFlags.AllowRenderTarget;

    if((p_description.BindFlags & BindFlags.DepthStencil) != 0)
      flags |= ResourceFlags.AllowDepthStencil;

    if((p_description.BindFlags & BindFlags.UnorderedAccess) != 0)
      flags |= ResourceFlags.AllowUnorderedAccess;

    // Если нет shader resource binding, можно запретить shader resource
    if((p_description.BindFlags & BindFlags.ShaderResource) == 0)
      flags |= ResourceFlags.DenyShaderResource;

    return flags;
  }

  private ResourceStates GetInitialResourceState()
  {
    // Выбираем начальное состояние на основе usage
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

  public override void Dispose()
  {
    if(!p_disposed)
    {
      // Освобождаем все представления
      foreach(var view in p_views.Values)
      {
        view.Dispose();
      }
      p_views.Clear();
    }

    base.Dispose();
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

    // Получаем layout информацию для subresource
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

    if((ulong)_dataSize < totalSize)
      throw new ArgumentException($"Data size ({_dataSize}) is less than required ({totalSize})");

    // Переводим текстуру в состояние копирования
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

    // Загружаем данные через upload heap
    uploadManager.UploadTextureData(
        _commandList,
        p_resource,
        subresource,
        _data,
        (ulong)_dataSize,
        layouts[0].Footprint.RowPitch,
        0);

    // Возвращаем в исходное состояние
    barrier.Transition.StateBefore = ResourceStates.CopyDest;
    barrier.Transition.StateAfter = p_currentState;
    _commandList->ResourceBarrier(1, &barrier);
  }

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
