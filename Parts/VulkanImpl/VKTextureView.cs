using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;
using Resources.Enums;

namespace VulkanImpl;

public class VKTextureView : ITextureView
{
  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public ITexture Texture { get; }
  public TextureViewType ViewType { get; }
  public TextureViewDescription Description { get; }

  public IntPtr GetNativeHandle()
  {
    throw new NotImplementedException();
  }
}
