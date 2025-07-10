namespace GraphicsAPI.Enums;

[Flags]
public enum ClearFlags
{
  Depth = 1 << 0,
  Stencil = 1 << 1,
  DepthStencil = Depth | Stencil
}