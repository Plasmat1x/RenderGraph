using Core.Data;
using Core.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core;
internal class CompiledRenderGraph
{
  private readonly List<CompiledRenderPass> p_passes = new();
  private readonly Dictionary<ResourceHandle, PhysicalResourceId> p_resources = new();
  private readonly Dictionary<PhysicalResourceId, IGraphicsResource> p_aliases = new();

  public void Execute()
  {
    throw new NotImplementedException();
  }
}
