using Core.Enums;

using GraphicsAPI.Enums;

using Resources.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core;
public class ResourceUsageInfo
{
  public ResourceHandle Handle;
  public ResourceAccessType AccessType;
  public ResourceUsage Usage;
  public ResourceState State;
  public string PassName;

  public bool IsRead()
  {
    throw new NotImplementedException();
  }

  public bool IsWrite()
  {
    throw new NotImplementedException();
  }

  public bool ConflictsWith(ResourceUsageInfo _other)
  {
    throw new NotImplementedException();
  }
}
