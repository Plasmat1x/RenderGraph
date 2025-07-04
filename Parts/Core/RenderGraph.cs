
using Core.Interfaces;
using Resources.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core;
internal class RenderGraph
{
  private readonly List<RenderPass> p_renderPasses = new();
  private readonly Dictionary<ResourceHandle, IResource> p_imports = new();

  public void AddPass(string _name, Action<RenderPass> _build)
  {
    throw new NotImplementedException();
  }

  public void AddPass<T>(T _pass) where T : RenderPass
  {
    throw new NotImplementedException();
  }

  public void Import(ResourceHandle _handle, bool _external)
  {
    throw new NotImplementedException();
  }
}
