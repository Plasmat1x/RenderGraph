using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockTextureView: ITextureView
{
  public ITexture Texture { get; }
  public TextureViewType ViewType { get; }
  public TextureViewDescription Description { get; }

  public MockTextureView(ITexture texture, TextureViewDescription description)
  {
    Texture = texture;
    ViewType = description.ViewType;
    Description = description;
  }

  public IntPtr GetNativeHandle() => new IntPtr(((MockTexture)Texture).Id + (uint)ViewType * 10000);

  public void Dispose()
  {
    // Views are lightweight, minimal cleanup needed
  }
}