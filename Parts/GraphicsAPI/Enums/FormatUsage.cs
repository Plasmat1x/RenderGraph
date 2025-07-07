namespace GraphicsAPI.Enums;

public enum FormatUsage
{
  RenderTarget = 1 << 0,
  DepthStencil = 1 << 1,
  ShaderResource = 1 << 2,
  UnorderedAccess = 1 << 3,
  VertexBuffer = 1 << 4,
  IndexBuffer = 1 << 5,
  TypedLoad = 1 << 6,
  TypedStore = 1 << 7,
  Blendable = 1 << 8,
  DisplayScanout = 1 << 9
}