using GraphicsAPI.Enums;

namespace GraphicsAPI;

public class ResourceBinding
{
  public string Name { get; set; } = string.Empty;
  public ResourceBindingType Type { get; set; }
  public uint Slot { get; set; }
  public uint Count { get; set; } = 1;
}