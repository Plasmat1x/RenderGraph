using Core.Data;
using Core.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core;
internal class CompiledRenderPass
{
  private readonly ICommandList p_cmdList;
  private readonly List<ResourceBarrier> p_barriers;

  public void Execute()
  {
    throw new NotImplementedException();
  }
}
