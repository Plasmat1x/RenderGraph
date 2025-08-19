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

using System;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Directx12Impl;

/// <summary>
/// Обновленный DX12Buffer, наследующийся от DX12Resource
/// </summary>
public unsafe class DX12Buffer: DX12Resource, IBuffer
{
  private readonly D3D12 p_d3d12;
  private readonly BufferDescription p_description;
  private readonly DX12DescriptorHeapManager p_descriptorManager;
  private readonly DX12GraphicsDevice p_parentDevice;
  private readonly Dictionary<BufferViewKey, DX12BufferView> p_views = new();
  private void* p_mappedData;
  private bool p_isMappable;

  public DX12Buffer(ID3D12Device* _device, D3D12 _d3d12, BufferDescription _description, DX12DescriptorHeapManager _descriptorManager, DX12GraphicsDevice _parentDevice)
    : base(_device, _description.Name)
  {
    p_d3d12 = _d3d12;
    p_description = _description;
    p_descriptorManager = _descriptorManager;
    p_parentDevice = _parentDevice;

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
    else
    {
      readRange = new Silk.NET.Direct3D12.Range { Begin = 0, End = 0 }; // CPU не читает
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

    var range = new Silk.NET.Direct3D12.Range { Begin = 0, End = (nuint)Size };
    p_resource->Unmap(0, &range);
    p_mappedData = null;
  }

  /// <summary>
  /// Загрузить данные в буфер (соответствует интерфейсу IBuffer)
  /// </summary>
  public void SetData<T>(T[] _data, ulong _offset = 0) where T : unmanaged
  {
    if(_data == null || _data.Length == 0)
      throw new ArgumentException("Data cannot be null or empty");

    var dataSize = (ulong)(_data.Length * sizeof(T));
    if(_offset + dataSize > Size)
      throw new ArgumentException("Data size exceeds buffer size");

    // Для Dynamic буферов используем Map/Unmap
    if(p_description.Usage == ResourceUsage.Dynamic)
    {
      SetDataViaMappedMemory(_data, _offset);
    }
    // Для Default буферов используем upload через command list
    else if(p_description.Usage == ResourceUsage.Default)
    {
      SetDataViaUpload(_data, _offset);
    }
    else
    {
      throw new InvalidOperationException(
          $"SetData not supported for {p_description.Usage} buffers");
    }
  }

  public void SetData<T>(T _data, ulong _offset = 0) where T : unmanaged
  {
    SetData(new[] { _data }, _offset);
  }

  /// <summary>
  /// Получить данные из буфера (соответствует интерфейсу IBuffer)
  /// </summary>
  public T[] GetData<T>(ulong _offset, int _count = -1) where T : unmanaged
  {
    var elementSize = (ulong)sizeof(T);
    var maxElements = (p_description.Size - _offset) / elementSize;
    var elementsToRead = _count < 0 ? maxElements : (ulong)_count;

    if(elementsToRead > maxElements)
      throw new ArgumentException("Requested count exceeds buffer capacity");

    var result = new T[elementsToRead];

    if(p_description.Usage == ResourceUsage.Staging)
    {
      GetDataViaMappedMemory(result, _offset);
    }
    // Для остальных нужен readback
    else
    {
      GetDataViaReadback(result, _offset);
    }

    return result;
  }

  public T GetData<T>(ulong _offset = 0) where T : unmanaged
  {
    return GetData<T>(_offset, 1)[0];
  }

  /// <summary>
  /// Получить данные из буфера (только для читаемых буферов)
  /// </summary>
  public T[] GetData<T>(ID3D12GraphicsCommandList* _commandList, ulong _offset = 0, int _count = -1) where T : unmanaged
  {
    if(p_description.Usage != ResourceUsage.Staging &&
        (p_description.CPUAccessFlags & CPUAccessFlags.Read) == 0)
    {
      throw new InvalidOperationException("Buffer must be created with staging usage or read access to read data");
    }

    if(_count < 0)
      _count = (int)((Size - _offset) / (ulong)sizeof(T));

    var data = new T[_count];
    fixed(T* pData = data)
    {
      GetDataInternal(_commandList, pData, (ulong)(_count * sizeof(T)), _offset);
    }
    return data;
  }

  public override ulong GetMemorySize()
  {
    return p_description.Size;
  }

  /// <summary>
  /// Внутренний метод для загрузки данных через command list
  /// Используется только внутри DirectX12 реализации
  /// </summary>
  internal void SetDataInternal(ID3D12GraphicsCommandList* _commandList, void* _data, ulong _dataSize, ulong _offset)
  {
    // Переводим буфер в состояние копирования
    var barrier = new ResourceBarrier
    {
      Type = ResourceBarrierType.Transition,
      Transition = new ResourceTransitionBarrier
      {
        PResource = p_resource,
        StateBefore = p_currentState,
        StateAfter = ResourceStates.CopyDest,
        Subresource = D3D12.ResourceBarrierAllSubresources
      }
    };
    _commandList->ResourceBarrier(1, &barrier);

    // Используем upload manager из device
    var uploadManager = p_parentDevice.GetUploadManager();
    uploadManager.UploadBufferData(_commandList, p_resource, _offset, _data, _dataSize);

    // Возвращаем в исходное состояние
    barrier.Transition.StateBefore = ResourceStates.CopyDest;
    barrier.Transition.StateAfter = p_currentState;
    _commandList->ResourceBarrier(1, &barrier);
  }

  private void GetDataInternal(ID3D12GraphicsCommandList* _commandList, void* _data, ulong _dataSize, ulong _offset)
  {
    // TODO: Implement readback using staging buffer
    throw new NotImplementedException("Buffer readback not yet implemented");
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

  /// <summary>
  /// Очистить буфер нулями
  /// </summary>
  public void Clear()
  {
    var zeroData = new byte[p_description.Size];
    SetData(zeroData);
  }

  /// <summary>
  /// Проверить, можно ли мапить буфер
  /// </summary>
  public bool CanMap()
  {
    return p_description.Usage == ResourceUsage.Dynamic ||
           p_description.Usage == ResourceUsage.Staging;
  }

  /// <summary>
  /// Проверить, поддерживает ли буфер upload операции
  /// </summary>
  public bool SupportsUpload()
  {
    return p_description.Usage != ResourceUsage.Staging;
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

  /// <summary>
  /// Установить данные через mapped memory (для Dynamic буферов)
  /// </summary>
  private void SetDataViaMappedMemory<T>(T[] _data, ulong _offset) where T : unmanaged
  {
    var mappedPtr = p_parentDevice.MapBuffer(this, MapMode.WriteDiscard);

    try
    {
      fixed(T* dataPtr = _data)
      {
        var destPtr = (byte*)mappedPtr.ToPointer() + _offset;
        var sourcePtr = (byte*)dataPtr;
        var copySize = _data.Length * sizeof(T);

        Buffer.MemoryCopy(sourcePtr, destPtr, copySize, copySize);
      }
    }
    finally
    {
      p_parentDevice.UnmapBuffer(this);
    }
  }


  /// <summary>
  /// Получить данные через mapped memory
  /// </summary>
  private void GetDataViaMappedMemory<T>(T[] _result, ulong _offset) where T : unmanaged
  {
    var mappedPtr = p_parentDevice.MapBuffer(this, MapMode.Read);

    try
    {
      fixed(T* resultPtr = _result)
      {
        var sourcePtr = (byte*)mappedPtr.ToPointer() + _offset;
        var destPtr = (byte*)resultPtr;
        var copySize = _result.Length * sizeof(T);

        Buffer.MemoryCopy(sourcePtr, destPtr, copySize, copySize);
      }
    }
    finally
    {
      p_parentDevice.UnmapBuffer(this);
    }
  }

  /// <summary>
  /// Получить данные через readback staging buffer
  /// </summary>
  private void GetDataViaReadback<T>(T[] _result, ulong _offset) where T : unmanaged
  {
    var readbackSize = (ulong)(_result.Length * sizeof(T));

    // Создаем временный staging buffer
    var stagingDesc = new BufferDescription
    {
      Name = $"{Name}_ReadbackStaging",
      Size = readbackSize,
      Usage = ResourceUsage.Staging,
      CPUAccessFlags = CPUAccessFlags.Read,
      BufferUsage = BufferUsage.Staging
    };

    using var stagingBuffer = (DX12Buffer)p_parentDevice.CreateBuffer(stagingDesc);

    // Копируем данные в staging buffer через command list
    p_parentDevice.CopyBufferToStaging(this, stagingBuffer, _offset, 0, readbackSize);

    // Читаем из staging buffer
    stagingBuffer.GetDataViaMappedMemory(_result, 0);
  }

  /// <summary>
  /// Установить данные через upload (для Default буферов)
  /// </summary>
  private void SetDataViaUpload<T>(T[] _data, ulong _offset) where T : unmanaged
  {
    p_parentDevice.UploadBufferData(this, _data, _offset);
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