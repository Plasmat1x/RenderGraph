using GraphicsAPI.Interfaces;

namespace VulkanImpl;

public class VKMonitor : IMonitor
{
  public string Name { get; }
  public IntPtr Handle { get; }
  public int Width { get; }
  public int Height { get; }
  public int RefreshRate { get; }
  public int BitsPerPixel { get; }

  public VKMonitor(string _name, IntPtr _handle, int _width, int _height, int _refreshRate, int _bitsPerPixel)
  {
    Name = _name;
    Handle = _handle;
    Width = _width;
    Height = _height;
    RefreshRate = _refreshRate;
    BitsPerPixel = _bitsPerPixel;
  }

  public static VKMonitor[] GetMonitors()
  {
    return new[]
    {
      new VKMonitor("Primary Monitor", IntPtr.Zero, 1920, 1080, 60, 32)
    };
  }
}
