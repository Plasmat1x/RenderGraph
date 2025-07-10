using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace GraphicsAPI;

public class BufferViewStub: IBufferView
{
  public BufferViewStub(IBuffer _buffer, BufferViewType _viewType)
  {
    Buffer = _buffer;
    ViewType = _viewType;
  }
  public IBuffer Buffer { get; }
  public BufferViewType ViewType { get; }
  public BufferViewDescription Description => throw new NotImplementedException();

  public nint GetNativeHandle() => throw new NotImplementedException();
  public void Dispose()
  {
  }
}




