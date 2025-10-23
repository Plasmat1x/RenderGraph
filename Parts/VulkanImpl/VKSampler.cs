using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;
using Resources.Enums;

namespace VulkanImpl;

public class VKSampler : ISampler
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

  public SamplerDescription Description { get; }
}
