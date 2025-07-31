namespace Resources.Enums;

/// <summary>
/// Расширенное использование текстур
/// </summary>
[Flags]
public enum TextureUsage
{
  None = 0,
  RenderTarget = 1 << 0,
  DepthStencil = 1 << 1,
  ShaderResource = 1 << 2,
  UnorderedAccess = 1 << 3,
  BackBuffer = 1 << 4,
  Staging = 1 << 5,
  ResolveTarget = 1 << 6,
  ResolveSource = 1 << 7,
  CubeMap = 1 << 8,
  VolumeTexture = 1 << 9,

  // Комбинированные варианты для удобства
  RenderTargetAndShaderResource = RenderTarget | ShaderResource,
  DepthStencilAndShaderResource = DepthStencil | ShaderResource,
  ComputeTexture = ShaderResource | UnorderedAccess
}
