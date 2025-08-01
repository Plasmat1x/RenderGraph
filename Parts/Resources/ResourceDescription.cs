using Resources.Enums;

namespace Resources;

public abstract class ResourceDescription
{
  public string Name { get; set; } = string.Empty;
  public ResourceUsage Usage { get; set; } = ResourceUsage.Default;
  public BindFlags BindFlags { get; set; } = BindFlags.None;
  public CPUAccessFlags CPUAccessFlags { get; set; } = CPUAccessFlags.None;
  public ResourceMiscFlags MiscFlags { get; set; } = ResourceMiscFlags.None;

  public abstract ulong GetMemorySize();
  public abstract bool IsCompatible(ResourceDescription _other);

  public virtual bool RequiresCPUAccess() => CPUAccessFlags != CPUAccessFlags.None;

  public virtual bool IsReadOnlyCPU() => CPUAccessFlags == CPUAccessFlags.Read;

  public virtual bool IsWriteOnlyCPU() => CPUAccessFlags == CPUAccessFlags.Write;

  public virtual bool IsReadWriteCPU() => CPUAccessFlags == CPUAccessFlags.ReadWrite;

  public virtual bool IsImmutable() => Usage == ResourceUsage.Immutable;

  public virtual bool IsDynamic() => Usage == ResourceUsage.Dynamic;

  public virtual bool IsStaging() => Usage == ResourceUsage.Staging;

  public virtual bool IsDefault() => Usage == ResourceUsage.Default;

  public virtual bool SupportsShaderResource() => (BindFlags & BindFlags.ShaderResource) != 0;

  public virtual bool SupportsRenderTarget() => (BindFlags & BindFlags.RenderTarget) != 0;

  public virtual bool SupportsDepthStencil() => (BindFlags & BindFlags.DepthStencil) != 0;

  public virtual bool SupportsUnorderedAccess() => (BindFlags & BindFlags.UnorderedAccess) != 0;

  public virtual bool SupportsVertexBuffer() => (BindFlags & BindFlags.VertexBuffer) != 0;

  public virtual bool SupportsIndexBuffer() => (BindFlags & BindFlags.IndexBuffer) != 0;

  public virtual bool SupportsConstantBuffer() => (BindFlags & BindFlags.ConstantBuffer) != 0;

  public virtual bool IsShared() => (MiscFlags & ResourceMiscFlags.Shared) != 0;

  public virtual bool CanGenerateMips() => (MiscFlags & ResourceMiscFlags.GenerateMips) != 0;

  public virtual bool IsGDICompatible() => (MiscFlags & ResourceMiscFlags.GDICompatible) != 0;

  public virtual bool Validate(out string _errorMessage)
  {
    _errorMessage = string.Empty;

    if(string.IsNullOrEmpty(Name))
    {
      _errorMessage = "Resource name cannot be empty";
      return false;
    }

    if(Usage == ResourceUsage.Immutable && CPUAccessFlags != CPUAccessFlags.None)
    {
      _errorMessage = "Immutable resources cannot have CPU access";
      return false;
    }

    if(Usage == ResourceUsage.Dynamic && CPUAccessFlags != CPUAccessFlags.Write)
    {
      _errorMessage = "Dynamic resources must have Write CPU access";
      return false;
    }

    if(Usage == ResourceUsage.Staging &&
        CPUAccessFlags != CPUAccessFlags.Read &&
        CPUAccessFlags != CPUAccessFlags.Write &&
        CPUAccessFlags != CPUAccessFlags.ReadWrite)
    {
      _errorMessage = "Staging resources must have CPU access";
      return false;
    }

    if((BindFlags & BindFlags.ConstantBuffer) != 0 &&
        (BindFlags & (BindFlags.VertexBuffer | BindFlags.IndexBuffer)) != 0)
    {
      _errorMessage = "Constant buffer cannot be bound as vertex/index buffer";
      return false;
    }

    if(Usage == ResourceUsage.Staging && BindFlags != BindFlags.None)
    {
      _errorMessage = "Staging resources cannot have bind flags";
      return false;
    }

    return true;
  }

  public virtual string GetBindFlagsString()
  {
    if(BindFlags == BindFlags.None)
      return "None";

    var flags = new List<string>();

    if((BindFlags & BindFlags.VertexBuffer) != 0)
      flags.Add("VertexBuffer");
    if((BindFlags & BindFlags.IndexBuffer) != 0)
      flags.Add("IndexBuffer");
    if((BindFlags & BindFlags.ConstantBuffer) != 0)
      flags.Add("ConstantBuffer");
    if((BindFlags & BindFlags.ShaderResource) != 0)
      flags.Add("ShaderResource");
    if((BindFlags & BindFlags.StreamOutput) != 0)
      flags.Add("StreamOutput");
    if((BindFlags & BindFlags.RenderTarget) != 0)
      flags.Add("RenderTarget");
    if((BindFlags & BindFlags.DepthStencil) != 0)
      flags.Add("DepthStencil");
    if((BindFlags & BindFlags.UnorderedAccess) != 0)
      flags.Add("UnorderedAccess");
    if((BindFlags & BindFlags.Decoder) != 0)
      flags.Add("Decoder");
    if((BindFlags & BindFlags.VideoEncoder) != 0)
      flags.Add("VideoEncoder");

    return string.Join(" | ", flags);
  }

  public virtual string GetCPUAccessFlagsString()
  {
    return CPUAccessFlags switch
    {
      CPUAccessFlags.None => "None",
      CPUAccessFlags.Read => "Read",
      CPUAccessFlags.Write => "Write",
      CPUAccessFlags.ReadWrite => "ReadWrite",
      _ => CPUAccessFlags.ToString()
    };
  }

  public virtual string GetMiscFlagsString()
  {
    if(MiscFlags == ResourceMiscFlags.None)
      return "None";

    var flags = new List<string>();

    if((MiscFlags & ResourceMiscFlags.GenerateMips) != 0)
      flags.Add("GenerateMips");
    if((MiscFlags & ResourceMiscFlags.Shared) != 0)
      flags.Add("Shared");
    if((MiscFlags & ResourceMiscFlags.TextureCube) != 0)
      flags.Add("TextureCube");
    if((MiscFlags & ResourceMiscFlags.DrawIndirectArgs) != 0)
      flags.Add("DrawIndirectArgs");
    if((MiscFlags & ResourceMiscFlags.BufferAllowRawViews) != 0)
      flags.Add("BufferAllowRawViews");
    if((MiscFlags & ResourceMiscFlags.BufferStructured) != 0)
      flags.Add("BufferStructured");
    if((MiscFlags & ResourceMiscFlags.ResourceClamp) != 0)
      flags.Add("ResourceClamp");
    if((MiscFlags & ResourceMiscFlags.SharedKeyedMutex) != 0)
      flags.Add("SharedKeyedMutex");
    if((MiscFlags & ResourceMiscFlags.GDICompatible) != 0)
      flags.Add("GDICompatible");

    return string.Join(" | ", flags);
  }

  public virtual ulong GetMemoryAlignment()
  {
    if(SupportsConstantBuffer())
      return 256;

    if(this is TextureDescription)
      return 512;

    if(this is BufferDescription bufferDesc && bufferDesc.IsStructured())
      return Math.Max(16, bufferDesc.StructureByteStride);

    return 16;
  }

  public virtual bool CanAliasWith(ResourceDescription _other)
  {
    if(_other == null)
      return false;

    return GetMemorySize() == _other.GetMemorySize() &&
           GetMemoryAlignment() == _other.GetMemoryAlignment();
  }

  public virtual ResourceDescription Clone()
  {
    throw new NotImplementedException("Clone must be implemented in derived classes");
  }

  public virtual void ApplyModifications(Action<ResourceDescription> _modifier)
  {
    _modifier?.Invoke(this);

    if(!Validate(out string error))
    {
      throw new InvalidOperationException($"Invalid resource description after modifications: {error}");
    }
  }

  public override bool Equals(object? _obj)
  {
    if(_obj is not ResourceDescription other)
      return false;

    return Name == other.Name &&
           Usage == other.Usage &&
           BindFlags == other.BindFlags &&
           CPUAccessFlags == other.CPUAccessFlags &&
           MiscFlags == other.MiscFlags;
  }

  public override int GetHashCode()
  {
    unchecked
    {
      int hash = 17;
      hash = hash * 23 + (Name?.GetHashCode() ?? 0);
      hash = hash * 23 + Usage.GetHashCode();
      hash = hash * 23 + BindFlags.GetHashCode();
      hash = hash * 23 + CPUAccessFlags.GetHashCode();
      hash = hash * 23 + MiscFlags.GetHashCode();
      return hash;
    }
  }

  public override string ToString()
  {
    return $"ResourceDescription(Name: '{Name}', Usage: {Usage}, " +
           $"BindFlags: {GetBindFlagsString()}, " +
           $"CPUAccess: {GetCPUAccessFlagsString()}, " +
           $"Size: {GetMemorySize()} bytes)";
  }
}
