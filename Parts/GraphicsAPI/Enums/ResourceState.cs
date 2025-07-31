namespace GraphicsAPI.Enums;

/// <summary>
/// Состояния ресурсов для state tracking
/// </summary>
[Flags]
public enum ResourceState
{
  Common = 0,
  VertexAndConstantBuffer = 1 << 0,
  IndexBuffer = 1 << 1,
  RenderTarget = 1 << 2,
  UnorderedAccess = 1 << 3,
  DepthWrite = 1 << 4,
  DepthRead = 1 << 5,
  ShaderResource = 1 << 6,
  StreamOut = 1 << 7,
  IndirectArgument = 1 << 8,
  CopyDest = 1 << 9,
  CopySource = 1 << 10,
  ResolveDest = 1 << 11,
  ResolveSource = 1 << 12,
  Present = 1 << 13,
  Predication = 1 << 14,
  VideoDecodeRead = 1 << 15,
  VideoDecodeWrite = 1 << 16,
  VideoProcessRead = 1 << 17,
  VideoProcessWrite = 1 << 18,
  VideoEncodeRead = 1 << 19,
  VideoEncodeWrite = 1 << 20,

  // Комбинированные состояния
  GenericRead = VertexAndConstantBuffer | IndexBuffer | ShaderResource | IndirectArgument | CopySource,
  AllShaderResource = ShaderResource | VertexAndConstantBuffer
}
