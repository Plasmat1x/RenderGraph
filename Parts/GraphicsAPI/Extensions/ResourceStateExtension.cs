using GraphicsAPI.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI.Extensions;
public static class ResourceStateExtension
{
  public static bool IsReadOnlyState(this ResourceState _state)
  {
    var readOnlyStates = ResourceState.VertexAndConstantBuffer |
                        ResourceState.IndexBuffer |
                        ResourceState.ShaderResource |
                        ResourceState.IndirectArgument |
                        ResourceState.CopySource;

    return (_state & readOnlyStates) == _state;
  }

  public static bool Compatible(this ResourceState _state, ResourceState _other)
  {

    var readOnlyStates = ResourceState.VertexAndConstantBuffer |
                        ResourceState.IndexBuffer |
                        ResourceState.ShaderResource |
                        ResourceState.IndirectArgument |
                        ResourceState.CopySource;

    if((_state & readOnlyStates) == _state && (_other & readOnlyStates) == _other)
      return true;


    if(_state == ResourceState.Common || _other == ResourceState.Common)
      return true;

    return _state == _other;
  }
}
