using GraphicsAPI.Interfaces;

namespace VulkanImpl;

public class VKFence : IFence
{
  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public ulong Value { get; }
  public bool IsSignaled { get; }

  public void Signal(ulong _value)
  {
    throw new NotImplementedException();
  }

  public void Wait(ulong _value, uint _timeoutMs = UInt32.MaxValue)
  {
    throw new NotImplementedException();
  }

  public IntPtr GetNativeHandle()
  {
    throw new NotImplementedException();
  }
}
