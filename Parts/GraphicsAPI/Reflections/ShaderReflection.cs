using System.Linq.Expressions;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.AccessControl;

namespace GraphicsAPI.Reflections;

public class ShaderReflection
{
  public ShaderInfo Info { get; set; }
  public List<ConstantBufferInfo> ConstantBuffers { get; set; } = [];
  public List<ResourceBindingInfo> BoundResources { get; set; } = [];
  public List<SamplerBindingInfo> Samplers { get; set; } = [];
  public List<ResourceBindingInfo> UnorderedAccessViews { get; set; } = [];
  public List<InputParameterInfo> InputParameters { get; set; } = [];
  public List<OutputParameterInfo> OutputParameters { get; set; } = [];
  public ThreadGroupSize ThreadGroupSize { get; set; }

  public ConstantBufferInfo GetConstantBuffer(string _name) => ConstantBuffers.FirstOrDefault(_cb => _cb.Name == _name);
  public ResourceBindingInfo GetResource(string _name) => BoundResources.FirstOrDefault(_r => _r.Name == _name);
  public SamplerBindingInfo GetSampler(string _name) => Samplers.FirstOrDefault(_s => _s.Name == _name);

}
