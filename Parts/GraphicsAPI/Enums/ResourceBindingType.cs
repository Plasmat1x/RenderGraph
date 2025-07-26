namespace GraphicsAPI.Enums;

public enum ResourceBindingType
{
  Unknown,
  Texture,
  Sampler,
  ConstantBuffer,
  StructuredBuffer,
  ByteAddressBuffer,
  UnorderedAccessView,
  RWStructuredBuffer,
  RWByteAddressBuffer
}