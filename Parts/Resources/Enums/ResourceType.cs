namespace Resources.Enums;

/// <summary>
/// Типы ресурсов
/// </summary>
public enum ResourceType
{
  Unknown,

  // Буферы
  Buffer,
  VertexBuffer,
  IndexBuffer,
  ConstantBuffer,
  StructuredBuffer,
  RawBuffer,
  IndirectArgsBuffer,

  // Текстуры
  Texture1D,
  Texture1DArray,
  Texture2D,
  Texture2DArray,
  Texture2DMS,        // Multisample
  Texture2DMSArray,   // Multisample Array
  Texture3D,
  TextureCube,
  TextureCubeArray,

  // GPU объекты (не ресурсы памяти, но объекты GPU)
  Shader,
  RenderState,
  Sampler,
  Query,
  Fence,
  SwapChain,

  ShaderResource,
  UnorderedAccess,

  // Представления (views) 
  TextureView,
  BufferView,
}
