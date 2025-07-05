using GraphicsAPI;

using Resources.Enums;

namespace Core;
public class RenderPassContext
{
  public ICommandBuffer CommandBuffer;
  public ResourceManager Resources;
  public FrameData FrameData;
  public int PassIndex;
  public uint ViewportWidth;
  public uint ViewportHeight;

  public ITexture GetTexture(ResourceHandle _handle)
  {
    throw new NotImplementedException();
  }

  public IBuffer GetBuffer(ResourceHandle _handle)
  {
    throw new NotImplementedException();
  }

  public ITextureView GetTextureView(ResourceHandle _handle, TextureViewType _viewType)
  {
    throw new NotImplementedException();
  }

  public IBufferView GetBufferView(ResourceHandle _handle, BufferViewType _viewType)
  {
    throw new NotImplementedException();
  }
}
