using Resources.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI.Utils;
internal static class Toolbox
{
  public static uint GetFormatSize(TextureFormat _format) => _format switch
  {
    TextureFormat.R32G32B32A32_TYPELESS or
    TextureFormat.R32G32B32A32_FLOAT or
    TextureFormat.R32G32B32A32_UINT or
    TextureFormat.R32G32B32A32_SINT => 16,

    TextureFormat.R32G32B32_TYPELESS or
    TextureFormat.R32G32B32_FLOAT or
    TextureFormat.R32G32B32_UINT or
    TextureFormat.R32G32B32_SINT => 12,

    TextureFormat.R16G16B16A16_TYPELESS or
    TextureFormat.R16G16B16A16_FLOAT or
    TextureFormat.R16G16B16A16_UNORM or
    TextureFormat.R16G16B16A16_UINT or
    TextureFormat.R16G16B16A16_SNORM or
    TextureFormat.R16G16B16A16_SINT or
    TextureFormat.R32G32_TYPELESS or
    TextureFormat.R32G32_FLOAT or
    TextureFormat.R32G32_UINT or
    TextureFormat.R32G32_SINT => 8,

    TextureFormat.R10G10B10A2_TYPELESS or
    TextureFormat.R10G10B10A2_UNORM or
    TextureFormat.R10G10B10A2_UINT or
    TextureFormat.R11G11B10_FLOAT or
    TextureFormat.R8G8B8A8_TYPELESS or
    TextureFormat.R8G8B8A8_UNORM or
    TextureFormat.R8G8B8A8_UNORM_SRGB or
    TextureFormat.R8G8B8A8_UINT or
    TextureFormat.R8G8B8A8_SNORM or
    TextureFormat.R8G8B8A8_SINT or
    TextureFormat.R16G16_TYPELESS or
    TextureFormat.R16G16_FLOAT or
    TextureFormat.R16G16_UNORM or
    TextureFormat.R16G16_UINT or
    TextureFormat.R16G16_SNORM or
    TextureFormat.R16G16_SINT or
    TextureFormat.R32_TYPELESS or
    TextureFormat.D32_FLOAT or
    TextureFormat.R32_FLOAT or
    TextureFormat.R32_UINT or
    TextureFormat.R32_SINT or
    TextureFormat.R24G8_TYPELESS or
    TextureFormat.D24_UNORM_S8_UINT or
    TextureFormat.R24_UNORM_X8_TYPELESS or
    TextureFormat.X24_TYPELESS_G8_UINT => 4,

    TextureFormat.R8G8_TYPELESS or
    TextureFormat.R8G8_UNORM or
    TextureFormat.R8G8_UINT or
    TextureFormat.R8G8_SNORM or
    TextureFormat.R8G8_SINT or
    TextureFormat.R16_TYPELESS or
    TextureFormat.R16_FLOAT or
    TextureFormat.D16_UNORM or
    TextureFormat.R16_UNORM or
    TextureFormat.R16_UINT or
    TextureFormat.R16_SNORM or
    TextureFormat.R16_SINT => 2,

    TextureFormat.R8_TYPELESS or
    TextureFormat.R8_UNORM or
    TextureFormat.R8_UINT or
    TextureFormat.R8_SNORM or
    TextureFormat.R8_SINT or
    TextureFormat.A8_UNORM => 1,

    TextureFormat.BC1_TYPELESS or
    TextureFormat.BC1_UNORM or
    TextureFormat.BC1_UNORM_SRGB or
    TextureFormat.BC4_TYPELESS or
    TextureFormat.BC4_UNORM or
    TextureFormat.BC4_SNORM => 8,

    TextureFormat.BC2_TYPELESS or
    TextureFormat.BC2_UNORM or
    TextureFormat.BC2_UNORM_SRGB or
    TextureFormat.BC3_TYPELESS or
    TextureFormat.BC3_UNORM or
    TextureFormat.BC3_UNORM_SRGB or
    TextureFormat.BC5_TYPELESS or
    TextureFormat.BC5_UNORM or
    TextureFormat.BC5_SNORM or
    TextureFormat.BC6H_TYPELESS or
    TextureFormat.BC6H_UF16 or
    TextureFormat.BC6H_SF16 or
    TextureFormat.BC7_TYPELESS or
    TextureFormat.BC7_UNORM or
    TextureFormat.BC7_UNORM_SRGB => 16,

    _ => 0
  };
}
