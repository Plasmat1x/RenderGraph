using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockBufferView: IBufferView
{
  public MockBufferView(IBuffer _buffer, BufferViewDescription _description)
  {
    Buffer = _buffer;
    ViewType = _description.ViewType;
    Description = _description;
  }
  public IBuffer Buffer { get; }
  public BufferViewType ViewType { get; }
  public BufferViewDescription Description { get; }


  public IntPtr GetNativeHandle() => new IntPtr(((MockBuffer)Buffer).Id + (uint)ViewType * 10000);

  public void Dispose()
  {
    // Views are lightweight, minimal cleanup needed
  }
}