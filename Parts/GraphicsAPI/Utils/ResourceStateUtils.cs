using GraphicsAPI.Enums;

using Resources.Enums;

namespace GraphicsAPI.Utils;


public static class ResourceStateUtils
{
  public static ResourceState GetInitialState(BindFlags _bindFlags, ResourceUsage _usage)
  {
    if(_usage == ResourceUsage.Dynamic)
      return ResourceState.GenericRead;

    if(_usage == ResourceUsage.Staging)
      return ResourceState.CopyDest;

    if((_bindFlags & BindFlags.DepthStencil) != 0)
      return ResourceState.DepthWrite;

    if((_bindFlags & BindFlags.RenderTarget) != 0)
      return ResourceState.RenderTarget;

    return ResourceState.Common;
  }
}