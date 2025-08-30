using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace MockImpl;

public class MockBuffer: IBuffer
{
  private IntPtr p_mappedPointer = IntPtr.Zero;
  private readonly Dictionary<BufferViewType, IBufferView> p_defaultViews = new();

  public MockBuffer(uint _id, BufferDescription _description)
  {
    Id = _id;
    Description = _description;
    Name = _description.Name;
  }

  public uint Id { get; }
  public BufferDescription Description { get; }
  public string Name { get; set; }
  public ResourceType ResourceType => ResourceType.Buffer;
  public bool IsDisposed { get; private set; }

  public ulong Size => Description.Size;
  public uint Stride => Description.Stride;
  public BufferUsage Usage => Description.BufferUsage;
  public bool IsMapped { get; private set; }


  public IBufferView CreateView(BufferViewDescription _description)
  {
    Console.WriteLine($"    [Resource] Creating buffer view for {Name} ({_description.ViewType})");
    return new MockBufferView(this, _description);
  }

  public IBufferView GetDefaultShaderResourceView()
  {
    if(!p_defaultViews.ContainsKey(BufferViewType.ShaderResource))
    {
      var desc = new BufferViewDescription
      {
        ViewType = BufferViewType.ShaderResource,
        NumElements = Size,
        StructureByteStride = Stride
      };
      p_defaultViews[BufferViewType.ShaderResource] = CreateView(desc);
    }
    return p_defaultViews[BufferViewType.ShaderResource];
  }

  public IBufferView GetDefaultUnorderedAccessView()
  {
    if(!p_defaultViews.ContainsKey(BufferViewType.UnorderedAccess))
    {
      var desc = new BufferViewDescription
      {
        ViewType = BufferViewType.UnorderedAccess,
        NumElements = Size,
        StructureByteStride = Stride
      };
      p_defaultViews[BufferViewType.UnorderedAccess] = CreateView(desc);
    }
    return p_defaultViews[BufferViewType.UnorderedAccess];
  }

  public IntPtr Map(MapMode _mode = MapMode.Write)
  {
    Console.WriteLine($"    [Resource] Mapping buffer {Name} ({_mode})");
    p_mappedPointer = new IntPtr(0x1000 + Id * 0x100);
    IsMapped = true;
    return p_mappedPointer;
  }

  public void Unmap()
  {
    Console.WriteLine($"    [Resource] Unmapping buffer {Name}");
    p_mappedPointer = IntPtr.Zero;
    IsMapped = false;
  }

  public void SetData<T>(T[] _data, ulong _offset = 0) where T : unmanaged => Console.WriteLine($"    [Resource] Setting data for buffer {Name} ({_data.Length} elements, offset: {_offset})");

  public void SetData<T>(T _data, ulong _offset = 0) where T : unmanaged => Console.WriteLine($"    [Resource] Setting single data element for buffer {Name} (offset: {_offset})");

  public T[] GetData<T>(ulong _offset = 0, int _count = -1) where T : unmanaged
  {
    var elementCount = _count > 0 ? (int)_count : (int)(Size / (ulong)System.Runtime.InteropServices.Marshal.SizeOf<T>());
    Console.WriteLine($"    [Resource] Getting data from buffer {Name} (offset: {_offset}, count: {elementCount})");
    return new T[elementCount];
  }

  public T GetData<T>(ulong _offset = 0) where T : unmanaged
  {
    Console.WriteLine($"    [Resource] Getting single data element from buffer {Name} (offset: {_offset})");
    return default(T);
  }

  public IntPtr GetNativeHandle() => new IntPtr(Id + 1000);

  public ulong GetMemorySize() => Size;

  public void Dispose()
  {
    if(IsDisposed)
      return;

    Console.WriteLine($"    [Resource] Disposed buffer {Name} (ID: {Id})");
    foreach(var view in p_defaultViews.Values)
    {
      view?.Dispose();
    }
    p_defaultViews.Clear();
    IsDisposed = true;

  }
}
