namespace GraphicsAPI.Enums;

/// <summary>
/// Дополнительные флаги для текстурных представлений
/// </summary>
[Flags]
public enum TextureViewFlags
{
  None = 0,
  ReadOnlyDepth = 1 << 0,
  ReadOnlyStencil = 1 << 1
}
