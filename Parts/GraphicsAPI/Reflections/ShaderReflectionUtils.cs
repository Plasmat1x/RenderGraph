using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

namespace GraphicsAPI.Reflections;

public static class ShaderReflectionUtils
{
  public static bool AreStagesCompatible(ShaderReflection _staget1, ShaderReflection _staget2)
  {
    if(_staget1 == null || _staget2 == null)
      return false;

    foreach(var output in _staget1.OutputParameters)
    {
      var  matchingInput = _staget2.InputParameters.FirstOrDefault(
        _input => _input.SemanticName == output.SemanticName && 
        _input.SemanticIndex == output.SemanticIndex);

      if(matchingInput == null)
      {
        if(output.SystemValueType == SystemValueType.Undefined)
          return false;
      }
      else
      {
        if(output.ComponentType != matchingInput.ComponentType || output.Mask != matchingInput.Mask)
          return false;
      }
    }

    return true;
  }

  public static void ApplyRefectioToPipelineState(
      PipelineStateDescription _pipelineState,
      ShaderReflection _vertexShaderReflection,
      ShaderReflection _pixelShaderReflection = null)
  {
    if(_vertexShaderReflection != null)
      _pipelineState.InputLayout = ShaderReflectionProviderBase.CreateInputLayoutFromReflection(_vertexShaderReflection);

    if(_vertexShaderReflection != null && _pixelShaderReflection != null)
    {
      if(!AreStagesCompatible(_vertexShaderReflection, _pixelShaderReflection))
        throw new InvalidOperationException("Vertex shader outputs do not match pixel shader inputs");
    }
  }

  public static uint CalculateTotalConstantBufferSize(ShaderReflection _reflection)
  {
    if(_reflection?.ConstantBuffers == null)
      return 0;

    uint totalSize = 0;
    foreach(var cb in _reflection.ConstantBuffers)
    {
      totalSize += cb.Size;
    }

    return totalSize;
  }

  public static HashSet<uint> GetUsedResourceSlots(ShaderReflection _reflection, ResourceBindingType _type)
  {
    var slots = new HashSet<uint>();

    if(_reflection == null)
      return slots;

    IEnumerable<ResourceBindingInfo> resources = _type switch
    {
      ResourceBindingType.ConstantBuffer => _reflection.ConstantBuffers.Select(cb => new ResourceBindingInfo
      {
        BindPoint = cb.BindPoint,
        Type = ResourceBindingType.ConstantBuffer
      }),
      ResourceBindingType.ShaderResource => _reflection.BoundResources,
      ResourceBindingType.UnorderedAccess => _reflection.UnorderedAccessViews,
      _ => Enumerable.Empty<ResourceBindingInfo>()
    };

    foreach(var resource in resources.Where(_r => _r.Type == _type))
    {
      for(uint i = 0; i < resource.BindCount; i++)
      {
        slots.Add(resource.BindPoint + i);
      }
    }

    return slots;
  }
}