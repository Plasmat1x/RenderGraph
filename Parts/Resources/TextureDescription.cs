using Resources.Enums;
using Resources.Extensions;

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
    uint bytesPerPixel = Format.GetFormatSize();

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
}
