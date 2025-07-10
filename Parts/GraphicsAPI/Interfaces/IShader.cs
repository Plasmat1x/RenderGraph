using GraphicsAPI.Enums;

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
  bool HasConstantBuffer(string name);
  bool HasTexture(string name);
  bool HasSampler(string name);
}
