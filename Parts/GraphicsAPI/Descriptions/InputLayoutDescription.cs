using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using GraphicsAPI.Reflections;
using GraphicsAPI.Reflections.Enums;
using GraphicsAPI.Utils;

using Resources.Enums;

namespace GraphicsAPI.Descriptions;

public class InputLayoutDescription
{
  public static InputLayoutDescription FromShader(IShader _shader, uint _inputSlot = 0, uint _instanceDataStepRate = 0)
  {
    if(_shader == null)
      throw new ArgumentNullException(nameof(_shader));

    if(_shader.Stage != ShaderStage.Vertex)
      throw new ArgumentException("Input layout can only be created from vertex shader", nameof(_shader));

    var reflection = _shader.GetReflection();
    if(reflection == null)
      throw new InvalidOperationException($"Shader '{_shader.Name}' does not provide reflection data");

    return FromReflection(reflection, _inputSlot, _instanceDataStepRate);
  }

  public static InputLayoutDescription FromReflection(ShaderReflection _reflection, uint _inputSlot = 0, uint _instanceDataStepRate = 0)
  {
    if(_reflection == null)
      throw new ArgumentNullException(nameof(_reflection));

    var layout = new InputLayoutDescription();

    if(_reflection.InputParameters == null || _reflection.InputParameters.Count == 0)
      return layout;

    var sortedParams = _reflection.InputParameters.OrderBy(p => p.Register).ToList();

    uint currentOffset = 0;
    var inputClassification = _instanceDataStepRate > 0
        ? InputClassification.PerInstanceData
        : InputClassification.PerVertexData;

    foreach(var param in sortedParams)
    {
      var element = new InputElementDescription
      {
        SemanticName = param.SemanticName,
        SemanticIndex = param.SemanticIndex,
        Format = GetFormatFromInputParameter(param),
        InputSlot = _inputSlot,
        AlignedByteOffset = currentOffset,
        InputSlotClass = inputClassification,
        InstanceDataStepRate = _instanceDataStepRate
      };

      uint elementSize = GetFormatSizeInBytes(element.Format);
      currentOffset += elementSize;

      layout.Elements.Add(element);
    }

    return layout;
  }

  public static InputLayoutDescription FromMultipleStreams(
        IShader _shader,
        Dictionary<string, uint> _semanticToSlotMapping,
        Dictionary<uint, uint> _slotToInstanceStepRate = null)
  {
    if(_shader == null)
      throw new ArgumentNullException(nameof(_shader));

    if(_semanticToSlotMapping == null)
      throw new ArgumentNullException(nameof(_semanticToSlotMapping));

    var reflection = _shader.GetReflection();
    var layout = new InputLayoutDescription();

    var slotGroups = new Dictionary<uint, List<InputParameterInfo>>();

    foreach(var param in reflection.InputParameters)
    {
      var semanticKey = param.SemanticName;
      if(!_semanticToSlotMapping.TryGetValue(semanticKey, out uint slot))
      {
        semanticKey = $"{param.SemanticName}{param.SemanticIndex}";
        if(!_semanticToSlotMapping.TryGetValue(semanticKey, out slot))
        {
          throw new InvalidOperationException($"No slot mapping found for semantic '{param.SemanticName}{param.SemanticIndex}'");
        }
      }

      if(!slotGroups.ContainsKey(slot))
        slotGroups[slot] = new List<InputParameterInfo>();

      slotGroups[slot].Add(param);
    }

    foreach(var (slot, parameters) in slotGroups)
    {
      uint currentOffset = 0;
      uint instanceStepRate = 0;

      if(_slotToInstanceStepRate?.TryGetValue(slot, out uint stepRate) == true)
        instanceStepRate = stepRate;

      var inputClassification = instanceStepRate > 0
          ? InputClassification.PerInstanceData
          : InputClassification.PerVertexData;

      foreach(var param in parameters.OrderBy(p => p.Register))
      {
        var element = new InputElementDescription
        {
          SemanticName = param.SemanticName,
          SemanticIndex = param.SemanticIndex,
          Format = GetFormatFromInputParameter(param),
          InputSlot = slot,
          AlignedByteOffset = currentOffset,
          InputSlotClass = inputClassification,
          InstanceDataStepRate = instanceStepRate
        };

        uint elementSize = GetFormatSizeInBytes(element.Format);
        currentOffset += elementSize;

        layout.Elements.Add(element);
      }
    }

    return layout;
  }

  public List<InputElementDescription> Elements { get; set; } = [];

  public bool Validate(out string _errorMessage)
  {
    _errorMessage = null;

    if(Elements == null || Elements.Count == 0)
    {
      _errorMessage = "Input layout must have at least one element";
      return false;
    }

    var semanticsBySlot = new Dictionary<uint, HashSet<string>>();

    foreach(var element in Elements)
    {
      if(string.IsNullOrWhiteSpace(element.SemanticName))
      {
        _errorMessage = "Element semantic name cannot be empty";
        return false;
      }

      if(element.Format == TextureFormat.Unknown)
      {
        _errorMessage = $"Element '{element.SemanticName}' has unknown format";
        return false;
      }

      if(!semanticsBySlot.ContainsKey(element.InputSlot))
        semanticsBySlot[element.InputSlot] = new HashSet<string>();

      string semanticKey = $"{element.SemanticName}_{element.SemanticIndex}";
      if(!semanticsBySlot[element.InputSlot].Add(semanticKey))
      {
        _errorMessage = $"Duplicate semantic '{element.SemanticName}{element.SemanticIndex}' in slot {element.InputSlot}";
        return false;
      }
    }

    foreach(var slotGroup in Elements.GroupBy(e => e.InputSlot))
    {
      var sortedElements = slotGroup.OrderBy(e => e.AlignedByteOffset).ToList();
      uint expectedOffset = 0;

      foreach(var element in sortedElements)
      {
        if(element.AlignedByteOffset < expectedOffset)
        {
          _errorMessage = $"Overlapping elements in slot {element.InputSlot}";
          return false;
        }

        expectedOffset = element.AlignedByteOffset + GetFormatSizeInBytes(element.Format);
      }
    }

    return true;
  }

  public uint GetVertexSizeForSlot(uint _slot)
  {
    var slotElements = Elements.Where(e => e.InputSlot == _slot).ToList();
    if(slotElements.Count == 0)
      return 0;

    uint maxOffset = 0;
    foreach(var element in slotElements)
    {
      uint endOffset = element.AlignedByteOffset + GetFormatSizeInBytes(element.Format);
      maxOffset = Math.Max(maxOffset, endOffset);
    }

    return maxOffset;
  }

  private static TextureFormat GetFormatFromInputParameter(InputParameterInfo _param)
  {
    int componentCount = _param.GetComponentCount();

    return _param.ComponentType switch
    {
      RegisterComponentType.Float32 => componentCount switch
      {
        1 => TextureFormat.R32_FLOAT,
        2 => TextureFormat.R32G32_FLOAT,
        3 => TextureFormat.R32G32B32_FLOAT,
        4 => TextureFormat.R32G32B32A32_FLOAT,
        _ => TextureFormat.Unknown
      },
      RegisterComponentType.UInt32 => componentCount switch
      {
        1 => TextureFormat.R32_UINT,
        2 => TextureFormat.R32G32_UINT,
        3 => TextureFormat.R32G32B32_UINT,
        4 => TextureFormat.R32G32B32A32_UINT,
        _ => TextureFormat.Unknown
      },
      RegisterComponentType.SInt32 => componentCount switch
      {
        1 => TextureFormat.R32_SINT,
        2 => TextureFormat.R32G32_SINT,
        3 => TextureFormat.R32G32B32_SINT,
        4 => TextureFormat.R32G32B32A32_SINT,
        _ => TextureFormat.Unknown
      },
      RegisterComponentType.Float16 => componentCount switch
      {
        1 => TextureFormat.R16_FLOAT,
        2 => TextureFormat.R16G16_FLOAT,
        4 => TextureFormat.R16G16B16A16_FLOAT,
        _ => TextureFormat.Unknown
      },
      RegisterComponentType.UInt16 => componentCount switch
      {
        1 => TextureFormat.R16_UINT,
        2 => TextureFormat.R16G16_UINT,
        4 => TextureFormat.R16G16B16A16_UINT,
        _ => TextureFormat.Unknown
      },
      RegisterComponentType.SInt16 => componentCount switch
      {
        1 => TextureFormat.R16_SINT,
        2 => TextureFormat.R16G16_SINT,
        4 => TextureFormat.R16G16B16A16_SINT,
        _ => TextureFormat.Unknown
      },
      RegisterComponentType.UInt8 => componentCount switch
      {
        1 => TextureFormat.R8_UINT,
        2 => TextureFormat.R8G8_UINT,
        4 => TextureFormat.R8G8B8A8_UINT,
        _ => TextureFormat.Unknown
      },
      RegisterComponentType.SInt8 => componentCount switch
      {
        1 => TextureFormat.R8_SINT,
        2 => TextureFormat.R8G8_SINT,
        4 => TextureFormat.R8G8B8A8_SINT,
        _ => TextureFormat.Unknown
      },
      _ => TextureFormat.Unknown
    };
  }

  private static uint GetFormatSizeInBytes(TextureFormat _format) => Toolbox.GetFormatSize(_format);
}
