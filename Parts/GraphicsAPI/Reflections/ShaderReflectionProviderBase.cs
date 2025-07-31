using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Utils;

namespace GraphicsAPI.Reflections;

public abstract class ShaderReflectionProviderBase: IShaderReflectionProvider
{
  public abstract ShaderReflection CreateReflection(byte[] _bytecode, ShaderStage _staget);
  public abstract bool IsBytecodeSupported(byte[] _byte);
  public abstract string GetShaderModel(byte[] _byte);

  public static InputLayoutDescription CreateInputLayoutFromReflection(ShaderReflection _reflection)
  {
    var layout = new InputLayoutDescription();

    if(_reflection?.InputParameters == null || _reflection.InputParameters.Count == 0)
      return layout;

    uint currentOffset = 0;

    foreach(var param in _reflection.InputParameters.OrderBy(_p => _p.Register))
    {
      var element = param.ToInputElement();
      element.AlignedByteOffset = currentOffset;
      uint elementSize = Toolbox.GetFormatSize(element.Format);
      currentOffset += elementSize;
      layout.Elements.Add(element);
    }

    return layout;
  }

  protected static ShaderVariableType ParseVariableType(string _typeName)
  {
    if(string.IsNullOrEmpty(_typeName))
      return ShaderVariableType.Void;

    return _typeName.ToLower() switch
    {
      "void" => ShaderVariableType.Void,
      "bool" => ShaderVariableType.Bool,
      "int" => ShaderVariableType.Int,
      "uint" => ShaderVariableType.UInt,
      "float" => ShaderVariableType.Float,
      "float2" => ShaderVariableType.Float2,
      "float3" => ShaderVariableType.Float3,
      "float4" => ShaderVariableType.Float4,
      "int2" => ShaderVariableType.Int2,
      "int3" => ShaderVariableType.Int3,
      "int4" => ShaderVariableType.Int4,
      "uint2" => ShaderVariableType.UInt2,
      "uint3" => ShaderVariableType.UInt3,
      "uint4" => ShaderVariableType.UInt4,
      "float2x2" => ShaderVariableType.Float2x2,
      "float3x3" => ShaderVariableType.Float3x3,
      "float4x4" => ShaderVariableType.Float4x4,
      "texture1d" => ShaderVariableType.Texture1D,
      "texture2d" => ShaderVariableType.Texture2D,
      "texture3d" => ShaderVariableType.Texture3D,
      "texturecube" => ShaderVariableType.TextureCube,
      "sampler" => ShaderVariableType.Sampler,
      "samplerstate" => ShaderVariableType.Sampler,
      _ => ShaderVariableType.UserDefined
    };
  }

  protected static uint GetVariableTypeSize(ShaderVariableType type)
  {
    return type switch
    {
      ShaderVariableType.Bool => 4,
      ShaderVariableType.Int => 4,
      ShaderVariableType.UInt => 4,
      ShaderVariableType.Float => 4,
      ShaderVariableType.Float2 => 8,
      ShaderVariableType.Float3 => 12,
      ShaderVariableType.Float4 => 16,
      ShaderVariableType.Int2 => 8,
      ShaderVariableType.Int3 => 12,
      ShaderVariableType.Int4 => 16,
      ShaderVariableType.UInt2 => 8,
      ShaderVariableType.UInt3 => 12,
      ShaderVariableType.UInt4 => 16,
      ShaderVariableType.Float2x2 => 32,
      ShaderVariableType.Float3x3 => 48,
      ShaderVariableType.Float4x4 => 64,
      _ => 0
    };
  }
}
