using GraphicsAPI.Enums;

namespace GraphicsAPI.Reflections;

public class InputParameterReflection
{
  public string SemanticName { get; set; } = string.Empty;
  public uint SemanticIndex { get; set; }
  public uint Register { get; set; }
  public ComponentType ComponentType { get; set; }
  public ComponentMask Mask { get; set; }
}