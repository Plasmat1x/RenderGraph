using GraphicsAPI.Enums;

namespace GraphicsAPI.Reflections;

public class ResourceBindingInfo
{
  public string Name { get; set; }
  public ResourceBindingType Type { get; set; }
  public uint BindPoint { get; set; }
  public uint BindCount { get; set; } = 1;
  public uint Space { get; set; } = 0;
  public ResourceDimension Dimension { get; set; }
  public ResourceReturnType ReturnType { get; set; }
  public uint NumSamples { get; set; }
  public ResourceBindingFlags Flags { get; set; }
}
