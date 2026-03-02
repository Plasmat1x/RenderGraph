using Resources.Enums;
using Silk.NET.Vulkan;

namespace VulkanImpl.Extensions;

public static class TextureFormatExtensions
{
  public static Format ToVk(this TextureFormat _format)
  {
    // Basic format mapping - more can be added as needed
    return _format switch
    {
      TextureFormat.Unknown => Format.Undefined,
      TextureFormat.R8G8B8A8_UNORM => Format.R8G8B8A8Unorm,
      TextureFormat.R8G8B8A8_UNORM_SRGB => Format.R8G8B8A8Srgb,
      TextureFormat.R32G32B32A32_FLOAT => Format.R32G32B32A32Sfloat,
      TextureFormat.R16G16B16A16_FLOAT => Format.R16G16B16A16Sfloat,
      TextureFormat.D32_FLOAT => Format.D32Sfloat,
      TextureFormat.D24_UNORM_S8_UINT => Format.D24UnormS8Uint,
      TextureFormat.D16_UNORM => Format.D16Unorm,
      _ => Format.Undefined
    };
  }

  public static TextureFormat ToGraphicsAPI(this Format _format)
  {
    return _format switch
    {
      Format.Undefined => TextureFormat.Unknown,
      Format.R8G8B8A8Unorm => TextureFormat.R8G8B8A8_UNORM,
      Format.R8G8B8A8Srgb => TextureFormat.R8G8B8A8_UNORM_SRGB,
      Format.R32G32B32A32Sfloat => TextureFormat.R32G32B32A32_FLOAT,
      Format.R16G16B16A16Sfloat => TextureFormat.R16G16B16A16_FLOAT,
      Format.D32Sfloat => TextureFormat.D32_FLOAT,
      Format.D24UnormS8Uint => TextureFormat.D24_UNORM_S8_UINT,
      Format.D16Unorm => TextureFormat.D16_UNORM,
      _ => TextureFormat.Unknown
    };
  }
}
