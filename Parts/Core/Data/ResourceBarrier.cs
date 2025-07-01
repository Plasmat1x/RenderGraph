using Core.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data;
internal struct ResourceBarrier
{
  public ResourceHandle Resource;
  public ResourceState Before;
  public ResourceState After;
}
