using Resources.Enums;

namespace GraphicsAPI;

/// <summary>
/// Базовый интерфейс для всех GPU ресурсов
/// </summary>
public interface IResource: IDisposable
{
  IntPtr GetNativeHandle();
  string Name { get; set; }
  ResourceType ResourceType { get; }
  ulong GetMemorySize();
  bool IsDisposed { get; }
}