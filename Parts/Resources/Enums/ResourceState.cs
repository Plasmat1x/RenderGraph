using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources.Enums;

public enum ResourceState
{
  Undefined,
  RenderTarget,
  ShaderResource,
  UnorderedAccess,
  DepthWrite,
  DepthRead,
  Present
}
