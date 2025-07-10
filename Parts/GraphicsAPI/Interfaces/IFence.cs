namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс fence для синхронизации
/// </summary>
public interface IFence: IDisposable
{
  ulong Value { get; }
  bool IsSignaled { get; }
  void Signal(ulong value);
  void Wait(ulong value, uint timeoutMs = uint.MaxValue);
  IntPtr GetNativeHandle();
}