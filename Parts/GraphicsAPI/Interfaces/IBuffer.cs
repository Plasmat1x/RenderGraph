using GraphicsAPI.Enums;

using Resources;
using Resources.Enums;

namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс буфера
/// </summary>
public interface IBuffer: IResource
{
  BufferDescription Description { get; }
  ulong Size { get; }
  uint Stride { get; }
  BufferUsage Usage { get; }

  // Views
  IBufferView CreateView(BufferViewDescription description);
  IBufferView GetDefaultShaderResourceView();
  IBufferView GetDefaultUnorderedAccessView();

  // Маппинг данных
  IntPtr Map(MapMode mode = MapMode.Write);
  void Unmap();
  bool IsMapped { get; }

  // Данные
  void SetData<T>(T[] data, ulong offset = 0) where T : struct;
  void SetData<T>(T data, ulong offset = 0) where T : struct;
  T[] GetData<T>(ulong offset = 0, ulong count = 0) where T : struct;
  T GetData<T>(ulong offset = 0) where T : struct;
}
