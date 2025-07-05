using Resources.Enums;
using Resources.Interfaces;

namespace Resources;

public struct BufferDescription: IResourceDescription
{
  public ulong Size { get; set; }
  public uint Stride { get; set; }
  public uint ElementCount { get; set; }
  public uint StructureByteStride { get; set; }

  public ResourceUsage Usage => throw new NotImplementedException();

  public BindFlags BindFlags => throw new NotImplementedException();

  public string Name => throw new NotImplementedException();

  public CPUAccessFlags CPUAcessFalgs => throw new NotImplementedException();

  public ResourceMiscFlags ResourceMiscFlags => throw new NotImplementedException();

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
