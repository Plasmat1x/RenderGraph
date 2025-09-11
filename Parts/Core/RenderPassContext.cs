using GraphicsAPI;
using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace Core;
public class RenderPassContext
{
  public CommandBuffer CommandBuffer { get; set; }
  public ResourceManager Resources { get; set; }
  public FrameData FrameData { get; set; }
  public int PassIndex { get; set; }
  public uint ViewportWidth { get; set; }
  public uint ViewportHeight { get; set; }

  public ITexture GetTexture(ResourceHandle _handle)
  {
    if(Resources == null)
      throw new InvalidOperationException("ResourceManager is not set in the context");

    return Resources.GetTexture(_handle);
  }

  public IBuffer GetBuffer(ResourceHandle _handle)
  {
    if(Resources == null)
      throw new InvalidOperationException("ResourceManager is not set in the context");

    return Resources.GetBuffer(_handle);
  }

  public ITextureView GetTextureView(ResourceHandle _handle, TextureViewType _viewType)
  {
    if(Resources == null)
      throw new InvalidOperationException("ResourceManager is not set in the context");

    var texture = Resources.GetTexture(_handle);

    var desc = new TextureViewDescription
    {
      ViewType = _viewType,
      Format = texture.Format,
      MostDetailedMip = 0,
      MipLevels = texture.MipLevels,
      FirstArraySlice = 0,
      ArraySize = texture.ArraySize
    };

    return texture.CreateView(desc);
  }

  public IBufferView GetBufferView(ResourceHandle _handle, BufferViewType _viewType)
  {
    if(Resources == null)
      throw new InvalidOperationException("ResourceManager is not set in the context");

    var buffer = Resources.GetBuffer(_handle);

    var desc = new BufferViewDescription
    {
      ViewType = _viewType,
      FirstElement = 0,
      NumElements = buffer.Size / Math.Max(buffer.Stride, 1),
      StructureByteStride = buffer.Stride
    };

    return buffer.CreateView(desc);
  }

  public void SetViewport(float _x, float _y, float _width, float _height, float _minDepth = 0.0f, float _maxDepth = 1.0f)
  {
    if(CommandBuffer == null)
      throw new InvalidOperationException("CommandBuffer is not set in the context");

    var viewport = new Viewport
    {
      X = _x,
      Y = _y,
      Width = _width,
      Height = _height,
      MinDepth = _minDepth,
      MaxDepth = _maxDepth
    };

    CommandBuffer.SetViewport(viewport);
  }

  public void SetFullScreenViewport() => SetViewport(0, 0, ViewportWidth, ViewportHeight);
}