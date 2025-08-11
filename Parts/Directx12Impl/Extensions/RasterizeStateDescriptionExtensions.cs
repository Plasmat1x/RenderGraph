using GraphicsAPI.Descriptions;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class RasterizeStateDescriptionExtensions
{
  public static RasterizerDesc Convert(this RasterizerStateDescription _desc)
  {
    if(_desc == null)
      _desc = new RasterizerStateDescription();

    return new RasterizerDesc
    {
      FillMode = _desc.FillMode == GraphicsAPI.Enums.FillMode.Solid ? Silk.NET.Direct3D12.FillMode.Solid : Silk.NET.Direct3D12.FillMode.Wireframe,
      CullMode = _desc.CullMode.Convert(),
      FrontCounterClockwise = _desc.FrontCounterClockwise,
      DepthBias = _desc.DepthBias,
      DepthBiasClamp = _desc.DepthBiasClamp,
      SlopeScaledDepthBias = _desc.SlopeScaledDepthBias,
      DepthClipEnable = _desc.DepthClipEnable,
      MultisampleEnable = _desc.MultisampleEnable,
      AntialiasedLineEnable = _desc.AntialiasedLineEnable,
      ForcedSampleCount = 0,
      ConservativeRaster = ConservativeRasterizationMode.Off
    };
  }
}