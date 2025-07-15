using GraphicsAPI.Interfaces;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;
public class DX12Fence: IFence
{
  private ComPtr<ID3D12Fence> p_fence;
  private nint p_fenceEvent;
  private ulong p_fenceValue;
    
  public DX12Fence()
  {

  }

  public ulong Value  => p_fenceValue;

  public bool IsSignaled => throw new NotImplementedException();

  public void Signal(ulong _value)
  {
    throw new NotImplementedException();
  }

  public void Wait(ulong _value, uint _timeoutMs = uint.MaxValue)
  {
    throw new NotImplementedException();
  }

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }
}
