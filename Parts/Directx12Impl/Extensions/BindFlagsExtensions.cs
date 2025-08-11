using Resources.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class BindFlagsExtensions
{
  public static ResourceFlags Convert(this BindFlags _flags)
  {
    var result = ResourceFlags.None;

    if((_flags & BindFlags.RenderTarget) != 0)
      result |= ResourceFlags.AllowRenderTarget;
    if((_flags & BindFlags.DepthStencil) != 0)
      result |= ResourceFlags.AllowDepthStencil;
    if((_flags & BindFlags.UnorderedAccess) != 0)
      result |= ResourceFlags.AllowUnorderedAccess;

    if((_flags & BindFlags.DepthStencil) != 0)
      result |= ResourceFlags.DenyShaderResource;

    return result;
  }
}
