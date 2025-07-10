using Resources.Enums;

namespace GraphicsAPI;

/// <summary>
/// Базовый интерфейс для всех GPU ресурсов
/// </summary>
public interface IResource: IDisposable
{
  string Name { get; set; }
  ResourceType ResourceType { get; }
  bool IsDisposed { get; }
  ulong GetMemorySize();
  IntPtr GetNativeHandle();
}