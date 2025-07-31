using Resources.Enums;

namespace Resources.Utils;

/// <summary>
/// Утилиты для работы с форматами
/// </summary>
public static class TextureFormatUtils
{
  /// <summary>
  /// Получить размер пикселя в байтах
  /// </summary>
  public static uint GetBytesPerPixel(TextureFormat _format)
  {
    return _format switch
    {
      // 32-bit formats
      TextureFormat.R32G32B32A32_FLOAT or
      TextureFormat.R32G32B32A32_UINT or
      TextureFormat.R32G32B32A32_SINT or
      TextureFormat.R32G32B32A32_TYPELESS => 16,

      TextureFormat.R32G32B32_FLOAT or
      TextureFormat.R32G32B32_UINT or
      TextureFormat.R32G32B32_SINT or
      TextureFormat.R32G32B32_TYPELESS => 12,

      TextureFormat.R32G32_FLOAT or
      TextureFormat.R32G32_UINT or
      TextureFormat.R32G32_SINT or
      TextureFormat.R32G32_TYPELESS => 8,

      TextureFormat.R32_FLOAT or
      TextureFormat.R32_UINT or
      TextureFormat.R32_SINT or
      TextureFormat.R32_TYPELESS => 4,

      // 16-bit formats
      TextureFormat.R16G16B16A16_FLOAT or
      TextureFormat.R16G16B16A16_UNORM or
      TextureFormat.R16G16B16A16_UINT or
      TextureFormat.R16G16B16A16_SNORM or
      TextureFormat.R16G16B16A16_SINT or
      TextureFormat.R16G16B16A16_TYPELESS => 8,

      TextureFormat.R16G16_FLOAT or
      TextureFormat.R16G16_UNORM or
      TextureFormat.R16G16_UINT or
      TextureFormat.R16G16_SNORM or
      TextureFormat.R16G16_SINT or
      TextureFormat.R16G16_TYPELESS => 4,

      TextureFormat.R16_FLOAT or
      TextureFormat.R16_UNORM or
      TextureFormat.R16_UINT or
      TextureFormat.R16_SNORM or
      TextureFormat.R16_SINT or
      TextureFormat.R16_TYPELESS => 2,

      // 8-bit formats
      TextureFormat.R8G8B8A8_UNORM or
      TextureFormat.R8G8B8A8_UNORM_SRGB or
      TextureFormat.R8G8B8A8_UINT or
      TextureFormat.R8G8B8A8_SNORM or
      TextureFormat.R8G8B8A8_SINT or
      TextureFormat.R8G8B8A8_TYPELESS or
      TextureFormat.B8G8R8A8_UNORM or
      TextureFormat.B8G8R8X8_UNORM or
      TextureFormat.B8G8R8A8_TYPELESS or
      TextureFormat.B8G8R8A8_UNORM_SRGB or
      TextureFormat.B8G8R8X8_TYPELESS or
      TextureFormat.B8G8R8X8_UNORM_SRGB => 4,

      TextureFormat.R8G8_UNORM or
      TextureFormat.R8G8_UINT or
      TextureFormat.R8G8_SNORM or
      TextureFormat.R8G8_SINT or
      TextureFormat.R8G8_TYPELESS => 2,

      TextureFormat.R8_UNORM or
      TextureFormat.R8_UINT or
      TextureFormat.R8_SNORM or
      TextureFormat.R8_SINT or
      TextureFormat.R8_TYPELESS => 1,

      // Depth formats
      TextureFormat.D32_FLOAT_S8X24_UINT => 8,
      TextureFormat.D32_FLOAT => 4,
      TextureFormat.D24_UNORM_S8_UINT => 4,
      TextureFormat.D16_UNORM => 2,

      // Special formats
      TextureFormat.R11G11B10_FLOAT or
      TextureFormat.R10G10B10A2_UNORM or
      TextureFormat.R10G10B10A2_UINT or
      TextureFormat.R10G10B10A2_TYPELESS => 4,

      // Compressed formats (block size)
      TextureFormat.BC1_UNORM or
      TextureFormat.BC1_UNORM_SRGB or
      TextureFormat.BC1_TYPELESS or
      TextureFormat.BC4_UNORM or
      TextureFormat.BC4_SNORM or
      TextureFormat.BC4_TYPELESS => 8, // 8 bytes per 4x4 block

      TextureFormat.BC2_UNORM or
      TextureFormat.BC2_UNORM_SRGB or
      TextureFormat.BC2_TYPELESS or
      TextureFormat.BC3_UNORM or
      TextureFormat.BC3_UNORM_SRGB or
      TextureFormat.BC3_TYPELESS or
      TextureFormat.BC5_UNORM or
      TextureFormat.BC5_SNORM or
      TextureFormat.BC5_TYPELESS or
      TextureFormat.BC6H_UF16 or
      TextureFormat.BC6H_SF16 or
      TextureFormat.BC6H_TYPELESS or
      TextureFormat.BC7_UNORM or
      TextureFormat.BC7_UNORM_SRGB or
      TextureFormat.BC7_TYPELESS => 16, // 16 bytes per 4x4 block

      _ => 0
    };
  }

  /// <summary>
  /// Проверить является ли формат сжатым
  /// </summary>
  public static bool IsCompressed(TextureFormat _format)
  {
    return _format switch
    {
      TextureFormat.BC1_TYPELESS or
      TextureFormat.BC1_UNORM or
      TextureFormat.BC1_UNORM_SRGB or
      TextureFormat.BC2_TYPELESS or
      TextureFormat.BC2_UNORM or
      TextureFormat.BC2_UNORM_SRGB or
      TextureFormat.BC3_TYPELESS or
      TextureFormat.BC3_UNORM or
      TextureFormat.BC3_UNORM_SRGB or
      TextureFormat.BC4_TYPELESS or
      TextureFormat.BC4_UNORM or
      TextureFormat.BC4_SNORM or
      TextureFormat.BC5_TYPELESS or
      TextureFormat.BC5_UNORM or
      TextureFormat.BC5_SNORM or
      TextureFormat.BC6H_TYPELESS or
      TextureFormat.BC6H_UF16 or
      TextureFormat.BC6H_SF16 or
      TextureFormat.BC7_TYPELESS or
      TextureFormat.BC7_UNORM or
      TextureFormat.BC7_UNORM_SRGB => true,
      _ => false
    };
  }

  /// <summary>
  /// Проверить является ли формат depth/stencil
  /// </summary>
  public static bool IsDepthStencil(TextureFormat _format)
  {
    return _format switch
    {
      TextureFormat.D32_FLOAT_S8X24_UINT or
      TextureFormat.D32_FLOAT or
      TextureFormat.D24_UNORM_S8_UINT or
      TextureFormat.D16_UNORM => true,
      _ => false
    };
  }

  /// <summary>
  /// Проверить поддерживает ли формат SRGB
  /// </summary>
  public static bool IsSRGB(TextureFormat _format)
  {
    return _format switch
    {
      TextureFormat.R8G8B8A8_UNORM_SRGB or
      TextureFormat.BC1_UNORM_SRGB or
      TextureFormat.BC2_UNORM_SRGB or
      TextureFormat.BC3_UNORM_SRGB or
      TextureFormat.BC7_UNORM_SRGB or
      TextureFormat.B8G8R8A8_UNORM_SRGB or
      TextureFormat.B8G8R8X8_UNORM_SRGB => true,
      _ => false
    };
  }

  /// <summary>
  /// Получить типизированную версию формата
  /// </summary>
  public static TextureFormat GetTypelessFormat(TextureFormat _format)
  {
    return _format switch
    {
      TextureFormat.R32G32B32A32_FLOAT or
      TextureFormat.R32G32B32A32_UINT or
      TextureFormat.R32G32B32A32_SINT => TextureFormat.R32G32B32A32_TYPELESS,

      TextureFormat.R32G32B32_FLOAT or
      TextureFormat.R32G32B32_UINT or
      TextureFormat.R32G32B32_SINT => TextureFormat.R32G32B32_TYPELESS,

      TextureFormat.R32G32_FLOAT or
      TextureFormat.R32G32_UINT or
      TextureFormat.R32G32_SINT => TextureFormat.R32G32_TYPELESS,

      TextureFormat.R32_FLOAT or
      TextureFormat.R32_UINT or
      TextureFormat.R32_SINT => TextureFormat.R32_TYPELESS,

      TextureFormat.R16G16B16A16_FLOAT or
      TextureFormat.R16G16B16A16_UNORM or
      TextureFormat.R16G16B16A16_UINT or
      TextureFormat.R16G16B16A16_SNORM or
      TextureFormat.R16G16B16A16_SINT => TextureFormat.R16G16B16A16_TYPELESS,

      TextureFormat.R8G8B8A8_UNORM or
      TextureFormat.R8G8B8A8_UNORM_SRGB or
      TextureFormat.R8G8B8A8_UINT or
      TextureFormat.R8G8B8A8_SNORM or
      TextureFormat.R8G8B8A8_SINT => TextureFormat.R8G8B8A8_TYPELESS,

      _ => _format
    };
  }
}
