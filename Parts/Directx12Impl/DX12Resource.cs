using GraphicsAPI;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;

/// <summary>
/// Базовый класс для всех DX12 ресурсов
/// </summary>
public abstract unsafe class DX12Resource: IResource
{
  protected ID3D12Device* p_device;
  protected ID3D12Resource* p_resource;
  protected ResourceStates p_currentState;
  protected string p_name;
  protected bool p_disposed;

  protected DX12Resource(ComPtr<ID3D12Device>? _device, string _name)
  {
    p_device = _device ?? throw new ArgumentNullException(nameof(_device));
    p_name = _name ?? string.Empty;
    p_currentState = ResourceStates.Common;
  }

  // === IResource implementation ===
  public string Name => p_name;
  public abstract ResourceType ResourceType { get; }
  public bool IsDisposed => p_disposed;

  public abstract ulong GetMemorySize();

  public IntPtr GetNativeHandle()
  {
    return new IntPtr(p_resource);
  }

  // === DX12-specific methods ===

  /// <summary>
  /// Получить нативный DX12 ресурс
  /// </summary>
  public ID3D12Resource* GetResource() => p_resource;

  /// <summary>
  /// Получить текущее состояние ресурса
  /// </summary>
  public ResourceStates GetCurrentState() => p_currentState;

  /// <summary>
  /// Установить текущее состояние ресурса (используется state tracker'ом)
  /// </summary>
  public void SetCurrentState(ResourceStates _state) => p_currentState = _state;

  /// <summary>
  /// Получить описание DX12 ресурса
  /// </summary>
  public ResourceDesc GetD3D12Description()
  {
    return p_resource->GetDesc();
  }

  /// <summary>
  /// Проверить, поддерживает ли ресурс указанное состояние
  /// </summary>
  public virtual bool SupportsState(ResourceStates _state)
  {
    // Базовая реализация - переопределяется в наследниках
    return true;
  }

  /// <summary>
  /// Получить GPU виртуальный адрес (для буферов)
  /// </summary>
  public virtual ulong GetGPUVirtualAddress()
  {
    return p_resource->GetGPUVirtualAddress();
  }

  public virtual void Dispose()
  {
    if(!p_disposed)
    {
      if(p_resource != null)
      {
        p_resource->Release();
        p_resource = null;
      }
      p_disposed = true;
    }
  }
}
