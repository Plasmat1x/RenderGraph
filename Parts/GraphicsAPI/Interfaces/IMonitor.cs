namespace GraphicsAPI.Interfaces;
public interface IMonitor
{
  string Name { get; }
  int Width { get; }
  int Height { get; }
  int RefreshRate { get; }
  IntPtr Handle { get; }
}
