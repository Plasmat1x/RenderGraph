using Resources.Enums;

namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс view текстуры
/// </summary>
public interface ITextureView: IDisposable
{
  ITexture Texture { get; }
  TextureViewType ViewType { get; }
  TextureViewDescription Description { get; }
  IntPtr GetNativeHandle();
}
