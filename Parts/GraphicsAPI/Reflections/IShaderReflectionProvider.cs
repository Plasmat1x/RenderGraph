using GraphicsAPI.Enums;

namespace GraphicsAPI.Reflections;

public interface IShaderReflectionProvider
{
  ShaderReflection CreateReflection(byte[] _bytecode, ShaderStage _stager);
  bool IsBytecodeSupported(byte[] _bytecode);
  string GetShaderModel(byte[] _bytecode);
}
