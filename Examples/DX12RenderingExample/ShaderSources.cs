namespace Examples;

/// <summary>
/// Пример HLSL шейдеров
/// </summary>
public static class ShaderSources
{
  public const string SimpleVertexShader = @"
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

        cbuffer Constants : register(b0)
        {
            float4x4 WorldViewProj;
        };

        PSInput VSMain(VSInput input)
        {
            PSInput output;
            output.Position = mul(float4(input.Position, 1.0f), WorldViewProj);
            output.Color = input.Color;
            return output;
        }
    ";

  public const string SimplePixelShader = @"
        struct PSInput
        {
            float4 Position : SV_POSITION;
            float4 Color : COLOR;
        };

        float4 PSMain(PSInput input) : SV_TARGET
        {
            return input.Color;
        }
    ";
}