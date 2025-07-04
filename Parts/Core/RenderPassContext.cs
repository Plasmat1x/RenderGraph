using Core.Interfaces;

using Resources.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core;
public class RenderPassContext
{
  public void Read(ResourceHandle _handle)
  {
    throw new NotImplementedException();
  }

  public void Write(ResourceHandle _handle)
  {
    throw new NotImplementedException();
  }

  public ResourceHandle Create(IResource _desc)
  {
    throw new NotImplementedException();
  }

  public ResourceHandle Import(IResource _external)
  {
    throw new NotImplementedException();
  }
}
