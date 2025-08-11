using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl.Parts;
public class DX12ResourceStateTracker: IDisposable
{
  private static readonly Dictionary<ComPtr<ID3D12Resource>, ResourceStates> s_globalResourceStates = [];
  private static readonly object s_globalMutex = new();

  private readonly Dictionary<ComPtr<ID3D12Resource>, ResourceStates> p_finalResourceStates = [];
  private readonly Dictionary<ComPtr<ID3D12Resource>, ResourceStates> p_pendingResourceStates = [];
  private List<ResourceBarrier> p_pendingResourceBarriers = [];
  private List<ResourceBarrier> p_resourceBarriers = [];

  private bool p_disposed;

  public DX12ResourceStateTracker() { }

  public unsafe void TransitionResource(ComPtr<ID3D12Resource> _resource, ResourceStates _stateAfter, uint _subresource = D3D12.ResourceBarrierAllSubresources)
  {
    if(_resource.Handle == null)
      return;

    ResourceStates stateBefore;

    if(p_finalResourceStates.TryGetValue(_resource, out stateBefore))
    {
      if(stateBefore != _stateAfter)
      {
        AddResourceBarrier(_resource, stateBefore, _stateAfter, _subresource);
        p_finalResourceStates[_resource] = _stateAfter;
      }
    }
    else
    {
      p_pendingResourceStates[_resource] = _stateAfter;

      var barrier = new ResourceBarrier
      {
        Type = ResourceBarrierType.Transition,
        Flags = ResourceBarrierFlags.None,
      };

      barrier.Anonymous.Transition.PResource = _resource;
      barrier.Anonymous.Transition.StateAfter = _stateAfter;
      barrier.Anonymous.Transition.Subresource = _subresource;

      p_pendingResourceBarriers.Add(barrier);
    }
  }

  public unsafe void UAVBarrier(ComPtr<ID3D12Resource>? _resource = null)
  {
    var barrier = new ResourceBarrier
    {
      Type = ResourceBarrierType.Uav,
      Flags = ResourceBarrierFlags.None,
    };
    barrier.Anonymous.UAV.PResource = _resource.Value;
    p_resourceBarriers.Add(barrier);
  }

  public unsafe void AliasBarrier(ComPtr<ID3D12Resource> _resourceBefore, ComPtr<ID3D12Resource> _resourceAfter)
  {
    var barrier = new ResourceBarrier
    {
      Type = ResourceBarrierType.Aliasing,
      Flags = ResourceBarrierFlags.None,
    };
    barrier.Anonymous.Aliasing.PResourceBefore = _resourceBefore;
    barrier.Anonymous.Aliasing.PResourceAfter = _resourceAfter;

    p_resourceBarriers.Add(barrier);
  }

  public unsafe void FlushResourceBarriers(ComPtr<ID3D12GraphicsCommandList> _commandList)
  {
    if(p_resourceBarriers.Count == 0)
      return;

    fixed(ResourceBarrier* pBarriers = p_resourceBarriers.ToArray())
    {
      _commandList.ResourceBarrier((uint)p_resourceBarriers.Count, pBarriers);
    }

    p_resourceBarriers.Clear();
  }

  public unsafe void ResolvePendingResourceBarriers()
  {
    if(p_pendingResourceBarriers.Count == 0) 
      return;

    lock(s_globalMutex)
    {
      foreach(var barrier in p_pendingResourceBarriers)
      {
        var resource = barrier.Anonymous.Transition.PResource;
        var stateAfter = barrier.Anonymous.Transition.StateAfter;
        var subresource = barrier.Anonymous.Transition.Subresource;

        var stateBefore = ResourceStates.Common;

        if(s_globalResourceStates.TryGetValue(resource, out var globalState))
          stateBefore = globalState;

        if(stateBefore != stateAfter)
        {
          var resolvedBarrier = barrier;
          resolvedBarrier.Anonymous.Transition.StateBefore = stateBefore;
          p_resourceBarriers.Add(resolvedBarrier);
        }

        p_finalResourceStates[resource] = stateAfter;
      }
    }
    p_pendingResourceBarriers.Clear();
  }

  public void CommitFinalResourceStates()
  {
    lock(s_globalMutex)
    {
      foreach(var kvp in p_finalResourceStates)
      {
        s_globalResourceStates[kvp.Key] = kvp.Value;
      }
    }
  }

  public void Reset()
  {
    p_pendingResourceBarriers.Clear();
    p_resourceBarriers.Clear();
    p_finalResourceStates.Clear();
    p_pendingResourceBarriers.Clear();
  }

  public int GetPengingBarrierCount() => p_pendingResourceBarriers.Count + p_resourceBarriers.Count;

  private unsafe void AddResourceBarrier(ComPtr<ID3D12Resource> _resource, ResourceStates _stateBefore, ResourceStates _stateAfter, uint _subresource)
  {
    if((_stateBefore & _stateAfter) == _stateAfter)
      return;

    var barrier = new ResourceBarrier
    {
      Type = ResourceBarrierType.Transition,
      Flags = ResourceBarrierFlags.None,
    };

    barrier.Anonymous.Transition.PResource = _resource;
    barrier.Anonymous.Transition.StateBefore = _stateBefore;
    barrier.Anonymous.Transition.StateAfter = _stateAfter;
    barrier.Anonymous.Transition.Subresource = _subresource;

    p_resourceBarriers.Add(barrier);
  }

  public static void ResetGlobalState()
  {
    lock(s_globalMutex)
    {
      s_globalResourceStates.Clear();
    }
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    s_globalResourceStates.Clear();
    p_finalResourceStates.Clear();
    p_pendingResourceStates.Clear();
    p_pendingResourceBarriers.Clear();
    p_resourceBarriers.Clear();

    p_disposed = true;  
  }

  internal int GetBarrierFlushCount()
  {
    throw new NotImplementedException();
  }

  internal int GetTransitionCount()
  {
    throw new NotImplementedException();
  }
}
