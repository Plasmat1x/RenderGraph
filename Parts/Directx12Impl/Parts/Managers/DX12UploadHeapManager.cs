// <summary>
using Directx12Impl.Parts;
using Directx12Impl.Parts.Utils;

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
  private const int MAX_POOLED_BUFFERS = 32;

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
    if(_requiredSize == 0)
      throw new ArgumentException("Required size cannot be zero");

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
      if(p_availableBuffers.Count < MAX_POOLED_BUFFERS)
      {
        p_availableBuffers.Add(_buffer);
      }
      else
      {
        _buffer.Dispose();
        p_uploadBuffers.Remove(_buffer);
      }
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
    if(_commandList == null)
      throw new ArgumentNullException(nameof(_commandList), "Command list cannot be null");

    if(_destinationBuffer == null)
      throw new ArgumentNullException(nameof(_destinationBuffer), "Destination buffer cannot be null");

    if(_data == null)
      throw new ArgumentNullException(nameof(_data), "Data pointer cannot be null");

    if(_dataSize == 0)
      throw new ArgumentException("Data size cannot be zero");

    var uploadBuffer = AcquireBuffer(_dataSize);
    if(uploadBuffer == null)
      throw new InvalidOperationException("Failed to acquire upload buffer");

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
    catch(Exception ex)
    {
      throw new InvalidOperationException($"Failed to upload buffer data: {ex.Message}", ex);
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
    var riid = ID3D12Resource.Guid;
    var hr = p_device->CreateCommittedResource(
        &heapProps,
        HeapFlags.None,
        &bufferDesc,
        ResourceStates.GenericRead,
        null,
        &riid,
        (void**)&resource);

    DX12Helpers.ThrowIfFailed(hr, "Failed to create upload buffer");

    var buffer = new DX12UploadBuffer(resource, _size);
    lock(p_lock)
    {
      p_uploadBuffers.Add(buffer);
    }
    return buffer;
  }

  /// <summary>
  /// Загрузить данные в регион текстуры
  /// </summary>
  public void UploadTextureDataRegion(
      ID3D12GraphicsCommandList* _commandList,
      ID3D12Resource* _destinationTexture,
      uint _subresource,
      void* _data,
      ulong _dataSize,
      uint _x, uint _y, uint _z,
      uint _rowPitch)
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

      var srcBox = new Box
      {
        Left = 0,
        Top = 0,
        Front = 0,
        Right = srcLocation.PlacedFootprint.Footprint.Width,
        Bottom = srcLocation.PlacedFootprint.Footprint.Height,
        Back = 1
      };

      _commandList->CopyTextureRegion(&destLocation, _x, _y, _z, &srcLocation, &srcBox);
    }
    finally
    {
      ReleaseBuffer(uploadBuffer);
    }
  }

  /// <summary>
  /// Пакетная загрузка множественных регионов
  /// </summary>
  public void UploadMultipleTextureRegions(
      ID3D12GraphicsCommandList* _commandList,
      TextureUploadBatch[] _uploads)
  {
    if(_uploads == null || _uploads.Length == 0)
      return;

    // Вычисляем общий размер всех данных
    ulong totalSize = 0;
    foreach(var upload in _uploads)
    {
      totalSize += upload.DataSize;
    }

    var uploadBuffer = AcquireBuffer(totalSize);

    try
    {
      ulong currentOffset = 0;

      foreach(var upload in _uploads)
      {
        uploadBuffer.WriteData(upload.Data, upload.DataSize, currentOffset);

        var destLocation = new TextureCopyLocation
        {
          PResource = upload.DestinationTexture,
          Type = TextureCopyType.SubresourceIndex,
          SubresourceIndex = upload.Subresource
        };

        var srcLocation = new TextureCopyLocation
        {
          PResource = uploadBuffer.Resource,
          Type = TextureCopyType.PlacedFootprint,
          PlacedFootprint = new PlacedSubresourceFootprint
          {
            Offset = currentOffset,
            Footprint = upload.Footprint
          }
        };

        var srcBox = new Box
        {
          Left = 0,
          Top = 0,
          Front = 0,
          Right = upload.Width,
          Bottom = upload.Height,
          Back = upload.Depth
        };

        _commandList->CopyTextureRegion(&destLocation,
            upload.X, upload.Y, upload.Z, &srcLocation, &srcBox);

        currentOffset += upload.DataSize;
      }
    }
    finally
    {
      ReleaseBuffer(uploadBuffer);
    }
  }

  /// <summary>
  /// Оптимизированная загрузка для мип-цепей
  /// </summary>
  public void UploadMipChain(
      ID3D12GraphicsCommandList* _commandList,
      ID3D12Resource* _destinationTexture,
      MipLevelData[] _mipData)
  {
    if(_mipData == null || _mipData.Length == 0)
      return;

    // Вычисляем общий размер всех мип-уровней
    ulong totalSize = 0;
    foreach(var mip in _mipData)
    {
      totalSize += mip.DataSize;
    }

    var uploadBuffer = AcquireBuffer(totalSize);

    try
    {
      ulong currentOffset = 0;

      for(uint mipLevel = 0; mipLevel < _mipData.Length; mipLevel++)
      {
        var mip = _mipData[mipLevel];

        // Записываем данные мип-уровня
        uploadBuffer.WriteData(mip.Data, mip.DataSize, currentOffset);

        // Для каждого array slice
        for(uint arraySlice = 0; arraySlice < mip.ArraySize; arraySlice++)
        {
          var subresource = CalculateSubresourceIndex(mipLevel, arraySlice, (uint)_mipData.Length);

          var destLocation = new TextureCopyLocation
          {
            PResource = _destinationTexture,
            Type = TextureCopyType.SubresourceIndex,
            SubresourceIndex = subresource
          };

          var srcLocation = new TextureCopyLocation
          {
            PResource = uploadBuffer.Resource,
            Type = TextureCopyType.PlacedFootprint,
            PlacedFootprint = new PlacedSubresourceFootprint
            {
              Offset = currentOffset + arraySlice * mip.SlicePitch,
              Footprint = new SubresourceFootprint
              {
                Format = mip.Format,
                Width = mip.Width,
                Height = mip.Height,
                Depth = mip.Depth,
                RowPitch = mip.RowPitch
              }
            }
          };

          _commandList->CopyTextureRegion(&destLocation, 0, 0, 0, &srcLocation, null);
        }

        currentOffset += mip.DataSize;
      }
    }
    finally
    {
      ReleaseBuffer(uploadBuffer);
    }
  }

  /// <summary>
  /// Создать оптимальный upload buffer для конкретной операции
  /// </summary>
  public DX12UploadBuffer AcquireOptimalBuffer(ulong _requiredSize, UploadBufferType _type)
  {
    lock(p_lock)
    {
      var suitableBuffers = p_availableBuffers.Where(_b =>
          _b.Size >= _requiredSize &&
          _b.Type == _type).OrderBy(_b => _b.Size);

      var buffer = suitableBuffers.FirstOrDefault();
      if(buffer != null)
      {
        p_availableBuffers.Remove(buffer);
        buffer.Reset();
        return buffer;
      }

      var optimalSize = GetOptimalBufferSize(_requiredSize, _type);
      return CreateUploadBuffer(optimalSize, _type);
    }
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


  // === Вспомогательные методы ===

  private ulong GetOptimalBufferSize(ulong _requiredSize, UploadBufferType _type)
  {
    return _type switch
    {
      UploadBufferType.Small => Math.Max(_requiredSize, SMALL_BUFFER_SIZE),
      UploadBufferType.Medium => Math.Max(_requiredSize, MEDIUM_BUFFER_SIZE),
      UploadBufferType.Large => Math.Max(_requiredSize, LARGE_BUFFER_SIZE),
      UploadBufferType.Texture => AlignToTextureRequirements(_requiredSize),
      UploadBufferType.Buffer => AlignToBufferRequirements(_requiredSize),
      _ => Math.Max(_requiredSize, MEDIUM_BUFFER_SIZE)
    };
  }

  private ulong AlignToTextureRequirements(ulong _size)
  {
    const ulong TEXTURE_ALIGNMENT = 512;
    return (_size + TEXTURE_ALIGNMENT - 1) & ~(TEXTURE_ALIGNMENT - 1);
  }

  private ulong AlignToBufferRequirements(ulong _size)
  {
    const ulong BUFFER_ALIGNMENT = 256;
    return (_size + BUFFER_ALIGNMENT - 1) & ~(BUFFER_ALIGNMENT - 1);
  }

  private uint CalculateSubresourceIndex(uint _mipLevel, uint _arraySlice, uint _mipLevels)
  {
    return _mipLevel + _arraySlice * _mipLevels;
  }

  private DX12UploadBuffer CreateUploadBuffer(ulong _size, UploadBufferType _type)
  {
    var buffer = CreateUploadBuffer(_size);
    buffer.Type = _type;
    return buffer;
  }
}