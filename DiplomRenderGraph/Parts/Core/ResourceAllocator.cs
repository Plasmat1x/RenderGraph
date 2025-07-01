using Core.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core;
internal class ResourceAllocator
{
  public IGraphicsResource Allocate(IResourceDesc _desc)
  {
    throw new NotImplementedException();
  }

  public void Release(IGraphicsResource _resource)
  {
    throw new NotImplementedException();
  }
}
