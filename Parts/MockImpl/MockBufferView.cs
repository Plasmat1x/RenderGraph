using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockBufferView: IBufferView
{
  public IBuffer Buffer { get; }
  public BufferViewType ViewType { get; }
  public BufferViewDescription Description { get; }

  public MockBufferView(IBuffer buffer, BufferViewDescription description)
  {
    Buffer = buffer;
    ViewType = description.ViewType;
    Description = description;
  }

  public IntPtr GetNativeHandle() => new IntPtr(((MockBuffer)Buffer).Id + (uint)ViewType * 10000);

  public void Dispose()
  {
    // Views are lightweight, minimal cleanup needed
  }
}