using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
