using GraphicsAPI;
using Resources;

namespace Core;
public class ResourceManager
{
  private readonly Dictionary<ResourceHandle, IResource> p_resources;
  private readonly List<ITexture> p_texturePool;
  private readonly List<IBuffer> p_bufferPool;
  public ResourceHandle CreateBuffer(BufferDescription _desc)
  {
    throw new NotImplementedException();
  }
  public ResourceHandle CreateTexture(TextureDescription _desc)
  {
    throw new NotImplementedException();
  }

  public ITexture GetTexture(ResourceHandle _handle)
  {
    throw new NotImplementedException();
  }

  public IBuffer GetBuffer(ResourceHandle _handle)
  {
    throw new NotImplementedException();
  }


  public void Release(IResource _resource)
  {
    throw new NotImplementedException();
  }
}
