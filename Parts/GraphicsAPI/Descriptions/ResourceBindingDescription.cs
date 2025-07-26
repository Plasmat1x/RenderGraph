using GraphicsAPI.Enums;

namespace GraphicsAPI.Descriptions;

public class ResourceBindingDescription
{
  public string Name { get; set; }
  public ResourceBindingType Type { get; set; }
  public uint BindPoint { get; set;}
  public uint BindCount { get; set; }
  public TextureDimension Dimension { get; set; }
}
