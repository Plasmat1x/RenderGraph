using GraphicsAPI.Descriptions;

using System.Numerics;
using System.Security.AccessControl;

namespace GraphicsAPI.Reflections;

public class ShaderReflection
{
  public List<ConstantBufferReflection> ConstantBuffers { get; set; } = [];
  public List<ResourceBinding> Resources { get; set; } = [];
  public List<InputParameterReflection> InputParameters { get; set; } = [];
  public List<OutputParameterReflection> OutputParameters { get; set; } = [];

  public List<ConstantBufferDescription> ConstatnBuffers { get; set; } = [];
  public List<ResourceBindingDescription> BoundResources { get; set; } = [];
  public List<SamplerBindingDescription> Samplers { get; set; } = [];
  public List<ResourceBindingDescription> UnordererdAccessViews { get; set; } = [];
  public List<InputElementDescription> InputElements { get; set; } = [];
  public Vector3 ThreadGroupSize { get; set; }
}
