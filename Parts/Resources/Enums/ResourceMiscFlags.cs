using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources.Enums;
public enum ResourceMiscFlags
{
  None,
  GenerateMips,
  Shared,
  TextureCube,
  DrawIndirectArgs,
  BufferAllowRawViews,
  BufferStructured,
  ResourceClamp,
  SharedKeyedMutex,
  GDICompatible,
}
