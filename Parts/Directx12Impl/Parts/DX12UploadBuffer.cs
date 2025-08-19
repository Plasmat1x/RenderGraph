using Directx12Impl.Parts;
using Directx12Impl.Parts.Utils;

using Silk.NET.Direct3D12;

/// <summary>
/// Upload buffer для временного хранения данных
/// </summary>
public unsafe class DX12UploadBuffer: IDisposable
{
  public ID3D12Resource* Resource { get; }
  public ulong Size { get; }
  public void* MappedData { get; private set; }
  public UploadBufferType Type { get; set; } = UploadBufferType.Medium;

  private bool p_disposed;

  public DX12UploadBuffer(ID3D12Resource* _resource, ulong _size)
  {
    Resource = _resource;
    Size = _size;

    var readRange = new Silk.NET.Direct3D12.Range { Begin = 0, End = 0 };
    void* mappedData;
    var hr = _resource->Map(0, &readRange, &mappedData);
    DX12Helpers.ThrowIfFailed(hr, "Failed to map upload buffer");
    MappedData = mappedData;
  }

  public void WriteData(void* _data, ulong _dataSize, ulong _offset = 0)
  {
    if(_offset + _dataSize > Size)
      throw new ArgumentException("Data size exceeds buffer size");

    var dst = (byte*)MappedData + _offset;
    Buffer.MemoryCopy(_data, dst, Size - _offset, _dataSize);
  }

  public void WriteData<T>(T[] _data, ulong _offset = 0) where T : unmanaged
  {
    fixed(T* pData = _data)
    {
      WriteData(pData, (ulong)(_data.Length * sizeof(T)), _offset);
    }
  }

  public void WriteData<T>(T _data, ulong _offset = 0) where T : unmanaged
  {
    WriteData(&_data, (ulong)sizeof(T), _offset);
  }

  /// <summary>
  /// Записать выровненные данные
  /// </summary>
  public void WriteAlignedData(void* _data, ulong _dataSize, ulong _offset, uint _alignment)
  {
    var alignedOffset = (_offset + _alignment - 1) & ~((ulong)_alignment - 1);

    if(alignedOffset + _dataSize > Size)
      throw new ArgumentException("Aligned data exceeds buffer size");

    WriteData(_data, _dataSize, alignedOffset);
  }

  /// <summary>
  /// Получить выровненный offset для следующей записи
  /// </summary>
  public ulong GetAlignedOffset(ulong _currentOffset, uint _alignment)
  {
    return (_currentOffset + _alignment - 1) & ~((ulong)_alignment - 1);
  }

  public void Reset()
  {
    // Можно очистить буфер если нужно
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    if(Resource != null)
    {
      if(MappedData != null)
      {
        Resource->Unmap(0, null);
        MappedData = null;
      }
      Resource->Release();
    }

    p_disposed = true;
  }
}