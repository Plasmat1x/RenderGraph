using GraphicsAPI.Descriptions;
using GraphicsAPI.Reflections;
using GraphicsAPI.Reflections.Extensions;

namespace GraphicsAPI.Extensions;
public static class PipelineStateDescriptionExtensions
{
  public static void ApplyReflectionToPipelineState(
    this PipelineStateDescription _pipelineState,
    ShaderReflection _vertexShaderReflection,
    ShaderReflection _pixelShaderReflection = null)
  {
    if(_vertexShaderReflection != null)
      _pipelineState.InputLayout = ShaderReflectionProviderBase.CreateInputLayoutFromReflection(_vertexShaderReflection);

    if(_vertexShaderReflection != null && _pixelShaderReflection != null)
    {
      if(!_vertexShaderReflection.Compatible(_pixelShaderReflection))
        throw new InvalidOperationException("Vertex shader outputs do not match pixel shader inputs");
    }
  }
}
