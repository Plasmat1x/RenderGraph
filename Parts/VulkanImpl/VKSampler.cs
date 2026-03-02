using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using Resources.Enums;

namespace VulkanImpl;

public class VKSampler : ISampler
{
  private readonly SamplerDescription p_description;
  private bool p_disposed;

  public VKSampler(SamplerDescription _description)
  {
    p_description = _description;
  }

  public SamplerDescription Description => p_description;
  public string Name => p_description.Name;
  public ResourceType ResourceType => ResourceType.Sampler;
  public bool IsDisposed => p_disposed;
  public ulong GetMemorySize() => 16;

  public IntPtr GetNativeHandle() => IntPtr.Zero;

  public void Dispose()
  {
    if(p_disposed)
      return;
    Console.WriteLine($"  [Vulkan] Dispose sampler {p_description.Name}");
    p_disposed = true;
  }
}
