using GraphicsAPI.Enums;

using Resources.Enums;

namespace GraphicsAPI.Descriptions;

public class InputElementDescription
{
  public string SemanticName { get; set; }
  public uint SemanticIndex { get; set; }
  public TextureFormat Format { get; set; }
  public uint InputSlot { get; set; }
  public uint AlignedByteOffset { get; set; }
  public InputClassification InputSlotClass { get; set; }
  public uint InstanceDataStepRate { get; set; }
}