using GraphicsAPI.Enums;

namespace Directx12Impl.Parts.Utils;

/// <summary>
/// Вспомогательный класс для валидации шейдеров
/// </summary>
public static class DX12ShaderValidator
{
  public static bool ValidateShaderBytecode(byte[] _bytecode)
  {
    if(_bytecode == null || _bytecode.Length < 4)
      return false;

    return _bytecode[0] == 0x44 && _bytecode[1] == 0x58 &&
           _bytecode[2] == 0x42 && _bytecode[3] == 0x43;
  }

  public static void ValidatePipelineShaders(
      DX12Shader _vertexShader,
      DX12Shader _pixelShader,
      DX12Shader _geometryShader = null,
      DX12Shader _hullShader = null,
      DX12Shader _domainShader = null)
  {
    if(_vertexShader == null)
      throw new ArgumentNullException(nameof(_vertexShader));

    if(_vertexShader.Stage != ShaderStage.Vertex)
      throw new ArgumentException("Invalid vertex shader stage");

    if(_pixelShader != null && _pixelShader.Stage != ShaderStage.Pixel)
      throw new ArgumentException("Invalid pixel shader stage");

    if(_pixelShader != null && !_vertexShader.IsCompatibleWith(_pixelShader))
    {
      throw new InvalidOperationException(
          "Vertex shader outputs do not match pixel shader inputs");
    }

    if(_hullShader != null || _domainShader != null)
    {
      if(_hullShader == null || _domainShader == null)
      {
        throw new InvalidOperationException(
            "Both hull and domain shaders must be specified for tessellation");
      }

      if(_hullShader.Stage != ShaderStage.Hull)
        throw new ArgumentException("Invalid hull shader stage");

      if(_domainShader.Stage != ShaderStage.Domain)
        throw new ArgumentException("Invalid domain shader stage");
    }

    if(_geometryShader != null)
    {
      if(_geometryShader.Stage != ShaderStage.Geometry)
        throw new ArgumentException("Invalid geometry shader stage");

      var inputShader = _domainShader ?? _vertexShader;
      if(!inputShader.IsCompatibleWith(_geometryShader))
      {
        throw new InvalidOperationException(
            "Geometry shader inputs do not match previous stage outputs");
      }

      if(_pixelShader != null && !_geometryShader.IsCompatibleWith(_pixelShader))
      {
        throw new InvalidOperationException(
            "Geometry shader outputs do not match pixel shader inputs");
      }
    }
  }

  public static List<string> GetMissingResources(
      DX12Shader _shader,
      Dictionary<string, object> _availableResources)
  {
    var missing = new List<string>();
    var reflection = _shader.GetReflection();


    foreach(var cb in reflection.ConstantBuffers)
    {
      if(!_availableResources.ContainsKey(cb.Name))
        missing.Add($"Constant Buffer: {cb.Name}");
    }

    foreach(var resource in reflection.BoundResources)
    {
      if(!_availableResources.ContainsKey(resource.Name))
        missing.Add($"Texture: {resource.Name}");
    }

    foreach(var sampler in reflection.Samplers)
    {
      if(!_availableResources.ContainsKey(sampler.Name))
        missing.Add($"Sampler: {sampler.Name}");
    }

    foreach(var uav in reflection.UnorderedAccessViews)
    {
      if(!_availableResources.ContainsKey(uav.Name))
        missing.Add($"UAV: {uav.Name}");
    }

    return missing;
  }
}
