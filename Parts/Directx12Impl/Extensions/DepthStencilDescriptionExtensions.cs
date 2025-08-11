using GraphicsAPI.Descriptions;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class DepthStencilDescriptionExtensions
{
  public static DepthStencilDesc Convert(this DepthStencilStateDescription _desc)
  {
    if(_desc == null)
      _desc = new DepthStencilStateDescription();

    return new DepthStencilDesc
    {
      DepthEnable = _desc.DepthEnable,
      DepthWriteMask = _desc.DepthWriteEnable
            ? DepthWriteMask.All
            : DepthWriteMask.Zero,
      DepthFunc = _desc.DepthFunction.Convert(),
      StencilEnable = _desc.StencilEnable,
      StencilReadMask = _desc.StencilReadMask,
      StencilWriteMask = _desc.StencilWriteMask,
      FrontFace = _desc.FrontFace.Convert(),
      BackFace = _desc.BackFace.Convert()
    };
  }
}
