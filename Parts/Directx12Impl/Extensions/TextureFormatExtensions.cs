using Resources.Enums;

using Silk.NET.DXGI;

namespace Directx12Impl.Extensions;

public static class TextureFormatExtensions
{
  public static Format Convert(this TextureFormat _format) => _format switch
  {
    TextureFormat.Unknown => Format.FormatUnknown,
    TextureFormat.R32G32B32A32_TYPELESS => Format.FormatR32G32B32A32Typeless,
    TextureFormat.R32G32B32A32_FLOAT => Format.FormatR32G32B32A32Float,
    TextureFormat.R32G32B32A32_UINT => Format.FormatR32G32B32A32Uint,
    TextureFormat.R32G32B32A32_SINT => Format.FormatR32G32B32A32Sint,
    TextureFormat.R32G32B32_FLOAT => Format.FormatR32G32B32Float,
    TextureFormat.R8G8B8A8_TYPELESS => Format.FormatR8G8B8A8Typeless,
    TextureFormat.R8G8B8A8_UNORM => Format.FormatR8G8B8A8Unorm,
    TextureFormat.R8G8B8A8_UNORM_SRGB => Format.FormatR8G8B8A8UnormSrgb,
    TextureFormat.R8G8B8A8_UINT => Format.FormatR8G8B8A8Uint,
    TextureFormat.R8G8B8A8_SNORM => Format.FormatR8G8B8A8SNorm,
    TextureFormat.R8G8B8A8_SINT => Format.FormatR8G8B8A8Sint,
    TextureFormat.D32_FLOAT => Format.FormatD32Float,
    TextureFormat.D24_UNORM_S8_UINT => Format.FormatD24UnormS8Uint,
    TextureFormat.D16_UNORM => Format.FormatD16Unorm,
    TextureFormat.BC1_TYPELESS => Format.FormatBC1Typeless,
    TextureFormat.BC1_UNORM => Format.FormatBC1Unorm,
    TextureFormat.BC1_UNORM_SRGB => Format.FormatBC1UnormSrgb,
    TextureFormat.BC3_TYPELESS => Format.FormatBC3Typeless,
    TextureFormat.BC3_UNORM => Format.FormatBC3Unorm,
    TextureFormat.BC3_UNORM_SRGB => Format.FormatBC3UnormSrgb,
    TextureFormat.BC7_TYPELESS => Format.FormatBC7Typeless,
    TextureFormat.BC7_UNORM => Format.FormatBC7Unorm,
    TextureFormat.BC7_UNORM_SRGB => Format.FormatBC7UnormSrgb,
    _ => throw new ArgumentException($"Unsupported texture format: {_format}")
  };
}
