using GraphicsAPI.Enums;

using Resources.Enums;

namespace GraphicsAPI.Descriptions;

/// <summary>
/// Обновленное описание буферных представлений для DX12 совместимости
/// </summary>
public class BufferViewDescription
{
  public BufferViewType ViewType { get; set; }
  public TextureFormat Format { get; set; } = TextureFormat.Unknown;
  public ulong FirstElement { get; set; } = 0;
  public ulong NumElements { get; set; } = 0;
  public uint StructureByteStride { get; set; } = 0;
  public BufferViewFlags Flags { get; set; } = BufferViewFlags.None;

  // Для UAV
  public ulong CounterOffsetInBytes { get; set; } = 0;

  /// <summary>
  /// Создать описание для Constant Buffer View
  /// </summary>
  public static BufferViewDescription CreateCBV(ulong _sizeInBytes, ulong _offsetInBytes = 0)
  {
    return new BufferViewDescription
    {
      ViewType = BufferViewType.ConstantBuffer,
      FirstElement = _offsetInBytes,
      NumElements = _sizeInBytes,
      Format = TextureFormat.Unknown
    };
  }

  /// <summary>
  /// Создать описание для Structured Buffer SRV
  /// </summary>
  public static BufferViewDescription CreateStructuredSRV(ulong _numElements, uint _stride, ulong _firstElement = 0)
  {
    return new BufferViewDescription
    {
      ViewType = BufferViewType.ShaderResource,
      Format = TextureFormat.Unknown,
      FirstElement = _firstElement,
      NumElements = _numElements,
      StructureByteStride = _stride
    };
  }

  /// <summary>
  /// Создать описание для Raw Buffer SRV
  /// </summary>
  public static BufferViewDescription CreateRawSRV(ulong _sizeInBytes, ulong _offsetInBytes = 0)
  {
    return new BufferViewDescription
    {
      ViewType = BufferViewType.ShaderResource,
      Format = TextureFormat.R32_TYPELESS,
      FirstElement = _offsetInBytes / 4,
      NumElements = _sizeInBytes / 4,
      StructureByteStride = 0,
      Flags = BufferViewFlags.Raw
    };
  }

  /// <summary>
  /// Создать описание для Structured Buffer UAV
  /// </summary>
  public static BufferViewDescription CreateStructuredUAV(ulong _numElements, uint _stride, ulong _firstElement = 0, bool _hasCounter = false)
  {
    return new BufferViewDescription
    {
      ViewType = BufferViewType.UnorderedAccess,
      Format = TextureFormat.Unknown,
      FirstElement = _firstElement,
      NumElements = _numElements,
      StructureByteStride = _stride,
      Flags = _hasCounter ? BufferViewFlags.Counter : BufferViewFlags.None
    };
  }

  /// <summary>
  /// Создать описание для Raw Buffer UAV
  /// </summary>
  public static BufferViewDescription CreateRawUAV(ulong _sizeInBytes, ulong _offsetInBytes = 0)
  {
    return new BufferViewDescription
    {
      ViewType = BufferViewType.UnorderedAccess,
      Format = TextureFormat.R32_TYPELESS,
      FirstElement = _offsetInBytes / 4,
      NumElements = _sizeInBytes / 4,
      StructureByteStride = 0,
      Flags = BufferViewFlags.Raw
    };
  }

  /// <summary>
  /// Валидация описания
  /// </summary>
  public bool Validate(out string _errorMessage)
  {
    _errorMessage = string.Empty;

    if(NumElements == 0)
    {
      _errorMessage = "NumElements must be greater than 0";
      return false;
    }

    if(ViewType == BufferViewType.ShaderResource || ViewType == BufferViewType.UnorderedAccess)
    {
      bool isRaw = (Flags & BufferViewFlags.Raw) != 0;
      bool hasStride = StructureByteStride > 0;
      bool hasFormat = Format != TextureFormat.Unknown;

      if(!isRaw && !hasStride)
      {
        _errorMessage = "Structured buffer must have StructureByteStride > 0 or Raw flag";
        return false;
      }

      if(isRaw && hasStride)
      {
        _errorMessage = "Raw buffer cannot have StructureByteStride > 0";
        return false;
      }
    }

    return true;
  }

  public override string ToString()
  {
    return $"BufferView({ViewType}, Elements:{FirstElement}-{FirstElement + NumElements - 1}, Stride:{StructureByteStride}, Flags:{Flags})";
  }
}