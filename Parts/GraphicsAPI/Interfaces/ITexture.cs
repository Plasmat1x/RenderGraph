using Resources;
using Resources.Enums;

namespace GraphicsAPI.Interfaces;

public interface ITexture : IResource, IDisposable
{
  uint Width { get; }
  uint Height { get; }
  TextureFormat Format { get; }
  uint MipLevels { get; }
  uint ArraySize { get; }
  TextureDescription Description { get; }
}
