using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using Resources;
using Resources.Enums;

namespace VulkanImpl;

public class VKBuffer : IBuffer
{
  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public string Name { get; }
  public ResourceType ResourceType { get; }
  public bool IsDisposed { get; }

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public IntPtr GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public BufferDescription Description { get; }
  public ulong Size { get; }
  public uint Stride { get; }
  public BufferUsage Usage { get; }
  public bool IsMapped { get; }

  public IBufferView CreateView(BufferViewDescription _description)
  {
    throw new NotImplementedException();
  }

  public IBufferView GetDefaultShaderResourceView()
  {
    throw new NotImplementedException();
  }

  public IBufferView GetDefaultUnorderedAccessView()
  {
    throw new NotImplementedException();
  }

  public IntPtr Map(MapMode _mode = MapMode.Write)
  {
    throw new NotImplementedException();
  }

  public void Unmap()
  {
    throw new NotImplementedException();
  }

  public void SetData<T>(T[] _data, ulong _offset = 0) where T : unmanaged
  {
    throw new NotImplementedException();
  }

  public void SetData<T>(T _data, ulong _offset = 0) where T : unmanaged
  {
    throw new NotImplementedException();
  }

  public T[] GetData<T>(ulong _offset = 0, int _count = 0) where T : unmanaged
  {
    throw new NotImplementedException();
  }

  public T GetData<T>(ulong _offset = 0) where T : unmanaged
  {
    throw new NotImplementedException();
  }
}
