using GraphicsAPI.Enums;
using GraphicsAPI.Reflections;
using GraphicsAPI.Reflections.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockImpl;
public class MockShaderReflectionProvider: ShaderReflectionProviderBase
{
  private static readonly Dictionary<ShaderStage, Func<ShaderReflection>> DefaultReflections = new()
    {
        { ShaderStage.Vertex, CreateDefaultVertexShaderReflection },
        { ShaderStage.Pixel, CreateDefaultPixelShaderReflection },
        { ShaderStage.Compute, CreateDefaultComputeShaderReflection }
    };

  public override ShaderReflection CreateReflection(byte[] bytecode, ShaderStage stage)
  {
    Console.WriteLine($"[MockReflection] Creating reflection for {stage} shader");

    // Для Mock-реализации возвращаем предопределенные данные рефлексии
    if(DefaultReflections.TryGetValue(stage, out var creator))
    {
      return creator();
    }

    // Для неизвестных стадий возвращаем пустую рефлексию
    return new ShaderReflection
    {
      Info = new ShaderInfo
      {
        Stage = stage,
        ShaderModel = "5_1",
        Creator = "MockShaderCompiler"
      }
    };
  }

  public override bool IsBytecodeSupported(byte[] bytecode)
  {
    // Mock всегда поддерживает любой байткод
    return bytecode != null && bytecode.Length > 0;
  }

  public override string GetShaderModel(byte[] bytecode)
  {
    // Mock всегда возвращает 5.1
    return "5_1";
  }

  /// <summary>
  /// Создать типичную рефлексию для вершинного шейдера
  /// </summary>
  private static ShaderReflection CreateDefaultVertexShaderReflection()
  {
    return new ShaderReflection
    {
      Info = new ShaderInfo
      {
        Stage = ShaderStage.Vertex,
        ShaderModel = "5_1",
        Creator = "MockShaderCompiler",
        InstructionCount = 42
      },

      // Константные буферы
      ConstantBuffers = new List<ConstantBufferInfo>
            {
                new ConstantBufferInfo
                {
                    Name = "PerFrameConstants",
                    Size = 256,
                    BindPoint = 0,
                    Type = ConstantBufferType.CBuffer,
                    Variables = new List<ShaderVariableInfo>
                    {
                        new ShaderVariableInfo
                        {
                            Name = "ViewMatrix",
                            Offset = 0,
                            Size = 64,
                            Type = ShaderVariableType.Float4x4,
                            Rows = 4,
                            Columns = 4
                        },
                        new ShaderVariableInfo
                        {
                            Name = "ProjectionMatrix",
                            Offset = 64,
                            Size = 64,
                            Type = ShaderVariableType.Float4x4,
                            Rows = 4,
                            Columns = 4
                        },
                        new ShaderVariableInfo
                        {
                            Name = "ViewProjectionMatrix",
                            Offset = 128,
                            Size = 64,
                            Type = ShaderVariableType.Float4x4,
                            Rows = 4,
                            Columns = 4
                        },
                        new ShaderVariableInfo
                        {
                            Name = "Time",
                            Offset = 192,
                            Size = 4,
                            Type = ShaderVariableType.Float
                        },
                        new ShaderVariableInfo
                        {
                            Name = "CameraPosition",
                            Offset = 208,
                            Size = 12,
                            Type = ShaderVariableType.Float3,
                            Rows = 1,
                            Columns = 3
                        }
                    }
                },
                new ConstantBufferInfo
                {
                    Name = "PerObjectConstants",
                    Size = 80,
                    BindPoint = 1,
                    Type = ConstantBufferType.CBuffer,
                    Variables = new List<ShaderVariableInfo>
                    {
                        new ShaderVariableInfo
                        {
                            Name = "WorldMatrix",
                            Offset = 0,
                            Size = 64,
                            Type = ShaderVariableType.Float4x4,
                            Rows = 4,
                            Columns = 4
                        },
                        new ShaderVariableInfo
                        {
                            Name = "ObjectColor",
                            Offset = 64,
                            Size = 16,
                            Type = ShaderVariableType.Float4,
                            Rows = 1,
                            Columns = 4
                        }
                    }
                }
            },

      // Входные параметры
      InputParameters = new List<InputParameterInfo>
            {
                new InputParameterInfo
                {
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    Register = 0,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b1111, // XYZW
                    SystemValueType = SystemValueType.Undefined
                },
                new InputParameterInfo
                {
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    Register = 1,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b0111, // XYZ
                    SystemValueType = SystemValueType.Undefined
                },
                new InputParameterInfo
                {
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    Register = 2,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b0011, // XY
                    SystemValueType = SystemValueType.Undefined
                },
                new InputParameterInfo
                {
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    Register = 3,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b1111, // XYZW
                    SystemValueType = SystemValueType.Undefined
                }
            },

      // Выходные параметры
      OutputParameters = new List<OutputParameterInfo>
            {
                new OutputParameterInfo
                {
                    SemanticName = "SV_Position",
                    SemanticIndex = 0,
                    Register = 0,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b1111,
                    SystemValueType = SystemValueType.Position
                },
                new OutputParameterInfo
                {
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    Register = 1,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b0011,
                    SystemValueType = SystemValueType.Undefined
                },
                new OutputParameterInfo
                {
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    Register = 2,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b0111,
                    SystemValueType = SystemValueType.Undefined
                },
                new OutputParameterInfo
                {
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    Register = 3,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b1111,
                    SystemValueType = SystemValueType.Undefined
                }
            }
    };
  }

  /// <summary>
  /// Создать типичную рефлексию для пиксельного шейдера
  /// </summary>
  private static ShaderReflection CreateDefaultPixelShaderReflection()
  {
    return new ShaderReflection
    {
      Info = new ShaderInfo
      {
        Stage = ShaderStage.Pixel,
        ShaderModel = "5_1",
        Creator = "MockShaderCompiler",
        InstructionCount = 35
      },

      // Константные буферы
      ConstantBuffers = new List<ConstantBufferInfo>
            {
                new ConstantBufferInfo
                {
                    Name = "MaterialConstants",
                    Size = 48,
                    BindPoint = 2,
                    Type = ConstantBufferType.CBuffer,
                    Variables = new List<ShaderVariableInfo>
                    {
                        new ShaderVariableInfo
                        {
                            Name = "DiffuseColor",
                            Offset = 0,
                            Size = 16,
                            Type = ShaderVariableType.Float4
                        },
                        new ShaderVariableInfo
                        {
                            Name = "SpecularColor",
                            Offset = 16,
                            Size = 16,
                            Type = ShaderVariableType.Float4
                        },
                        new ShaderVariableInfo
                        {
                            Name = "Roughness",
                            Offset = 32,
                            Size = 4,
                            Type = ShaderVariableType.Float
                        },
                        new ShaderVariableInfo
                        {
                            Name = "Metalness",
                            Offset = 36,
                            Size = 4,
                            Type = ShaderVariableType.Float
                        }
                    }
                }
            },

      // Текстурные ресурсы
      BoundResources = new List<ResourceBindingInfo>
            {
                new ResourceBindingInfo
                {
                    Name = "DiffuseTexture",
                    Type = ResourceBindingType.ShaderResource,
                    BindPoint = 0,
                    Dimension = ResourceDimension.Texture2D,
                    ReturnType = ResourceReturnType.Float,
                    Flags = ResourceBindingFlags.UsedByPixelShader
                },
                new ResourceBindingInfo
                {
                    Name = "NormalTexture",
                    Type = ResourceBindingType.ShaderResource,
                    BindPoint = 1,
                    Dimension = ResourceDimension.Texture2D,
                    ReturnType = ResourceReturnType.Float,
                    Flags = ResourceBindingFlags.UsedByPixelShader
                },
                new ResourceBindingInfo
                {
                    Name = "SpecularTexture",
                    Type = ResourceBindingType.ShaderResource,
                    BindPoint = 2,
                    Dimension = ResourceDimension.Texture2D,
                    ReturnType = ResourceReturnType.Float,
                    Flags = ResourceBindingFlags.UsedByPixelShader
                }
            },

      // Сэмплеры
      Samplers = new List<SamplerBindingInfo>
            {
                new SamplerBindingInfo
                {
                    Name = "LinearSampler",
                    BindPoint = 0,
                    Flags = SamplerBindingFlags.UsedByPixelShader
                },
                new SamplerBindingInfo
                {
                    Name = "PointSampler",
                    BindPoint = 1,
                    Flags = SamplerBindingFlags.UsedByPixelShader
                }
            },

      // Входные параметры (из вершинного шейдера)
      InputParameters = new List<InputParameterInfo>
            {
                new InputParameterInfo
                {
                    SemanticName = "SV_Position",
                    SemanticIndex = 0,
                    Register = 0,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b1111,
                    SystemValueType = SystemValueType.Position
                },
                new InputParameterInfo
                {
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    Register = 1,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b0011,
                    SystemValueType = SystemValueType.Undefined
                },
                new InputParameterInfo
                {
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    Register = 2,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b0111,
                    SystemValueType = SystemValueType.Undefined
                },
                new InputParameterInfo
                {
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    Register = 3,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b1111,
                    SystemValueType = SystemValueType.Undefined
                }
            },

      // Выходные параметры
      OutputParameters = new List<OutputParameterInfo>
            {
                new OutputParameterInfo
                {
                    SemanticName = "SV_Target",
                    SemanticIndex = 0,
                    Register = 0,
                    ComponentType = RegisterComponentType.Float32,
                    Mask = 0b1111,
                    SystemValueType = SystemValueType.Target
                }
            }
    };
  }

  /// <summary>
  /// Создать типичную рефлексию для compute shader
  /// </summary>
  private static ShaderReflection CreateDefaultComputeShaderReflection()
  {
    return new ShaderReflection
    {
      Info = new ShaderInfo
      {
        Stage = ShaderStage.Compute,
        ShaderModel = "5_1",
        Creator = "MockShaderCompiler",
        InstructionCount = 128
      },

      ThreadGroupSize = new ThreadGroupSize
      {
        X = 8,
        Y = 8,
        Z = 1
      },

      ConstantBuffers = new List<ConstantBufferInfo>
            {
                new ConstantBufferInfo
                {
                    Name = "ComputeConstants",
                    Size = 32,
                    BindPoint = 0,
                    Type = ConstantBufferType.CBuffer,
                    Variables = new List<ShaderVariableInfo>
                    {
                        new ShaderVariableInfo
                        {
                            Name = "ThreadGroupCount",
                            Offset = 0,
                            Size = 12,
                            Type = ShaderVariableType.UInt3
                        },
                        new ShaderVariableInfo
                        {
                            Name = "Time",
                            Offset = 16,
                            Size = 4,
                            Type = ShaderVariableType.Float
                        }
                    }
                }
            },

      BoundResources = new List<ResourceBindingInfo>
            {
                new ResourceBindingInfo
                {
                    Name = "InputBuffer",
                    Type = ResourceBindingType.ShaderResource,
                    BindPoint = 0,
                    Dimension = ResourceDimension.StructuredBuffer,
                    ReturnType = ResourceReturnType.Mixed,
                    Flags = ResourceBindingFlags.UsedByComputeShader
                }
            },

      UnorderedAccessViews = new List<ResourceBindingInfo>
            {
                new ResourceBindingInfo
                {
                    Name = "OutputBuffer",
                    Type = ResourceBindingType.UnorderedAccess,
                    BindPoint = 0,
                    Dimension = ResourceDimension.StructuredBuffer,
                    ReturnType = ResourceReturnType.Mixed,
                    Flags = ResourceBindingFlags.UsedByComputeShader
                }
            }
    };
  }
}