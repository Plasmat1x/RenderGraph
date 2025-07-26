namespace GraphicsAPI.Descriptions;

public class ConstantBufferDescription
{
  public string Name {  get; set; }
  public uint Size { get;set; }
  public uint BindPoint { get; set; }
  public uint BindCount {  get; set; }
  public List<ShaderVariableDescription> Variables { get; set; }
}
