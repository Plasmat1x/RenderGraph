using Resources;
using Resources.Enums;

namespace GraphicsAPI.Interfaces;

public interface IBuffer : IResource, IDisposable
{
  ulong Size { get; }
  TextureUsage Usage { get; }
  uint Stride { get; }
  BufferDescription Description { get; }

  nint Map();
  void Unmap();
}
