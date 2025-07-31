namespace Resources.Enums;

/// <summary>
/// Флаги привязки ресурсов
/// </summary>
[Flags]
public enum BindFlags
{
  None = 0,
  VertexBuffer = 1 << 0,
  IndexBuffer = 1 << 1,
  ConstantBuffer = 1 << 2,
  ShaderResource = 1 << 3,
  StreamOutput = 1 << 4,
  RenderTarget = 1 << 5,
  DepthStencil = 1 << 6,
  UnorderedAccess = 1 << 7,
  Decoder = 1 << 8,
  VideoEncoder = 1 << 9
}
