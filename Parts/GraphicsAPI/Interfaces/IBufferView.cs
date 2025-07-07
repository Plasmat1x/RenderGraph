using Resources.Enums;

namespace GraphicsAPI.Interfaces;

public interface IBufferView : IDisposable
{
  IBuffer Buffer { get; }
  BufferViewType ViewType { get; }
}