// <summary>
using Directx12Impl.Parts.Utils;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

/// Менеджер upload heap для загрузки данных на GPU
/// </summary>
public unsafe class DX12UploadHeapManager: IDisposable
{
  private readonly ID3D12Device* p_device;
  private readonly List<DX12UploadBuffer> p_uploadBuffers = new();
  private readonly List<DX12UploadBuffer> p_availableBuffers = new();
  private readonly object p_lock = new();
  private bool p_disposed;

  // Размеры пулов
  private const ulong SMALL_BUFFER_SIZE = 64 * 1024;      // 64 KB
  private const ulong MEDIUM_BUFFER_SIZE = 1024 * 1024;   // 1 MB  
  private const ulong LARGE_BUFFER_SIZE = 16 * 1024 * 1024; // 16 MB

  public DX12UploadHeapManager(ID3D12Device* _device)
  {
    p_device = _device;

    // Предварительно создаем несколько буферов разных размеров
    for(int i = 0; i < 4; i++)
      CreateUploadBuffer(SMALL_BUFFER_SIZE);
    for(int i = 0; i < 2; i++)
      CreateUploadBuffer(MEDIUM_BUFFER_SIZE);
    CreateUploadBuffer(LARGE_BUFFER_SIZE);
  }

  /// <summary>
  /// Получить upload buffer подходящего размера
  /// </summary>
  public DX12UploadBuffer AcquireBuffer(ulong _requiredSize)
  {
    lock(p_lock)
    {
      foreach(var buffer in p_availableBuffers)
      {
        if(buffer.Size >= _requiredSize)
        {
          p_availableBuffers.Remove(buffer);
          buffer.Reset();
          return buffer;
        }
      }

      var bufferSize = GetOptimalBufferSize(_requiredSize);
      return CreateUploadBuffer(bufferSize);
    }
  }

  /// <summary>
  /// Вернуть буфер в пул
  /// </summary>
  public void ReleaseBuffer(DX12UploadBuffer _buffer)
  {
    if(_buffer == null)
      return;

    lock(p_lock)
    {
      _buffer.Reset();
      p_availableBuffers.Add(_buffer);
    }
  }

  /// <summary>
  /// Загрузить данные в буфер назначения
  /// </summary>
  public void UploadBufferData(
      ID3D12GraphicsCommandList* _commandList,
      ID3D12Resource* _destinationBuffer,
      ulong _destinationOffset,
      void* _data,
      ulong _dataSize)
  {
    var uploadBuffer = AcquireBuffer(_dataSize);

    try
    {
      uploadBuffer.WriteData(_data, _dataSize, 0);

      _commandList->CopyBufferRegion(
          _destinationBuffer,
          _destinationOffset,
          uploadBuffer.Resource,
          0,
          _dataSize);
    }
    finally
    {
      ReleaseBuffer(uploadBuffer);
    }
  }

  /// <summary>
  /// Загрузить данные текстуры
  /// </summary>
  public void UploadTextureData(
      ID3D12GraphicsCommandList* _commandList,
      ID3D12Resource* _destinationTexture,
      uint _subresource,
      void* _data,
      ulong _dataSize,
      uint _rowPitch,
      uint _slicePitch)
  {
    var uploadBuffer = AcquireBuffer(_dataSize);

    try
    {
      uploadBuffer.WriteData(_data, _dataSize, 0);

      var destLocation = new TextureCopyLocation
      {
        PResource = _destinationTexture,
        Type = TextureCopyType.SubresourceIndex,
        SubresourceIndex = _subresource
      };

      var srcLocation = new TextureCopyLocation
      {
        PResource = uploadBuffer.Resource,
        Type = TextureCopyType.PlacedFootprint,
        PlacedFootprint = new PlacedSubresourceFootprint
        {
          Offset = 0,
          Footprint = new SubresourceFootprint
          {
            Format = GetTextureFormat(_destinationTexture),
            Width = GetTextureWidth(_destinationTexture),
            Height = GetTextureHeight(_destinationTexture),
            Depth = 1,
            RowPitch = _rowPitch
          }
        }
      };

      _commandList->CopyTextureRegion(&destLocation, 0, 0, 0, &srcLocation, null);
    }
    finally
    {
      ReleaseBuffer(uploadBuffer);
    }
  }

  private DX12UploadBuffer CreateUploadBuffer(ulong _size)
  {
    var heapProps = new HeapProperties
    {
      Type = HeapType.Upload,
      CPUPageProperty = CpuPageProperty.Unknown,
      MemoryPoolPreference = MemoryPool.Unknown,
      CreationNodeMask = 0,
      VisibleNodeMask = 0
    };

    var bufferDesc = new ResourceDesc
    {
      Dimension = ResourceDimension.Buffer,
      Alignment = 0,
      Width = _size,
      Height = 1,
      DepthOrArraySize = 1,
      MipLevels = 1,
      Format = Silk.NET.DXGI.Format.FormatUnknown,
      SampleDesc = new Silk.NET.DXGI.SampleDesc { Count = 1, Quality = 0 },
      Layout = TextureLayout.LayoutRowMajor,
      Flags = ResourceFlags.None
    };

    ID3D12Resource* resource;
    var hr = p_device->CreateCommittedResource(
        &heapProps,
        HeapFlags.None,
        &bufferDesc,
        ResourceStates.GenericRead,
        null,
        SilkMarshal.GuidPtrOf<ID3D12Resource>(),
        (void**)&resource);

    DX12Helpers.ThrowIfFailed(hr, "Failed to create upload buffer");

    var buffer = new DX12UploadBuffer(resource, _size);
    p_uploadBuffers.Add(buffer);
    return buffer;
  }

  private ulong GetOptimalBufferSize(ulong _requiredSize)
  {
    if(_requiredSize <= SMALL_BUFFER_SIZE)
      return SMALL_BUFFER_SIZE;
    if(_requiredSize <= MEDIUM_BUFFER_SIZE)
      return MEDIUM_BUFFER_SIZE;
    if(_requiredSize <= LARGE_BUFFER_SIZE)
      return LARGE_BUFFER_SIZE;

    return ((_requiredSize + 1024 * 1024 - 1) / (1024 * 1024)) * (1024 * 1024);
  }

  private Silk.NET.DXGI.Format GetTextureFormat(ID3D12Resource* _texture)
  {
    var desc = _texture->GetDesc();
    return desc.Format;
  }

  private uint GetTextureWidth(ID3D12Resource* _texture)
  {
    var desc = _texture->GetDesc();
    return (uint)desc.Width;
  }

  private uint GetTextureHeight(ID3D12Resource* _texture)
  {
    var desc = _texture->GetDesc();
    return desc.Height;
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    lock(p_lock)
    {
      foreach(var buffer in p_uploadBuffers)
      {
        buffer.Dispose();
      }
      p_uploadBuffers.Clear();
      p_availableBuffers.Clear();
    }

    p_disposed = true;
  }
}