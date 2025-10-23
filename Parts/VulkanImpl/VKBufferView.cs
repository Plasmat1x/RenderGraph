using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;
using Resources.Enums;

namespace VulkanImpl;

public class VKBufferView : IBufferView
{
  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public IBuffer Buffer { get; }
  public BufferViewType ViewType { get; }
  public BufferViewDescription Description { get; }

  public IntPtr GetNativeHandle()
  {
    throw new NotImplementedException();
  }
}
