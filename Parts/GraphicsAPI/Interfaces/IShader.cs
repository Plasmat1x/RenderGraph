using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Reflections;

namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс шейдера
/// </summary>
public interface IShader: IResource
{
  ShaderStage Stage { get; }
  ShaderDescription Description { get; }
  byte[] Bytecode { get; }

  // Рефлексия
  ShaderReflection GetReflection();
  bool HasConstantBuffer(string _name);
  bool HasTexture(string _name);
  bool HasSampler(string _name);
  bool HasUnordererAccess(string _name);

  ConstantBufferInfo GetConstantBufferInfo(string _name);
  ResourceBindingInfo GetResourceInfo(string _name);
  SamplerBindingInfo GetSamplerInfo(string _name);
  bool IsCompatibleWith(IShader _otherShader);
}
