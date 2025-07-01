using Core.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core;
internal class RenderPass
{
  string Name { get; init; }

  private readonly List<ResourceHandle> p_reads = new();
  private readonly List<ResourceHandle> p_writes = new();
  private readonly List<RenderPass> p_deps = new();

  public void Setup(RenderGraphContext _ctx)
  {
    throw new NotImplementedException();
  }
  public void Execute(RenderGraphContext _ctx)
  {
    throw new NotImplementedException();
  }
}
