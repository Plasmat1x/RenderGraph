using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using Resources;
using Resources.Enums;

using Silk.NET.Vulkan;

namespace VulkanImpl;

public unsafe class VKBuffer : IBuffer
{
  private readonly BufferDescription p_description;
  private bool p_disposed;

  public VKBuffer(BufferDescription _description)
  {
    p_description = _description;
  }

  public BufferDescription Description => p_description;
  public string Name => p_description.Name;
  public ResourceType ResourceType => ResourceType.Buffer;
  public bool IsDisposed => p_disposed;
  public ulong Size => p_description.Size;
  public uint Stride => p_description.Stride;
  public BufferUsage Usage => p_description.BufferUsage;
  public bool IsMapped => false;

  public IntPtr Map(MapMode _mode = MapMode.Write)
  {
    throw new NotImplementedException("VKBuffer.Map not implemented");
  }

  public void Unmap()
  {
    throw new NotImplementedException("VKBuffer.Unmap not implemented");
  }

  public void SetData<T>(T[] _data, ulong _offset = 0) where T : unmanaged
  {
    throw new NotImplementedException("VKBuffer.SetData not implemented");
  }

  public void SetData<T>(T _data, ulong _offset = 0) where T : unmanaged
  {
    throw new NotImplementedException("VKBuffer.SetData not implemented");
  }

  public T[] GetData<T>(ulong _offset = 0, int _count = 0) where T : unmanaged
  {
    throw new NotImplementedException("VKBuffer.GetData not implemented");
  }

  public T GetData<T>(ulong _offset = 0) where T : unmanaged
  {
    throw new NotImplementedException("VKBuffer.GetData not implemented");
  }

  public IBufferView CreateView(BufferViewDescription _description)
  {
    throw new NotImplementedException("VKBuffer.CreateView not implemented");
  }

  public IBufferView GetDefaultShaderResourceView()
  {
    throw new NotImplementedException("VKBuffer.GetDefaultShaderResourceView not implemented");
  }

  public IBufferView GetDefaultUnorderedAccessView()
  {
    throw new NotImplementedException("VKBuffer.GetDefaultUnorderedAccessView not implemented");
  }

  public IntPtr GetNativeHandle() => IntPtr.Zero;

  public ulong GetMemorySize() => Size;

  public void Dispose()
  {
    if(p_disposed)
      return;
    p_disposed = true;
  }
}
