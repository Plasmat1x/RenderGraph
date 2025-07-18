using GraphicsAPI.Enums;

using Resources.Enums;

using System.Numerics;
using System.Security.AccessControl;

namespace GraphicsAPI;

public class ShaderReflection
{
  public List<ConstantBufferReflection> ConstantBuffers { get; set; } = [];
  public List<ResourceBinding> Resources { get; set; } = [];
  public List<InputParameterReflection> InputParameters { get; set; } = [];
  public List<OutputParameterReflection> OutputParameters { get; set; } = [];

  public List<ConstantBufferInfo> ConstatnBuffers { get; set; } = [];
  public List<ResourceBindingInfo> BoundResources { get; set; } = [];
  public List<SamplerBindingInfo> Samplers { get; set; } = [];
  public List<ResourceBindingInfo> UnordererdAccessViews { get; set; } = [];
  public List<InputElementInfo> InputElements { get; set; } = [];
  public Vector3 ThreadGroupSize { get; set; }
}

public class ConstantBufferInfo
{
  public string Name {  get; set; }
  public uint Size { get;set; }
  public uint BindPoint { get; set; }
  public uint BindCount {  get; set; }
  public List<ShaderVariableInfo> Variables { get; set; }
}

public class ShaderVariableInfo
{
  public string Name { get; set; }
  public uint Offset { get; set; }
  public uint Size { get; set; }
}

public class ResourceBindingInfo
{
  public string Name { get; set; }
  public ResourceBindingType Type { get; set; }
  public uint BindPoint { get; set;}
  public uint BindCount { get; set; }
  public TextureDimension Dimension { get; set; }
}

public class SamplerBindingInfo
{
  public string Name { get; set; }
  public uint BindPoint { get; set; }
  public uint BindCount { get; set; }
}

public class InputElementInfo
{
  public string SemanticName { get; set; }
  public uint SemanticIndex { get; set; }
  public TextureFormat Format { get; set; }
  public uint InputSlot { get; set; }
  public uint AlignedByteOffset { get; set; }
  public InputClassification InputSlotClass { get; set; }
  public uint InstanceDataStepRate { get; set; }
}

public enum TextureDimension
{
  Unknown,
  Texture1D,
  Texture2D,
  Texture3D,
  TextureCube,
  Texture1DArray,
  Texture2DArray,
  TextureCubeArray
}

public enum InputClassification
{
  PerVertexData,
  PerInstanceData
}