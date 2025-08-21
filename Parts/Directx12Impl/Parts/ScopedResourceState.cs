using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts;
public static unsafe partial class DX12ResourceStateHelper
{
  /// <summary>
  /// Scoped resource state manager - автоматически восстанавливает исходное состояние
  /// </summary>
  public struct ScopedResourceState: System.IDisposable
  {
    private readonly ID3D12GraphicsCommandList* p_commandList;
    private readonly DX12Resource p_resource;
    private readonly ResourceStates p_originalState;
    private readonly uint p_subresource;
    private bool p_disposed;

    public ScopedResourceState(ID3D12GraphicsCommandList* _commandList, DX12Resource _resource,
        ResourceStates _targetState, uint _subresource = D3D12.ResourceBarrierAllSubresources)
    {
      p_commandList = _commandList;
      p_resource = _resource;
      p_originalState = _resource.GetCurrentState();
      p_subresource = _subresource;
      p_disposed = false;

      // Transition в целевое состояние
      TransitionResource(_commandList, _resource, _targetState, _subresource);
    }

    public void Dispose()
    {
      if(!p_disposed && p_commandList != null)
      {
        // Возвращаем в исходное состояние
        TransitionResource(p_commandList, p_resource, p_originalState, p_subresource);
        p_disposed = true;
      }
    }
  }
}
