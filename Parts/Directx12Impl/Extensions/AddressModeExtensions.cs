using GraphicsAPI.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class AddressModeExtensions
{
  public static TextureAddressMode Convert(this AddressMode _mode) => _mode switch
  {
    AddressMode.Wrap => TextureAddressMode.Wrap,
    AddressMode.Mirror => TextureAddressMode.Mirror,
    AddressMode.Clamp => TextureAddressMode.Clamp,
    AddressMode.Border => TextureAddressMode.Border,
    AddressMode.MirrorOnce => TextureAddressMode.MirrorOnce,
    _ => throw new ArgumentException($"Unsupported texture address mode: {_mode}")
  };
}
