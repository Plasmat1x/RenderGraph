using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl.Parts;
/// <summary>
/// Helper класс для правильного управления состояниями ресурсов DX12
/// </summary>
public static unsafe partial class DX12ResourceStateHelper
{
  /// <summary>
  /// Выполнить transition ресурса в новое состояние, если это необходимо
  /// </summary>
  public static void TransitionResource(ID3D12GraphicsCommandList* _commandList,
      DX12Resource _resource, ResourceStates _targetState, uint _subresource = D3D12.ResourceBarrierAllSubresources)
  {
    var currentState = _resource.GetCurrentState();

    if(currentState == _targetState)
      return; // Transition не нужен

    var barrier = new ResourceBarrier
    {
      Type = ResourceBarrierType.Transition,
      Transition = new ResourceTransitionBarrier
      {
        PResource = _resource.GetResource(),
        StateBefore = currentState,
        StateAfter = _targetState,
        Subresource = _subresource
      }
    };

    _commandList->ResourceBarrier(1, &barrier);
    _resource.SetCurrentState(_targetState);
  }

  /// <summary>
  /// Выполнить множественные transitions за один вызов
  /// </summary>
  public static void TransitionResources(ID3D12GraphicsCommandList* _commandList,
      params (DX12Resource resource, ResourceStates targetState, uint subresource)[] _transitions)
  {
    var barriers = new List<ResourceBarrier>();

    foreach(var (resource, targetState, subresource) in _transitions)
    {
      var currentState = resource.GetCurrentState();

      if(currentState != targetState)
      {
        barriers.Add(new ResourceBarrier
        {
          Type = ResourceBarrierType.Transition,
          Transition = new ResourceTransitionBarrier
          {
            PResource = resource.GetResource(),
            StateBefore = currentState,
            StateAfter = targetState,
            Subresource = subresource
          }
        });
      }
    }

    if(barriers.Count == 0)
      return;

    // Выполняем все barriers
    var barriersArray = stackalloc ResourceBarrier[barriers.Count];
    for(int i = 0; i < barriers.Count; i++)
    {
      barriersArray[i] = barriers[i];
    }

    _commandList->ResourceBarrier((uint)barriers.Count, barriersArray);

    // Обновляем состояния ресурсов
    foreach(var (resource, targetState, _) in _transitions)
    {
      resource.SetCurrentState(targetState);
    }
  }

  /// <summary>
  /// Создать scoped state manager для автоматического восстановления состояния
  /// </summary>
  public static ScopedResourceState CreateScopedTransition(ID3D12GraphicsCommandList* _commandList,
      DX12Resource _resource, ResourceStates _targetState, uint _subresource = D3D12.ResourceBarrierAllSubresources)
  {
    return new ScopedResourceState(_commandList, _resource, _targetState, _subresource);
  }
}
