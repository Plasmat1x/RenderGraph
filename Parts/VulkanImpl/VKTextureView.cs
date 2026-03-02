using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using Resources.Enums;

namespace VulkanImpl;

public class VKTextureView : ITextureView
{
  private readonly TextureViewDescription p_description;
  private bool p_disposed;

  public VKTextureView(TextureViewDescription _description)
  {
    p_description = _description;
  }

  public TextureViewDescription Description => p_description;
  public ITexture? Texture => null;
  public TextureViewType ViewType => p_description.ViewType;
  public TextureFormat Format => p_description.Format;
  public ResourceType ResourceType => ResourceType.Texture2D;

  public IntPtr GetNativeHandle() => IntPtr.Zero;

  public void Dispose()
  {
    if(p_disposed)
      return;
    p_disposed = true;
  }
}
