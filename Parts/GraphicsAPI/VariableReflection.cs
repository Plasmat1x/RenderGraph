using GraphicsAPI.Enums;

namespace GraphicsAPI;

public class VariableReflection
{
  public string Name { get; set; } = string.Empty;
  public uint Offset { get; set; }
  public uint Size { get; set; }
  public ShaderVariableType Type { get; set; }
}