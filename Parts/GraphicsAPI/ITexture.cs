using Resources.Enums;

namespace GraphicsAPI;

public interface ITexture : IDisposable
{
  uint Width { get; }
  uint Height { get; }
  TextureFormat Format { get; }
  uint MipLevels { get; }
  uint ArraySize { get; }

  nint GetNativeHandle();
}
