using Resources.Enums;

namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс view буфера
/// </summary>
public interface IBufferView: IDisposable
{
  IBuffer Buffer { get; }
  BufferViewType ViewType { get; }
  BufferViewDescription Description { get; }
  IntPtr GetNativeHandle();
}
