using Resources.Enums;

namespace Resources;
public class TextureDescription: ResourceDescription
{
  public uint Width { get; set; } = 1;
  public uint Height { get; set; } = 1;
  public uint Depth { get; set; } = 1;
  public uint MipLevels { get; set; } = 1;
  public uint ArraySize { get; set; } = 1;
  public TextureFormat Format { get; set; } = TextureFormat.R8G8B8A8_UNORM;
  public uint SampleCount { get; set; } = 1;
  public uint SampleQuality { get; set; } = 0;
  public TextureUsage TextureUsage { get; set; } = TextureUsage.ShaderResource;

  public List<TextureDescription> CreateMipChain()
  {
    var mipChain = new List<TextureDescription>();

    uint w = Width, h = Height, d = Depth;

    for(uint mip = 0; mip < MipLevels; mip++)
    {
      var mipDesc = new TextureDescription
      {
        Name = $"{Name}_Mip{mip}",
        Width = w,
        Height = h,
        Depth = d,
        MipLevels = 1,
        ArraySize = ArraySize,
        Format = Format,
        SampleCount = SampleCount,
        SampleQuality = SampleQuality,
        TextureUsage = TextureUsage,
        Usage = Usage,
        BindFlags = BindFlags,
        CPUAccessFlags = CPUAccessFlags,
        MiscFlags = MiscFlags
      };

      mipChain.Add(mipDesc);

      w = Math.Max(1, w / 2);
      h = Math.Max(1, h / 2);
      d = Math.Max(1, d / 2);
    }

    return mipChain;
  }

  public bool IsRenderTarget()
  {
    return TextureUsage == TextureUsage.RenderTarget ||
           (BindFlags & BindFlags.RenderTarget) != 0;
  }

  public bool IsDepthStencil()
  {
    return TextureUsage == TextureUsage.DepthStencil ||
           (BindFlags & BindFlags.DepthStencil) != 0;
  }

  public bool IsShaderResource()
  {
    return TextureUsage == TextureUsage.ShaderResource ||
           (BindFlags & BindFlags.ShaderResource) != 0;
  }

  public bool IsUnorderedAccess()
  {
    return TextureUsage == TextureUsage.UnorderedAccess ||
           (BindFlags & BindFlags.UnorderedAccess) != 0;
  }

  public bool IsMultisampled() => SampleCount > 1;

  public bool IsArray() => ArraySize > 1;

  public bool IsVolume() => Depth > 1;

  public bool IsCubemap() => (MiscFlags & ResourceMiscFlags.TextureCube) != 0;


  public override string ToString() => $"TextureDescription(Name: '{Name}', Size: {Width}x{Height}x{Depth}, Format: {Format}, Mips: {MipLevels}, Array: {ArraySize})";

  public override ulong GetMemorySize()
  {
    uint bytesPerPixel = GetBytesPerPixel(Format);

    ulong levelSize = (ulong)(Width * Height * Depth * bytesPerPixel * SampleCount);

    ulong totalSize = 0;
    uint w = Width, h = Height, d = Depth;

    for(uint mip = 0; mip < MipLevels; mip++)
    {
      totalSize += (ulong)(w * h * d * bytesPerPixel * SampleCount);

      w = Math.Max(1, w / 2);
      h = Math.Max(1, h / 2);
      d = Math.Max(1, d / 2);
    }

    totalSize *= ArraySize;

    return totalSize;
  }

  public override ResourceDescription Clone()
  {
    return new TextureDescription
    {
      Name = Name,
      Width = Width,
      Height = Height,
      Depth = Depth,
      MipLevels = MipLevels,
      ArraySize = ArraySize,
      Format = Format,
      SampleCount = SampleCount,
      SampleQuality = SampleQuality,
      TextureUsage = TextureUsage,
      Usage = Usage,
      BindFlags = BindFlags,
      CPUAccessFlags = CPUAccessFlags,
      MiscFlags = MiscFlags
    };
  }

  public override bool IsCompatible(ResourceDescription _other)
  {
    if(_other is not TextureDescription otherTexture)
      return false;

    return Width == otherTexture.Width &&
           Height == otherTexture.Height &&
           Depth == otherTexture.Depth &&
           Format == otherTexture.Format &&
           MipLevels == otherTexture.MipLevels &&
           SampleCount == otherTexture.SampleCount &&
           SampleQuality == otherTexture.SampleQuality &&
           ArraySize == otherTexture.ArraySize;
  }

  public override bool Validate(out string _errorMessage)
  {

    if(!base.Validate(out _errorMessage))
      return false;

    if(Width == 0 || Height == 0 || Depth == 0)
    {
      _errorMessage = "Texture dimensions must be greater than 0";
      return false;
    }

    if(MipLevels == 0)
    {
      _errorMessage = "Texture must have at least 1 mip level";
      return false;
    }

    if(ArraySize == 0)
    {
      _errorMessage = "Texture array size must be greater than 0";
      return false;
    }

    if(SampleCount == 0)
    {
      _errorMessage = "Sample count must be greater than 0";
      return false;
    }

    if(SampleCount > 1 && MipLevels > 1)
    {
      _errorMessage = "Multisampled textures cannot have multiple mip levels";
      return false;
    }

    if(IsCubemap() && Width != Height)
    {
      _errorMessage = "Cube map textures must be square (Width == Height)";
      return false;
    }

    if(IsCubemap() && ArraySize % 6 != 0)
    {
      _errorMessage = "Cube map array size must be multiple of 6";
      return false;
    }

    return true;
  }

  private static uint GetBytesPerPixel(TextureFormat _format)
  {
    return _format switch
    {
      // 32-bit formats
      TextureFormat.R32G32B32A32_TYPELESS or
      TextureFormat.R32G32B32A32_FLOAT or
      TextureFormat.R32G32B32A32_UINT or
      TextureFormat.R32G32B32A32_SINT => 16,

      // 24-bit formats (actually 32-bit aligned)
      TextureFormat.R32G32B32_TYPELESS or
      TextureFormat.R32G32B32_FLOAT or
      TextureFormat.R32G32B32_UINT or
      TextureFormat.R32G32B32_SINT => 12,

      // 16-bit formats
      TextureFormat.R16G16B16A16_TYPELESS or
      TextureFormat.R16G16B16A16_FLOAT or
      TextureFormat.R16G16B16A16_UNORM or
      TextureFormat.R16G16B16A16_UINT or
      TextureFormat.R16G16B16A16_SNORM or
      TextureFormat.R16G16B16A16_SINT => 8,

      TextureFormat.R32G32_TYPELESS or
      TextureFormat.R32G32_FLOAT or
      TextureFormat.R32G32_UINT or
      TextureFormat.R32G32_SINT => 8,

      // 8-bit formats
      TextureFormat.R10G10B10A2_TYPELESS or
      TextureFormat.R10G10B10A2_UNORM or
      TextureFormat.R10G10B10A2_UINT or
      TextureFormat.R11G11B10_FLOAT or
      TextureFormat.R8G8B8A8_TYPELESS or
      TextureFormat.R8G8B8A8_UNORM or
      TextureFormat.R8G8B8A8_UNORM_SRGB or
      TextureFormat.R8G8B8A8_UINT or
      TextureFormat.R8G8B8A8_SNORM or
      TextureFormat.R8G8B8A8_SINT => 4,

      TextureFormat.R16G16_TYPELESS or
      TextureFormat.R16G16_FLOAT or
      TextureFormat.R16G16_UNORM or
      TextureFormat.R16G16_UINT or
      TextureFormat.R16G16_SNORM or
      TextureFormat.R16G16_SINT => 4,

      TextureFormat.R32_TYPELESS or
      TextureFormat.D32_FLOAT or
      TextureFormat.R32_FLOAT or
      TextureFormat.R32_UINT or
      TextureFormat.R32_SINT => 4,

      TextureFormat.R24G8_TYPELESS or
      TextureFormat.D24_UNORM_S8_UINT or
      TextureFormat.R24_UNORM_X8_TYPELESS or
      TextureFormat.X24_TYPELESS_G8_UINT => 4,

      TextureFormat.R8G8_TYPELESS or
      TextureFormat.R8G8_UNORM or
      TextureFormat.R8G8_UINT or
      TextureFormat.R8G8_SNORM or
      TextureFormat.R8G8_SINT => 2,

      TextureFormat.R16_TYPELESS or
      TextureFormat.R16_FLOAT or
      TextureFormat.D16_UNORM or
      TextureFormat.R16_UNORM or
      TextureFormat.R16_UINT or
      TextureFormat.R16_SNORM or
      TextureFormat.R16_SINT => 2,

      TextureFormat.R8_TYPELESS or
      TextureFormat.R8_UNORM or
      TextureFormat.R8_UINT or
      TextureFormat.R8_SNORM or
      TextureFormat.R8_SINT or
      TextureFormat.A8_UNORM => 1,

      // Compressed formats (приблизительный размер)
      TextureFormat.BC1_TYPELESS or
      TextureFormat.BC1_UNORM or
      TextureFormat.BC1_UNORM_SRGB => 1, // 4:1 compression

      TextureFormat.BC2_TYPELESS or
      TextureFormat.BC2_UNORM or
      TextureFormat.BC2_UNORM_SRGB or
      TextureFormat.BC3_TYPELESS or
      TextureFormat.BC3_UNORM or
      TextureFormat.BC3_UNORM_SRGB => 1, // 4:1 compression

      TextureFormat.BC4_TYPELESS or
      TextureFormat.BC4_UNORM or
      TextureFormat.BC4_SNORM => 1, // 2:1 compression

      TextureFormat.BC5_TYPELESS or
      TextureFormat.BC5_UNORM or
      TextureFormat.BC5_SNORM => 1, // 2:1 compression

      TextureFormat.BC6H_TYPELESS or
      TextureFormat.BC6H_UF16 or
      TextureFormat.BC6H_SF16 or
      TextureFormat.BC7_TYPELESS or
      TextureFormat.BC7_UNORM or
      TextureFormat.BC7_UNORM_SRGB => 1, // 3:1 compression

      _ => 4 // Default fallback
    };
  }
}
