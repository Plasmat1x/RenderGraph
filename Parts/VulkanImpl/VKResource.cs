using GraphicsAPI;
using Resources.Enums;

namespace VulkanImpl;

public class VKResource : IResource
{
  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public string Name { get; }
  public ResourceType ResourceType { get; }
  public bool IsDisposed { get; }

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public IntPtr GetNativeHandle()
  {
    throw new NotImplementedException();
  }
}
