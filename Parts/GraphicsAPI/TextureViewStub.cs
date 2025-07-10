using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace GraphicsAPI;
public class TextureViewStub: ITextureView
{
  public TextureViewStub(ITexture _texture, TextureViewType _viewType)
  {
    Texture = _texture;
    ViewType = _viewType;
  }
  public ITexture Texture { get; }
  public TextureViewType ViewType { get; }

  public TextureViewDescription Description => throw new NotImplementedException();

  public void Dispose()
  {

  }

  public nint GetNativeHandle() => throw new NotImplementedException();
}
