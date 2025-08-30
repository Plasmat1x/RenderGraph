using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts.Managers;
public class DX12FrameFenceManager: IDisposable
{
  private readonly DX12Fence p_fence;
  private readonly ulong[] p_fenceValues;
  private readonly int p_frameCount;
  private ulong p_currentFenceValue;
  private int p_currentFrameIndex;

  public DX12FrameFenceManager(ComPtr<ID3D12Device> _device, int _frameCount = 3)
  {
    if(_frameCount < 1)
      throw new ArgumentException("Frame count must be at least 1", nameof(_frameCount));

    p_frameCount = _frameCount;
    p_fenceValues = new ulong[_frameCount];
    p_fence = new DX12Fence(_device, 0);
    p_currentFenceValue = 1;
  }

  public int CurrentFrameIndex => p_currentFrameIndex;
  public ulong CurrentFenceValue => p_currentFenceValue;

  public void WaitForPreviousFrame()
  {
    var fenceValueToWait = p_fenceValues[p_currentFrameIndex];
    if(fenceValueToWait != 0)
      p_fence.Wait(fenceValueToWait);
  }

  public void SignalEndOfFrame(ComPtr<ID3D12CommandQueue> _queue)
  {
    p_fence.SignalFromQueue(_queue, p_currentFenceValue);
    p_fenceValues[p_currentFrameIndex] = p_currentFenceValue;
    p_currentFenceValue++;
  }

  public void MoveToNextFrame()
  {
    p_currentFrameIndex = (p_currentFrameIndex + 1) % p_frameCount;
  }

  public void WaitForGPU(ComPtr<ID3D12CommandQueue> _queue)
  {
    p_fence.SignalFromQueue(_queue, p_currentFenceValue);
    p_fence.Wait(p_currentFenceValue);
    p_currentFenceValue++;
  }

  public void Dispose()
  {
    p_fence?.Dispose();
  }
}
