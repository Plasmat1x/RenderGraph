using Core.Data;
using Core.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core;
internal class RenderGraphContext
{
  public void Read(ResourceHandle _handle)
  {
    throw new NotImplementedException();
  }

  public void Write(ResourceHandle _handle)
  {
    throw new NotImplementedException();
  }

  public ResourceHandle Create(IResourceDesc _desc)
  {
    throw new NotImplementedException();
  }

  public ResourceHandle Import(IGraphicsResource _external)
  {
    throw new NotImplementedException();
  }
}
