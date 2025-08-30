using Directx12Impl;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

namespace DX12ShaderCompilation;

/// <summary>
/// Тест компиляции и рефлексии DX12Shader
/// </summary>
public class DX12ShaderCompilation
{
  public static void RunTests()
  {
    Console.WriteLine("=== DX12Shader Compilation Tests ===\n");

    try
    {
      TestSimpleVertexShaderCompilation();
      TestPixelShaderWithResources();
      TestComputeShaderCompilation();
      TestShaderWithMacros();
      TestShaderReflection();

      Console.WriteLine("\n✅ All tests passed!");
    }
    catch(Exception ex)
    {
      Console.WriteLine($"\n❌ Test failed: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
  }

  static void TestSimpleVertexShaderCompilation()
  {
    Console.WriteLine("Test 1: Simple Vertex Shader Compilation");
    Console.WriteLine("----------------------------------------");

    string hlslCode = @"
                struct VSInput
                {
                    float3 Position : POSITION;
                    float4 Color : COLOR;
                };

                struct PSInput
                {
                    float4 Position : SV_POSITION;
                    float4 Color : COLOR;
                };

                PSInput main(VSInput input)
                {
                    PSInput output;
                    output.Position = float4(input.Position, 1.0);
                    output.Color = input.Color;
                    return output;
                }
            ";

    var shaderDesc = new ShaderDescription
    {
      Name = "SimpleVertexShader",
      Stage = ShaderStage.Vertex,
      SourceCode = hlslCode,
      EntryPoint = "main",
      ShaderModel = "5.1"
    };

    using(var shader = new DX12Shader(shaderDesc))
    {
      Console.WriteLine($"✓ Shader compiled: {shader.Name}");
      Console.WriteLine($"  Bytecode size: {shader.Bytecode.Length} bytes");

      // Проверка рефлексии
      var reflection = shader.GetReflection();
      Console.WriteLine($"  Input parameters: {reflection.InputParameters.Count}");
      Console.WriteLine($"  Output parameters: {reflection.OutputParameters.Count}");

      foreach(var input in reflection.InputParameters)
      {
        Console.WriteLine($"    Input: {input.SemanticName}{input.SemanticIndex} (Register: {input.Register})");
      }
    }

    Console.WriteLine();
  }

  static void TestPixelShaderWithResources()
  {
    Console.WriteLine("Test 2: Pixel Shader with Resources");
    Console.WriteLine("------------------------------------");

    string hlslCode = @"
                Texture2D DiffuseTexture : register(t0);
                Texture2D NormalTexture : register(t1);
                SamplerState LinearSampler : register(s0);

                cbuffer MaterialConstants : register(b0)
                {
                    float4 DiffuseColor;
                    float4 SpecularColor;
                    float Roughness;
                    float Metallic;
                    float2 Padding;
                };

                struct PSInput
                {
                    float4 Position : SV_POSITION;
                    float2 TexCoord : TEXCOORD0;
                    float3 Normal : NORMAL;
                };

                float4 main(PSInput input) : SV_TARGET
                {
                    float4 diffuse = DiffuseTexture.Sample(LinearSampler, input.TexCoord);
                    float3 normal = NormalTexture.Sample(LinearSampler, input.TexCoord).xyz;
                    
                    // Simple shading
                    float lighting = dot(normal, float3(0, 1, 0)) * 0.5 + 0.5;
                    return diffuse * DiffuseColor * lighting;
                }
            ";

    var shaderDesc = new ShaderDescription
    {
      Name = "MaterialPixelShader",
      Stage = ShaderStage.Pixel,
      SourceCode = hlslCode,
      EntryPoint = "main",
      ShaderModel = "5.1"
    };

    using(var shader = new DX12Shader(shaderDesc))
    {
      Console.WriteLine($"✓ Shader compiled: {shader.Name}");

      var reflection = shader.GetReflection();

      Console.WriteLine($"  Constant Buffers: {reflection.ConstantBuffers.Count}");
      foreach(var cb in reflection.ConstantBuffers)
      {
        Console.WriteLine($"    - {cb.Name} (Size: {cb.Size} bytes)");
        foreach(var variable in cb.Variables)
        {
          Console.WriteLine($"      • {variable.Name}: Offset={variable.Offset}, Size={variable.Size}");
        }
      }

      Console.WriteLine($"  Textures: {reflection.BoundResources.Count}");
      foreach(var resource in reflection.BoundResources)
      {
        Console.WriteLine($"    - {resource.Name} (Slot: t{resource.BindPoint})");
      }

      Console.WriteLine($"  Samplers: {reflection.Samplers.Count}");
      foreach(var sampler in reflection.Samplers)
      {
        Console.WriteLine($"    - {sampler.Name} (Slot: s{sampler.BindPoint})");
      }

      Console.WriteLine("\n  Resource checks:");
      Console.WriteLine($"    Has 'MaterialConstants': {shader.HasConstantBuffer("MaterialConstants")}");
      Console.WriteLine($"    Has 'DiffuseTexture': {shader.HasTexture("DiffuseTexture")}");
      Console.WriteLine($"    Has 'LinearSampler': {shader.HasSampler("LinearSampler")}");
    }

    Console.WriteLine();
  }

  static void TestComputeShaderCompilation()
  {
    Console.WriteLine("Test 3: Compute Shader Compilation");
    Console.WriteLine("-----------------------------------");

    string hlslCode = @"
                RWTexture2D<float4> OutputTexture : register(u0);
                Texture2D<float4> InputTexture : register(t0);

                cbuffer ComputeParams : register(b0)
                {
                    uint2 TextureSize;
                    float Time;
                    float DeltaTime;
                };

                [numthreads(8, 8, 1)]
                void main(uint3 id : SV_DispatchThreadID)
                {
                    if (id.x >= TextureSize.x || id.y >= TextureSize.y)
                        return;
                    
                    float4 color = InputTexture[id.xy];
                    
                    // Simple color manipulation
                    color.rgb = lerp(color.rgb, color.bgr, sin(Time) * 0.5 + 0.5);
                    
                    OutputTexture[id.xy] = color;
                }
            ";

    var shaderDesc = new ShaderDescription
    {
      Name = "ImageProcessingCS",
      Stage = ShaderStage.Compute,
      SourceCode = hlslCode,
      EntryPoint = "main",
      ShaderModel = "5.1"
    };

    using(var shader = new DX12Shader(shaderDesc))
    {
      Console.WriteLine($"✓ Shader compiled: {shader.Name}");

      var reflection = shader.GetReflection();

      var threadGroup = reflection.ThreadGroupSize;
      Console.WriteLine($"  Thread Group Size: [{threadGroup.X}, {threadGroup.Y}, {threadGroup.Z}]");

      Console.WriteLine($"  UAVs: {reflection.UnorderedAccessViews.Count}");
      foreach(var uav in reflection.UnorderedAccessViews)
      {
        Console.WriteLine($"    - {uav.Name} (Slot: u{uav.BindPoint})");
      }

      Console.WriteLine($"  Has UAV 'OutputTexture': {shader.HasUnordererAccess("OutputTexture")}");
    }

    Console.WriteLine();
  }

  static void TestShaderWithMacros()
  {
    Console.WriteLine("Test 4: Shader Compilation with Macros");
    Console.WriteLine("---------------------------------------");

    string hlslCode = @"
                struct PSInput
                {
                    float4 Position : SV_POSITION;
                    float4 Color : COLOR;
                    #ifdef USE_TEXTURE
                    float2 TexCoord : TEXCOORD0;
                    #endif
                };

                #ifdef USE_TEXTURE
                Texture2D MainTexture : register(t0);
                SamplerState MainSampler : register(s0);
                #endif

                float4 main(PSInput input) : SV_TARGET
                {
                    float4 color = input.Color;
                    
                    #ifdef USE_TEXTURE
                    color *= MainTexture.Sample(MainSampler, input.TexCoord);
                    #endif
                    
                    #ifdef ENABLE_GAMMA_CORRECTION
                    color.rgb = pow(color.rgb, 1.0 / 2.2);
                    #endif
                    
                    return color;
                }
            ";

    var shaderDesc = new ShaderDescription
    {
      Name = "ShaderWithMacros",
      Stage = ShaderStage.Pixel,
      SourceCode = hlslCode,
      EntryPoint = "main",
      ShaderModel = "5.1",
      Defines = new List<ShaderMacro>
                {
                    new ShaderMacro { Name = "USE_TEXTURE", Definition = "1" },
                    new ShaderMacro { Name = "ENABLE_GAMMA_CORRECTION", Definition = "1" }
                }
    };

    using(var shader = new DX12Shader(shaderDesc))
    {
      Console.WriteLine($"✓ Shader compiled with macros: {shader.Name}");

      var reflection = shader.GetReflection();

      bool hasTexture = shader.HasTexture("MainTexture");
      bool hasSampler = shader.HasSampler("MainSampler");

      Console.WriteLine($"  Has texture (macro USE_TEXTURE): {hasTexture}");
      Console.WriteLine($"  Has sampler (macro USE_TEXTURE): {hasSampler}");

      if(!hasTexture || !hasSampler)
      {
        throw new Exception("Macros were not applied correctly!");
      }
    }

    Console.WriteLine();
  }

  static void TestShaderReflection()
  {
    Console.WriteLine("Test 5: Detailed Shader Reflection");
    Console.WriteLine("-----------------------------------");

    string hlslCode = @"
                cbuffer PerFrameData : register(b0)
                {
                    float4x4 ViewProjection;
                    float4 CameraPosition;
                    float4 LightDirection;
                    float Time;
                    float3 Padding;
                };

                cbuffer PerObjectData : register(b1)
                {
                    float4x4 World;
                    float4x4 WorldInverseTranspose;
                    float4 ObjectColor;
                };

                struct VSInput
                {
                    float3 Position : POSITION;
                    float3 Normal : NORMAL;
                    float2 TexCoord : TEXCOORD0;
                    float4 Tangent : TANGENT;
                };

                struct PSInput
                {
                    float4 Position : SV_POSITION;
                    float3 WorldPos : POSITION0;
                    float3 Normal : NORMAL;
                    float2 TexCoord : TEXCOORD0;
                    float4 Tangent : TANGENT;
                };

                PSInput main(VSInput input)
                {
                    PSInput output;
                    
                    float4 worldPos = mul(float4(input.Position, 1.0), World);
                    output.WorldPos = worldPos.xyz;
                    output.Position = mul(worldPos, ViewProjection);
                    output.Normal = mul(input.Normal, (float3x3)WorldInverseTranspose);
                    output.TexCoord = input.TexCoord;
                    output.Tangent = float4(mul(input.Tangent.xyz, (float3x3)World), input.Tangent.w);
                    
                    return output;
                }
            ";

    var shaderDesc = new ShaderDescription
    {
      Name = "DetailedVertexShader",
      Stage = ShaderStage.Vertex,
      SourceCode = hlslCode,
      EntryPoint = "main",
      ShaderModel = "5.1"
    };

    using(var shader = new DX12Shader(shaderDesc))
    {
      Console.WriteLine($"✓ Shader compiled: {shader.Name}");

      var reflection = shader.GetReflection();

      Console.WriteLine($"\n  Shader Info:");
      Console.WriteLine($"    Model: {reflection.Info.ShaderModel}");
      Console.WriteLine($"    Instructions: {reflection.Info.InstructionCount}");

      Console.WriteLine($"\n  Constant Buffers Details:");
      foreach(var cb in reflection.ConstantBuffers)
      {
        Console.WriteLine($"    {cb.Name}:");
        Console.WriteLine($"      Size: {cb.Size} bytes");
        Console.WriteLine($"      Variables:");

        foreach(var variable in cb.Variables)
        {
          Console.WriteLine($"        - {variable.Name}");
          Console.WriteLine($"          Type: {variable.Type}");
          Console.WriteLine($"          Offset: {variable.Offset} bytes");
          Console.WriteLine($"          Size: {variable.Size} bytes");
        }
      }

      Console.WriteLine($"\n  Input/Output Layout:");
      Console.WriteLine($"    Inputs: {reflection.InputParameters.Count}");
      foreach(var input in reflection.InputParameters)
      {
        Console.WriteLine($"      - {input.SemanticName}{input.SemanticIndex}");
        Console.WriteLine($"        Register: {input.Register}");
        Console.WriteLine($"        Component Type: {input.ComponentType}");
        Console.WriteLine($"        Component Mask: 0x{input.Mask:X2}");
      }

      Console.WriteLine($"    Outputs: {reflection.OutputParameters.Count}");
      foreach(var output in reflection.OutputParameters)
      {
        Console.WriteLine($"      - {output.SemanticName}{output.SemanticIndex}");
        Console.WriteLine($"        Register: {output.Register}");
      }

      var cbInfo = shader.GetConstantBufferInfo("PerFrameData");
      if(cbInfo != null)
      {
        Console.WriteLine($"\n  'PerFrameData' constant buffer:");
        Console.WriteLine($"    Total size: {cbInfo.Size} bytes");
        Console.WriteLine($"    Variable count: {cbInfo.Variables.Count}");
      }
    }

    Console.WriteLine();
  }
}