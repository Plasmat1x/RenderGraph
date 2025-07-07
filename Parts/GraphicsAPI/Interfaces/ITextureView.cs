using Resources.Enums;

namespace GraphicsAPI.Interfaces;

public interface ITextureView : IDisposable
{
  ITexture Texture { get; }
  TextureViewType ViewType { get; }
}
