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
    var texture = GetTexture(_handle);
    return new TextureViewStub(texture, _viewType);
  }

  public IBufferView GetBufferView(ResourceHandle _handle, BufferViewType _viewType)
  {
    var buffer = GetBuffer(_handle);
    return new BufferViewStub(buffer, _viewType);
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

internal class BufferViewStub: IBufferView
{
  public BufferViewStub(IBuffer _buffer, BufferViewType _viewType)
  {
    Buffer = _buffer;
    ViewType = _viewType;
  }

  public IBuffer Buffer {get;}

  public BufferViewType ViewType { get; }

  public BufferViewDescription Description => throw new NotImplementedException();

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }
}

internal class TextureViewStub: ITextureView
{
  public TextureViewStub(ITexture _texture, TextureViewType _viewType)
  {
    Texture = _texture;
    ViewType = _viewType;
  }

  public ITexture Texture {get;}

  public TextureViewType ViewType { get;}

  public _TextureViewDescription Description => throw new NotImplementedException();

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }
}