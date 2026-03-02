using GraphicsAPI.Interfaces;

namespace MockImpl;

public class MockMonitor : IMonitor
{
  public string Name { get; }
  public int Width { get; }
  public int Height { get; }
  public int RefreshRate { get; }
  public IntPtr Handle { get; }
  public int BitsPerPixel { get; }

  public MockMonitor(string _name, int _width, int _height, int _refreshRate, int _bitsPerPixel, bool _isPrimary = false)
  {
    Name = _name;
    Width = _width;
    Height = _height;
    RefreshRate = _refreshRate;
    BitsPerPixel = _bitsPerPixel;
    Handle = _isPrimary ? new IntPtr(1) : IntPtr.Zero;
  }
}
