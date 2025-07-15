using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;
public class DX12StateTracker
{
  private Dictionary<ID3D12Resource, ResourceStates> p_globalResourceStates;
  private List<ResourceBarrier> p_pendingBarriers;

  public DX12StateTracker() { }

  public void TransitionResource(ID3D12Resource _resource, ResourceStates _newState)
  {
    throw new NotImplementedException();
  }

  public void FlushPendingBarriers(ID3D12GraphicsCommandList _commandList)
  {
    throw new NotImplementedException();
  }

  public void CommitResourceState()
  {
    throw new NotImplementedException();
  }

  public void Reset()
  {
    throw new NotImplementedException();
  }
}
