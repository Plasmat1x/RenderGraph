namespace GraphicsAPI.Enums;

public enum ResourceBindingType
{
  Unknown,
  Texture,
  Sampler,
  UnorderedAccess,
  ShaderResource,
  ConstantBuffer,
  StructuredBuffer,
  ByteAddressBuffer,
  UnorderedAccessView,
  RWStructuredBuffer,
  RWByteAddressBuffer,
  TextureBuffer,
  AppendStructuredBuffer,
  ConsumeStructuredBuffer,
  RWStructuredBufferWithCounter
}