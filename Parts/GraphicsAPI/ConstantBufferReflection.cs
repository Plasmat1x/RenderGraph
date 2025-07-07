namespace GraphicsAPI;

public class ConstantBufferReflection
{
  public string Name { get; set; } = string.Empty;
  public uint Size { get; set; }
  public uint Slot { get; set; }
  public List<VariableReflection> Variables { get; set; } = new();
}