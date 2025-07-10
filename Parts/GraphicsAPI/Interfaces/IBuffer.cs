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
  bool IsMapped { get; }

  IBufferView CreateView(BufferViewDescription _description);
  IBufferView GetDefaultShaderResourceView();
  IBufferView GetDefaultUnorderedAccessView();
  IntPtr Map(MapMode _mode = MapMode.Write);
  void Unmap();
  void SetData<T>(T[] _data, ulong _offset = 0) where T : struct;
  void SetData<T>(T _data, ulong _offset = 0) where T : struct;
  T[] GetData<T>(ulong _offset = 0, ulong _count = 0) where T : struct;
  T GetData<T>(ulong _offset = 0) where T : struct;
}
