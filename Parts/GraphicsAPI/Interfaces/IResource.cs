namespace GraphicsAPI;

public interface IResource : IDisposable
{
  nint GetNativeHandle();
}