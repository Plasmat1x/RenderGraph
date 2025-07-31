using Resources.Enums;

namespace Resources;

/// <summary>
/// ??????????? ???????? ?????? ? ?????????? DX12
/// </summary>
public class BufferDescription: ResourceDescription
{
  public ulong Size { get; set; }
  public uint Stride { get; set; } = 0;
  public BufferUsage BufferUsage { get; set; } = BufferUsage.Vertex;
  public ulong ElementCount { get; set; } = 0;
  public uint StructureByteStride { get; set; } = 0;

  /// <summary>
  /// ????????????? ????????? ElementCount ?? ?????? Size ? Stride
  /// </summary>
  public void UpdateElementCount()
  {
    if(Stride > 0)
    {
      ElementCount = Size / Stride;
    }
    else if(StructureByteStride > 0)
    {
      ElementCount = Size / StructureByteStride;
    }
  }

  /// <summary>
  /// ????????? ???????? ?? ????? structured buffer
  /// </summary>
  public bool IsStructured()
  {
    return BufferUsage == BufferUsage.Structured ||
           (StructureByteStride > 0 && (BindFlags & (BindFlags.ShaderResource | BindFlags.UnorderedAccess)) != 0);
  }

  /// <summary>
  /// ????????? ???????? ?? ????? constant buffer
  /// </summary>
  public bool IsConstant()
  {
    return BufferUsage == BufferUsage.Constant || (BindFlags & BindFlags.ConstantBuffer) != 0;
  }

  /// <summary>
  /// ????????? ???????? ?? ????? vertex buffer
  /// </summary>
  public bool IsVertex()
  {
    return BufferUsage == BufferUsage.Vertex || (BindFlags & BindFlags.VertexBuffer) != 0;
  }

  /// <summary>
  /// ????????? ???????? ?? ????? index buffer
  /// </summary>
  public bool IsIndex()
  {
    return BufferUsage == BufferUsage.Index || (BindFlags & BindFlags.IndexBuffer) != 0;
  }

  /// <summary>
  /// ??????? ???????? ??? Vertex Buffer
  /// </summary>
  public static BufferDescription CreateVertexBuffer(ulong _size, uint _stride, string _name = "")
  {
    return new BufferDescription
    {
      Name = _name,
      Size = _size,
      Stride = _stride,
      BufferUsage = BufferUsage.Vertex,
      BindFlags = BindFlags.VertexBuffer,
      Usage = ResourceUsage.Default
    };
  }

  /// <summary>
  /// ??????? ???????? ??? Index Buffer
  /// </summary>
  public static BufferDescription CreateIndexBuffer(ulong _size, string _name = "")
  {
    return new BufferDescription
    {
      Name = _name,
      Size = _size,
      Stride = 0,
      BufferUsage = BufferUsage.Index,
      BindFlags = BindFlags.IndexBuffer,
      Usage = ResourceUsage.Default
    };
  }

  /// <summary>
  /// ??????? ???????? ??? Constant Buffer
  /// </summary>
  public static BufferDescription CreateConstantBuffer(ulong _size, string _name = "")
  {
    // ??????????? ?????? ?? 256 ???? (?????????? DX12)
    var alignedSize = (_size + 255) & ~255UL;

    return new BufferDescription
    {
      Name = _name,
      Size = alignedSize,
      Stride = 0,
      BufferUsage = BufferUsage.Constant,
      BindFlags = BindFlags.ConstantBuffer,
      Usage = ResourceUsage.Dynamic,
      CPUAccessFlags = CPUAccessFlags.Write
    };
  }

  /// <summary>
  /// ??????? ???????? ??? Structured Buffer
  /// </summary>
  public static BufferDescription CreateStructuredBuffer(ulong _elementCount, uint _elementSize, string _name = "", bool _allowUAV = false)
  {
    var bindFlags = BindFlags.ShaderResource;
    if(_allowUAV)
      bindFlags |= BindFlags.UnorderedAccess;

    return new BufferDescription
    {
      Name = _name,
      Size = _elementCount * _elementSize,
      Stride = _elementSize,
      StructureByteStride = _elementSize,
      BufferUsage = BufferUsage.Structured,
      BindFlags = bindFlags,
      Usage = ResourceUsage.Default,
      ElementCount = _elementCount
    };
  }

  /// <summary>
  /// ??????? ???????? ??? Raw Buffer (ByteAddressBuffer)
  /// </summary>
  public static BufferDescription CreateRawBuffer(ulong _sizeInBytes, string _name = "", bool _allowUAV = false)
  {
    var bindFlags = BindFlags.ShaderResource;
    if(_allowUAV)
      bindFlags |= BindFlags.UnorderedAccess;

    return new BufferDescription
    {
      Name = _name,
      Size = _sizeInBytes,
      Stride = 0,
      StructureByteStride = 0,
      BufferUsage = BufferUsage.Raw,
      BindFlags = bindFlags,
      Usage = ResourceUsage.Default,
      MiscFlags = ResourceMiscFlags.BufferAllowRawViews
    };
  }

  public override ulong GetMemorySize()
  {
    return Size;
  }

  public override bool IsCompatible(ResourceDescription _other)
  {
    if(_other is not BufferDescription otherBuffer)
      return false;

    return Size == otherBuffer.Size &&
           Stride == otherBuffer.Stride &&
           BufferUsage == otherBuffer.BufferUsage;
  }

  public override ResourceDescription Clone()
  {
    return new BufferDescription
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

    if(IsConstant() && Size % 256 != 0)
    {
      _errorMessage = "Constant buffer size must be aligned to 256 bytes";
      return false;
    }

    if(IsStructured() && StructureByteStride == 0)
    {
      _errorMessage = "Structured buffer must have StructureByteStride > 0";
      return false;
    }

    return true;
  }

  public override string ToString()
  {
    return $"BufferDescription(Name: '{Name}', Size: {Size}, Usage: {BufferUsage}, Stride: {Stride})";
  }
}
