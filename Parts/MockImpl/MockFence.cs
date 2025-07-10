using GraphicsAPI.Interfaces;

namespace MockImpl;

public class MockFence: IFence
{
  public MockFence(ulong _initialValue)
  {
    Value = _initialValue;
  }

  public ulong Value { get; private set; }
  public bool IsSignaled => Value > 0;

  public void Signal(ulong _value)
  {
    Value = _value;
    Console.WriteLine($"    [Sync] Fence signaled with value {_value}");
  }

  public void Wait(ulong _value, uint _timeoutMs = uint.MaxValue)
  {
    Console.WriteLine($"    [Sync] Waiting for fence value {_value}");
    Thread.Sleep(Math.Min(10, (int)_timeoutMs));
    Value = Math.Max(Value, _value);
  }

  public IntPtr GetNativeHandle() => new IntPtr(99999);

  public void Dispose() => Console.WriteLine($"    [Sync] Disposed fence");
}