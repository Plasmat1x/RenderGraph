namespace GraphicsAPI.Enums;

public enum ResourceState
{
  Undefined,
  Common,
  RenderTarget,
  UnorderedAccess,
  DepthWrite,
  DepthRead,
  ShaderResource,
  StreamOut,
  IndirectArgument,
  CopyDestination,
  CopySource,
  ResolveDestination,
  ResolveSource,
  Present,
}
