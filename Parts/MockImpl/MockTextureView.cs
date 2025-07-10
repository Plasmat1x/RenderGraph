using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockTextureView: ITextureView
{
  public MockTextureView(ITexture _texture, TextureViewDescription _description)
  {
    Texture = _texture;
    ViewType = _description.ViewType;
    Description = _description;
  }

  public ITexture Texture { get; }
  public TextureViewType ViewType { get; }
  public TextureViewDescription Description { get; }

  public IntPtr GetNativeHandle() => new IntPtr(((MockTexture)Texture).Id + (uint)ViewType * 10000);

  public void Dispose()
  {
    // Views are lightweight, minimal cleanup needed
  }
}