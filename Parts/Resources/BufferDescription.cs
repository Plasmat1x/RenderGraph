using Resources.Enums;

namespace Resources;

public class BufferDescription: ResourceDescription
{
  public ulong Size { get; set; }
  public uint Stride { get; set; }
  public uint ElementCount { get; set; }
  public uint StructureByteStride { get; set; }
  public BufferUsage Usage { get; set; }

  public override ResourceUsage Usage => throw new NotImplementedException();

  public override BindFlags BindFlags => throw new NotImplementedException();

  public override string Name => throw new NotImplementedException();

  public override CPUAccessFlags CPUAcessFalgs => throw new NotImplementedException();

  public override ResourceMiscFlags ResourceMiscFlags => throw new NotImplementedException();

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public override ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public bool IsComaptible(BufferDescription _other)
  {
    throw new NotImplementedException();
  }

  public override bool IsComaptible(ResourceDescription _other)
  {
    throw new NotImplementedException();
  }

  public bool IsStructured()
  {
    throw new NotImplementedException();
  }
}

public enum ResourceUsage
{

}