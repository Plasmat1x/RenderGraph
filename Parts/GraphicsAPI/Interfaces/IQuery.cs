using GraphicsAPI.Enums;

namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс query
/// </summary>
public interface IQuery: IDisposable
{
  QueryType Type { get; }
  bool GetData<T>(out T _data) where T : struct;
  IntPtr GetNativeHandle();
}