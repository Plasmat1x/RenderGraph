using GraphicsAPI.Interfaces;

namespace VulkanImpl;

public class VKFence : IFence
{
  private ulong p_value;
  private bool p_signaled;
  private bool p_disposed;

  public VKFence(ulong _initialValue)
  {
    p_value = _initialValue;
  }

  public ulong Value => p_value;
  public bool IsSignaled => p_signaled;

  public void Signal(ulong _value)
  {
    Console.WriteLine($"  [Vulkan] Fence signal {_value}");
    p_value = _value;
    p_signaled = true;
  }

  public void Wait(ulong _value, uint _timeoutMs = uint.MaxValue)
  {
    Console.WriteLine($"  [Vulkan] Fence wait {_value}");
    p_signaled = false;
  }

  public void Reset()
  {
    Console.WriteLine($"  [Vulkan] Fence reset");
    p_signaled = false;
  }

  public IntPtr GetNativeHandle() => IntPtr.Zero;
  public ulong GetMemorySize() => 8;

  public void Dispose()
  {
    if(p_disposed)
      return;
    Console.WriteLine($"  [Vulkan] Dispose fence");
    p_disposed = true;
  }
}
