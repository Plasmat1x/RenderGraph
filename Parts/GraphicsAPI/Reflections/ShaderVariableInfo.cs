namespace GraphicsAPI.Reflections;

public class ShaderVariableInfo
{
  public string Name { get; set; }
  public uint Offset { get; set; }
  public uint Size { get; set; }
  public ShaderVariableType Type { get; set; }
  public uint Rows { get; set; }
  public uint Columns { get; set; }
  public uint Elements { get; set; }
  public ShaderVariableFlags Flags {get;set;}
  public object DefaultValue { get;set;}
}
