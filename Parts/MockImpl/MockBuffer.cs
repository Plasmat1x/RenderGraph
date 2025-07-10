using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace MockImpl;

public class MockBuffer: IBuffer
{
  public uint Id { get; }
  public BufferDescription Description { get; }
  public string Name { get; set; }
  public ResourceType ResourceType => ResourceType.Buffer;
  public bool IsDisposed { get; private set; }

  public ulong Size => Description.Size;
  public uint Stride => Description.Stride;
  public BufferUsage Usage => Description.BufferUsage;
  public bool IsMapped { get; private set; }

  private IntPtr _mappedPointer = IntPtr.Zero;
  private readonly Dictionary<BufferViewType, IBufferView> _defaultViews = new();

  public MockBuffer(uint id, BufferDescription description)
  {
    Id = id;
    Description = description;
    Name = description.Name;
  }

  public IBufferView CreateView(BufferViewDescription description)
  {
    Console.WriteLine($"    [Resource] Creating buffer view for {Name} ({description.ViewType})");
    return new MockBufferView(this, description);
  }

  public IBufferView GetDefaultShaderResourceView()
  {
    if(!_defaultViews.ContainsKey(BufferViewType.ShaderResource))
    {
      var desc = new BufferViewDescription
      {
        ViewType = BufferViewType.ShaderResource,
        Size = Size,
        Stride = Stride
      };
      _defaultViews[BufferViewType.ShaderResource] = CreateView(desc);
    }
    return _defaultViews[BufferViewType.ShaderResource];
  }

  public IBufferView GetDefaultUnorderedAccessView()
  {
    if(!_defaultViews.ContainsKey(BufferViewType.UnorderedAccess))
    {
      var desc = new BufferViewDescription
      {
        ViewType = BufferViewType.UnorderedAccess,
        Size = Size,
        Stride = Stride
      };
      _defaultViews[BufferViewType.UnorderedAccess] = CreateView(desc);
    }
    return _defaultViews[BufferViewType.UnorderedAccess];
  }

  public IntPtr Map(MapMode mode = MapMode.Write)
  {
    Console.WriteLine($"    [Resource] Mapping buffer {Name} ({mode})");
    _mappedPointer = new IntPtr(0x1000 + Id * 0x100); // Fake pointer
    IsMapped = true;
    return _mappedPointer;
  }

  public void Unmap()
  {
    Console.WriteLine($"    [Resource] Unmapping buffer {Name}");
    _mappedPointer = IntPtr.Zero;
    IsMapped = false;
  }

  public void SetData<T>(T[] data, ulong offset = 0) where T : struct
  {
    Console.WriteLine($"    [Resource] Setting data for buffer {Name} ({data.Length} elements, offset: {offset})");
  }

  public void SetData<T>(T data, ulong offset = 0) where T : struct
  {
    Console.WriteLine($"    [Resource] Setting single data element for buffer {Name} (offset: {offset})");
  }

  public T[] GetData<T>(ulong offset = 0, ulong count = 0) where T : struct
  {
    var elementCount = count > 0 ? (int)count : (int)(Size / (ulong)System.Runtime.InteropServices.Marshal.SizeOf<T>());
    Console.WriteLine($"    [Resource] Getting data from buffer {Name} (offset: {offset}, count: {elementCount})");
    return new T[elementCount];
  }

  public T GetData<T>(ulong offset = 0) where T : struct
  {
    Console.WriteLine($"    [Resource] Getting single data element from buffer {Name} (offset: {offset})");
    return default(T);
  }

  public IntPtr GetNativeHandle()
  {
    return new IntPtr(Id + 1000);
  }

  public ulong GetMemorySize()
  {
    return Size;
  }

  public void Dispose()
  {
    if(!IsDisposed)
    {
      Console.WriteLine($"    [Resource] Disposed buffer {Name} (ID: {Id})");
      foreach(var view in _defaultViews.Values)
      {
        view?.Dispose();
      }
      _defaultViews.Clear();
      IsDisposed = true;
    }
  }
}
