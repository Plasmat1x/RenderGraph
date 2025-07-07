using Resources.Enums;
using Resources.Interfaces;

namespace Resources;

public class BufferDescription: IResourceDescription
{
  public ulong Size { get; set; }
  public uint Stride { get; set; }
  public uint ElementCount { get; set; }
  public uint StructureByteStride { get; set; }
  public string Name { get; set; }

  public BufferUsage Usage { get; set; }

  public BindFlags BindFlags { get; set; }

  public CPUAccessFlags CPUAcessFalgs { get; set; }

  public ResourceMiscFlags ResourceMiscFlags { get; set; }

  ResourceUsage IResourceDescription.Usage { get;}

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public bool IsComaptible(BufferDescription _other)
  {
    throw new NotImplementedException();
  }

  public bool IsComaptible(IResourceDescription _other)
  {
    throw new NotImplementedException();
  }

  public bool IsStructured()
  {
    throw new NotImplementedException();
  }
}
