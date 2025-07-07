using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace GraphicsAPI;

public class BufferViewStub: IBufferView
{
  public IBuffer Buffer { get; }

  public BufferViewType ViewType { get; }

  public BufferViewStub(IBuffer _buffer, BufferViewType _viewType)
  {
    Buffer = _buffer;
    ViewType = _viewType;
  }

  public void Dispose()
  {
  }
}




