namespace GraphicsAPI.Enums;

/// <summary>
/// Флаги для очистки depth/stencil буферов
/// </summary>
[Flags]
public enum ClearFlags
{
  Depth = 1 << 0,
  Stencil = 1 << 1,
  DepthAndStencil = Depth | Stencil
}