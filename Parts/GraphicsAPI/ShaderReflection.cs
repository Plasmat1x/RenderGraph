namespace GraphicsAPI;

public class ShaderReflection
{
  public List<ConstantBufferReflection> ConstantBuffers { get; set; } = new();
  public List<ResourceBinding> Resources { get; set; } = new();
  public List<InputParameterReflection> InputParameters { get; set; } = new();
  public List<OutputParameterReflection> OutputParameters { get; set; } = new();
}