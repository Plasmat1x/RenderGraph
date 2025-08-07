using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Reflections.Enums;

using Resources.Enums;

namespace GraphicsAPI.Reflections;

public class InputParameterInfo
{
  public string SemanticName { get; set; }
  public uint SemanticIndex { get; set; }
  public uint Register { get; set; }
  public SystemValueType SystemValueType { get; set; }
  public RegisterComponentType ComponentType { get; set; }
  public byte Mask { get; set; }
  public byte ReadWriteMask { get; set; }
  public uint Stream { get; set; }
  public MinPrecision MinPrecision { get; set; }

  public int GetComponentCount()
  {
    var count = 0;
    for (var i = 0; i<4; i++)
    {
      if ((Mask & 1 << i) != 0)
        count++;
    }

    return count;
  }

  public InputElementDescription ToInputElement()
  {
    return new InputElementDescription
    {
      SemanticName = SemanticName,
      SemanticIndex = SemanticIndex,
      Format = GetFormatFromComponentType(),
      InputSlot = 0,
      AlignedByteOffset = 0,
      InputSlotClass = InputClassification.PerVertexData,
      InstanceDataStepRate = 0
    };
  }

  private TextureFormat GetFormatFromComponentType()
  {
    var componentCount = GetComponentCount();

    return ComponentType switch
    {
      RegisterComponentType.Float32 => componentCount switch
      {
        1 => TextureFormat.R32_FLOAT,
        2 => TextureFormat.R32G32_FLOAT,
        3 => TextureFormat.R32G32B32_FLOAT,
        4 => TextureFormat.R32G32B32A32_FLOAT,
        _ => TextureFormat.Unknown
      },
      RegisterComponentType.UInt32 => componentCount switch
      {
        1 => TextureFormat.R32_UINT,
        2 => TextureFormat.R32G32_UINT,
        3 => TextureFormat.R32G32B32_UINT,
        4 => TextureFormat.R32G32B32A32_UINT,
        _ => TextureFormat.Unknown
      },
      RegisterComponentType.SInt32 => componentCount switch
      {
        1 => TextureFormat.R32_SINT,
        2 => TextureFormat.R32G32_SINT,
        3 => TextureFormat.R32G32B32_SINT,
        4 => TextureFormat.R32G32B32A32_SINT,
        _ => TextureFormat.Unknown
      },
      _ => TextureFormat.Unknown
    };
  }
}
