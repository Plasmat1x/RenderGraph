using Resources.Enums;

namespace Resources;

public class _BufferDescription: ResourceDescription
{
  public ulong Size { get; set; } = 0;
  public uint Stride { get; set; } = 0;
  public BufferUsage BufferUsage { get; set; } = BufferUsage.Vertex;
  public uint ElementCount { get; set; } = 0;
  public uint StructureByteStride { get; set; } = 0;

  public override ulong GetMemorySize() => Size;

  public override bool IsCompatible(ResourceDescription _other)
  {
    if(_other is not _BufferDescription otherBuffer)
      return false;

    return Size == otherBuffer.Size &&
           Stride == otherBuffer.Stride &&
           StructureByteStride == otherBuffer.StructureByteStride;
  }

  public bool IsStructured()
  {
    return BufferUsage == BufferUsage.Structured &&
           StructureByteStride > 0;
  }

  public bool IsConstant()
  {
    return BufferUsage == BufferUsage.Constant ||
           (BindFlags & BindFlags.ConstantBuffer) != 0;
  }

  public bool IsVertex()
  {
    return BufferUsage == BufferUsage.Vertex ||
           (BindFlags & BindFlags.VertexBuffer) != 0;
  }

  public bool IsIndex()
  {
    return BufferUsage == BufferUsage.Index ||
           (BindFlags & BindFlags.IndexBuffer) != 0;
  }

  public bool IsUnorderedAccess()
  {
    return BufferUsage == BufferUsage.Raw ||
           BufferUsage == BufferUsage.Structured ||
           (BindFlags & BindFlags.UnorderedAccess) != 0;
  }

  public bool IsShaderResource()
  {
    return (BindFlags & BindFlags.ShaderResource) != 0;
  }

  public bool IsIndirectArgs()
  {
    return BufferUsage == BufferUsage.IndirectArgs;
  }

  public bool IsAppendConsume()
  {
    return BufferUsage == BufferUsage.Append ||
           BufferUsage == BufferUsage.Consume;
  }

  public uint GetElementCount()
  {
    if(ElementCount > 0)
      return ElementCount;

    if(IsStructured() && StructureByteStride > 0)
      return (uint)(Size / StructureByteStride);

    if(Stride > 0)
      return (uint)(Size / Stride);

    return (uint)Size;
  }

  public void SetElementCount(uint _count)
  {
    ElementCount = _count;

    if(IsStructured() && StructureByteStride > 0)
      Size = (ulong)(_count * StructureByteStride);
    else if(Stride > 0)
      Size = (ulong)(_count * Stride);
  }

  public override string ToString() => $"BufferDescription(Name: '{Name}', Size: {Size}, Usage: {BufferUsage}, Stride: {Stride})";

  public override ResourceDescription Clone()
  {
    return new _BufferDescription
    {
      Name = Name,
      Size = Size,
      Stride = Stride,
      BufferUsage = BufferUsage,
      ElementCount = ElementCount,
      StructureByteStride = StructureByteStride,
      Usage = Usage,
      BindFlags = BindFlags,
      CPUAccessFlags = CPUAccessFlags,
      MiscFlags = MiscFlags
    };
  }

  public override bool Validate(out string _errorMessage)
  {

    if(!base.Validate(out _errorMessage))
      return false;

    if(Size == 0)
    {
      _errorMessage = "Buffer size must be greater than 0";
      return false;
    }

    if(IsConstant())
    {
      if(Size % 16 != 0)
      {
        _errorMessage = "Constant buffer size must be multiple of 16 bytes";
        return false;
      }

      if(CPUAccessFlags == CPUAccessFlags.None && Usage == ResourceUsage.Default)
      {
        _errorMessage = "Constant buffer should be Dynamic with Write CPU access for frequent updates";
        return false;
      }
    }

    if(IsStructured())
    {
      if(StructureByteStride == 0)
      {
        _errorMessage = "Structured buffer must have non-zero structure stride";
        return false;
      }

      if(Size % StructureByteStride != 0)
      {
        _errorMessage = "Structured buffer size must be multiple of structure stride";
        return false;
      }
    }

    if(IsVertex() && Stride == 0)
    {
      _errorMessage = "Vertex buffer should have non-zero stride";
      return false;
    }

    if(IsIndex() && Stride != 2 && Stride != 4)
    {
      _errorMessage = "Index buffer stride must be 2 or 4 bytes (16-bit or 32-bit indices)";
      return false;
    }

    return true;
  }
}
