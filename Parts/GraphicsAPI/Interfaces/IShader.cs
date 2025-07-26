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
}
