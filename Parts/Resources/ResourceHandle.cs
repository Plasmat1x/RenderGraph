using Core.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Resources.Data;
public struct ResourceHandle
{
  public uint Id;
  public ResourceType Type;
  public uint Generation;

  public readonly bool IsValid()
  {
    throw new NotImplementedException();
  }

  public bool Equals(ResourceHandle _other)
  {
    return _other.Id == Id 
      && _other.Type == Type
      && _other.Generation == Generation
      && _other.IsValid();
  }

}
