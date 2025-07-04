using Resources.Enums;

namespace GraphicsAPI;

public interface IBuffer : IDisposable
{
  ulong Size { get; }
  TextureUsage Usage { get; }
  uint Stride { get; }

  nint Map();
  void Unmap();
  nint GetNativeHandle();
}
