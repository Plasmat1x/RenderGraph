using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Directx12Impl;

/// <summary>
/// Обновленный DX12Buffer, наследующийся от DX12Resource
/// </summary>
public unsafe class DX12Buffer: DX12Resource, IBuffer
{
  private readonly D3D12 p_d3d12;
  private readonly BufferDescription p_description;
  private readonly DX12DescriptorHeapManager p_descriptorManager;
  private readonly Dictionary<BufferViewKey, DX12BufferView> p_views = new();
  private void* p_mappedData;

  public DX12Buffer(ID3D12Device* _device, D3D12 _d3d12, BufferDescription _description, DX12DescriptorHeapManager _descriptorManager)
    : base(_device, _description.Name)
  {
    p_d3d12 = _d3d12;
    p_description = _description;
    p_descriptorManager = _descriptorManager;

    CreateBufferResource();
  }

  // === IBuffer implementation ===
  public BufferDescription Description => p_description;
  public override ResourceType ResourceType => ResourceType.Buffer;
  public ulong Size => p_description.Size;
  public uint Stride => p_description.Stride;
  public BufferUsage Usage => p_description.BufferUsage;
  public bool IsMapped => p_mappedData != null;

  public IBufferView CreateView(BufferViewDescription _description)
  {
    var key = new BufferViewKey(_description);

    if(!p_views.TryGetValue(key, out var view))
    {
      view = new DX12BufferView(this, _description, p_descriptorManager);
      p_views[key] = view;
    }

    return view;
  }

  public IBufferView GetDefaultShaderResourceView()
  {
    return CreateView(new BufferViewDescription
    {
      FirstElement = 0,
      NumElements = p_description.Size / Math.Max(p_description.Stride, 1),
      Flags = BufferViewFlags.None
    });
  }

  public IBufferView GetDefaultUnorderedAccessView()
  {
    return CreateView(new BufferViewDescription
    {
      FirstElement = 0,
      NumElements = p_description.Size / Math.Max(p_description.Stride, 1),
      Flags = BufferViewFlags.None
    });
  }

  public IntPtr Map(MapMode _mode)
  {
    if(p_mappedData != null)
      throw new InvalidOperationException("Buffer is already mapped");

    Silk.NET.Direct3D12.Range readRange = default;
    if(_mode == MapMode.Read || _mode == MapMode.ReadWrite)
    {
      readRange.Begin = 0;
      readRange.End = (nuint)p_description.Size;
    }

    fixed(void** ppMappedData = &p_mappedData)
    {
      HResult hr = p_resource->Map(0, &readRange, ppMappedData);
      DX12Helpers.ThrowIfFailed(hr, "Failed to map buffer");
    }

    return new IntPtr(p_mappedData);
  }

  public void Unmap()
  {
    if(p_mappedData == null)
      throw new InvalidOperationException("Buffer is not mapped");

    p_resource->Unmap(0, null);
    p_mappedData = null;
  }

  public void SetData<T>(T[] _data, ulong _offset = 0) where T : unmanaged
  {
    var dataPtr = Map(MapMode.Write);
    try
    {
      var size = _data.Length * sizeof(T);
      var destPtr = new IntPtr(dataPtr.ToInt64() + (long)_offset);

      fixed(T* srcPtr = _data)
      {
        Buffer.MemoryCopy(srcPtr, destPtr.ToPointer(), size, size);
      }
    }
    finally
    {
      Unmap();
    }
  }

  public void SetData<T>(T _data, ulong _offset = 0) where T : unmanaged
  {
    SetData(new[] { _data }, _offset);
  }

  public T[] GetData<T>(ulong _offset = 0, ulong _count = 0) where T : unmanaged
  {
    if(_count == 0)
      _count = (p_description.Size - _offset) / (ulong)sizeof(T);

    var result = new T[_count];
    var dataPtr = Map(MapMode.Read);
    try
    {
      var srcPtr = new IntPtr(dataPtr.ToInt64() + (long)_offset);

      fixed(T* destPtr = result)
      {
        var size = (int)(_count * (ulong)sizeof(T));
        Buffer.MemoryCopy(srcPtr.ToPointer(), destPtr, size, size);
      }
    }
    finally
    {
      Unmap();
    }

    return result;
  }

  public T GetData<T>(ulong _offset = 0) where T : unmanaged
  {
    return GetData<T>(_offset, 1)[0];
  }

  public override ulong GetMemorySize()
  {
    return p_description.Size;
  }

  private void CreateBufferResource()
  {
    var resourceDesc = new ResourceDesc
    {
      Dimension = Silk.NET.Direct3D12.ResourceDimension.Buffer,
      Alignment = 0,
      Width = p_description.Size,
      Height = 1,
      DepthOrArraySize = 1,
      MipLevels = 1,
      Format = Silk.NET.DXGI.Format.FormatUnknown,
      SampleDesc = new SampleDesc(1, 0),
      Layout = TextureLayout.LayoutRowMajor,
      Flags = GetResourceFlags()
    };

    var heapType = p_description.Usage switch
    {
      ResourceUsage.Default => HeapType.Default,
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
        (void**)&ppResource);

      DX12Helpers.ThrowIfFailed(hr, "Failed to create buffer resource");
    }

    p_currentState = initialState;

    // Установим имя ресурса для отладки
    if(!string.IsNullOrEmpty(p_name))
    {
      DX12Helpers.SetResourceName(p_resource, p_name);
    }
  }

  private ResourceFlags GetResourceFlags()
  {
    ResourceFlags flags = ResourceFlags.None;

    if((p_description.BindFlags & BindFlags.UnorderedAccess) != 0)
      flags |= ResourceFlags.AllowUnorderedAccess;

    return flags;
  }

  private ResourceStates GetInitialResourceState()
  {
    return p_description.Usage switch
    {
      ResourceUsage.Dynamic => ResourceStates.GenericRead,
      ResourceUsage.Staging => ResourceStates.CopyDest,
      _ => ResourceStates.Common
    };
  }

  public override void Dispose()
  {
    if(!p_disposed)
    {
      // Убеждаемся, что буфер не замаплен
      if(p_mappedData != null)
      {
        Unmap();
      }

      // Освобождаем все представления
      foreach(var view in p_views.Values)
      {
        view.Dispose();
      }
      p_views.Clear();
    }

    base.Dispose();
  }

  private struct BufferViewKey: IEquatable<BufferViewKey>
  {
    public BufferViewDescription Description;

    public BufferViewKey(BufferViewDescription _description)
    {
      Description = _description;
    }

    public bool Equals(BufferViewKey _other)
    {
      return Description.FirstElement == _other.Description.FirstElement &&
             Description.NumElements == _other.Description.NumElements &&
             Description.Flags == _other.Description.Flags;
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Description.FirstElement, Description.NumElements, Description.Flags);
    }
  }
}