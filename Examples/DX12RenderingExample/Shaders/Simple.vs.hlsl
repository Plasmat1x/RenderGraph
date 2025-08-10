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