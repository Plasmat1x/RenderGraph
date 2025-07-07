using GraphicsAPI.Interfaces;

namespace MockImpl;

public class MockFence: IFence
{
  public ulong Value { get; private set; }
  public bool IsSignaled => Value > 0;

  public MockFence(ulong initialValue)
  {
    Value = initialValue;
  }

  public void Signal(ulong value)
  {
    Value = value;
    Console.WriteLine($"    [Sync] Fence signaled with value {value}");
  }

  public void Wait(ulong value, uint timeoutMs = uint.MaxValue)
  {
    Console.WriteLine($"    [Sync] Waiting for fence value {value}");
    Thread.Sleep(Math.Min(10, (int)timeoutMs));
    Value = Math.Max(Value, value);
  }

  public IntPtr GetNativeHandle() => new IntPtr(99999);

  public void Dispose()
  {
    Console.WriteLine($"    [Sync] Disposed fence");
  }
}