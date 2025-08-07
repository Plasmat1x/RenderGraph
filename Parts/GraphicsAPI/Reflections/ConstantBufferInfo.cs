using GraphicsAPI.Reflections.Enums;

namespace GraphicsAPI.Reflections;

public class ConstantBufferInfo
{
  public string Name { get; set; }
  public uint Size { get; set; }
  public uint BindPoint { get; set; }
  public uint BindCount { get; set; }
  public ConstantBufferType Type { get; set; }
  public List<ShaderVariableInfo> Variables { get; set; } = [];

  public ShaderVariableInfo GetVariable(string _name) => Variables.FirstOrDefault(_v => _v.Name == _name);
}
