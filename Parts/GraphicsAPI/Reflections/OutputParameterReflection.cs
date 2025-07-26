using GraphicsAPI.Enums;

namespace GraphicsAPI.Reflections;

public class OutputParameterReflection
{
  public string SemanticName { get; set; } = string.Empty;
  public uint SemanticIndex { get; set; }
  public uint Register { get; set; }
  public ComponentType ComponentType { get; set; }
  public ComponentMask Mask { get; set; }
}