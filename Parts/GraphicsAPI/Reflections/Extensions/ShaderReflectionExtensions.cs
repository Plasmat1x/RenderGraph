using GraphicsAPI.Enums;
using GraphicsAPI.Reflections.Enums;

namespace GraphicsAPI.Reflections.Extensions;

public static class ShaderReflectionExtensions
{
  public static bool Compatible(this ShaderReflection _stage, ShaderReflection _other)
  {
    if(_stage == null || _other == null)
      return false;

    foreach(var output in _stage.OutputParameters)
    {
      var matchingInput = _other.InputParameters.FirstOrDefault(
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

  public static uint CalculateTotalConstantBufferSize(this ShaderReflection _reflection)
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

  public static HashSet<uint> GetUsedResourceSlots(this ShaderReflection _reflection, ResourceBindingType _type)
  {
    var slots = new HashSet<uint>();

    if(_reflection == null)
      return slots;

    var resources = _type switch
    {
      ResourceBindingType.ConstantBuffer => _reflection.ConstantBuffers.Select(_cb => new ResourceBindingInfo
      {
        BindPoint = _cb.BindPoint,
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