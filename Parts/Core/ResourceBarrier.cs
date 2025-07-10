using GraphicsAPI.Enums;

namespace Core;
public struct ResourceBarrier
{
  public ResourceHandle Resource;
  public ResourceState Before;
  public ResourceState After;
}
