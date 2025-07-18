using GraphicsAPI.Interfaces;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Directx12Impl;
public class DX12Fence: IFence
{
  private readonly ComPtr<ID3D12Fence> p_fence;
  private readonly AutoResetEvent p_fenceEvent;
  private ulong p_fenceValue;
  private bool p_disposed;
    
  public DX12Fence(ComPtr<ID3D12Device>? _device, ulong _initialValue = 0)
  {
    if(_device == null)
      throw new ArgumentNullException(nameof(_device));

    HResult hr = _device.Value.CreateFence(_initialValue,
      FenceFlags.None,
      out p_fence);

    if(hr.IsFailure)
      throw new COMException("Failed to create D3D12 fence", hr);

    p_fenceValue = _initialValue;
    p_fenceEvent = new AutoResetEvent(false);
  }

  public ulong Value  => p_fenceValue;

  public bool IsSignaled
  {
    get
    {
      ThrowIfDisposed();
      return p_fence.GetCompletedValue() >= p_fenceValue;
    }
  }

  public void Signal(ulong _value)
  {
    ThrowIfDisposed();

    if(_value <= p_fenceValue)
      throw new ArgumentException($"Signal value ({_value}) must be greater than current value ({p_fenceValue})");

    p_fenceValue = _value;

    HResult hr = p_fence.Signal(_value);
    if(hr.IsFailure)
      throw new COMException($"Failed to signal fence with value ({_value})", hr);

  }

  public void SignalFromQueue(ComPtr<ID3D12CommandQueue>? _queue, ulong _value)
  {
    ThrowIfDisposed();

    if(_queue == null)
      throw new ArgumentNullException(nameof(_queue));

    if(_value <= p_fenceValue)
      throw new ArgumentException($"Signal value ({_value}) must be greater than current value ({p_fenceValue})");

    p_fenceValue = _value;

    HResult hr = _queue.Value.Signal(p_fence, _value);
    if(hr.IsFailure)
      throw new COMException($"Failed to signal fence from queue with value ({_value})", hr);

  }

  public unsafe void Wait(ulong _value, uint _timeoutMs = uint.MaxValue)
  {
    ThrowIfDisposed();

    if(p_fence.GetCompletedValue() >= _value)
      return;

    HResult hr = p_fence.SetEventOnCompletion(_value,
      (void*)p_fenceEvent.SafeWaitHandle.DangerousGetHandle());

    if(hr.IsFailure)
      throw new COMException($"Failed to set event on fence completion for value {_value}", hr);

    bool signaled = p_fenceEvent.WaitOne((int)_timeoutMs);

    if(!signaled)
      throw new TimeoutException($"Fence wait timed out after {_timeoutMs}ms waiting for value {_value}");
  }

  public ulong GetCompletedValue()
  {
    ThrowIfDisposed();
    return p_fence.GetCompletedValue();
  }

  public IntPtr GetNativeHandle()
  {
    ThrowIfDisposed();
    return Marshal.GetIUnknownForObject(p_fence);
  }

  public void WaitForQueue(ComPtr<ID3D12CommandQueue>? _queue, ulong _value)
  {
    ThrowIfDisposed();

    if(_queue == null)
      throw new ArgumentNullException(nameof(_queue));

    HResult hr = _queue.Value.Wait(p_fence, _value);
    if(hr.IsFailure)
      throw new COMException($"Failed to make queue wait for fence value {_value}", hr);
  }

  public ComPtr<ID3D12Fence> GetFence()
  {
    ThrowIfDisposed(); 
    return p_fence;
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    p_fenceEvent?.Dispose();
    p_fence.Dispose();

    p_disposed = true;
    GC.SuppressFinalize(this);
  }

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(DX12Fence));
  }
}
