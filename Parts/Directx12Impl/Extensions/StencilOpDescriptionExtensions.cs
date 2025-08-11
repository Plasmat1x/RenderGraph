using GraphicsAPI.Descriptions;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class StencilOpDescriptionExtensions
{
  public static DepthStencilopDesc Convert(this StencilOpDescription _desc)
  {
    return new DepthStencilopDesc
    {
      StencilFailOp = _desc.StencilFailOp.Convert(),
      StencilDepthFailOp = _desc.StencilDepthFailOp.Convert(),
      StencilPassOp = _desc.StencilPassOp.Convert(),
      StencilFunc = _desc.StencilFunction.Convert()
    };
  }
}
