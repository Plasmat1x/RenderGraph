using GraphicsAPI.Interfaces;

namespace VulkanImpl;

public class VKMonitor : IMonitor
{
  public string Name { get; }
  public int Width { get; }
  public int Height { get; }
  public int RefreshRate { get; }
  public IntPtr Handle { get; }
}
