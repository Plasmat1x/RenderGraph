namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс fence для синхронизации
/// </summary>
public interface IFence: IDisposable
{
  ulong Value { get; }
  bool IsSignaled { get; }
  void Signal(ulong _value);
  void Wait(ulong _value, uint _timeoutMs = uint.MaxValue);
  IntPtr GetNativeHandle();
}