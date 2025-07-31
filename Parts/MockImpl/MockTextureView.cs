using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockTextureView: ITextureView
{
  public MockTextureView(ITexture _texture, _TextureViewDescription _description)
  {
    Texture = _texture;
    ViewType = _description.ViewType;
    Description = _description;
  }

  public ITexture Texture { get; }
  public TextureViewType ViewType { get; }
  public _TextureViewDescription Description { get; }

  public IntPtr GetNativeHandle() => new IntPtr(((MockTexture)Texture).Id + (uint)ViewType * 10000);

  public void Dispose()
  {
    // Views are lightweight, minimal cleanup needed
  }
}