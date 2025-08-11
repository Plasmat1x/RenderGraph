using GraphicsAPI.Enums;

using Resources.Enums;

namespace GraphicsAPI.Descriptions;


/// <summary>
/// Обновленное описание текстурных представлений для DX12 совместимости
/// </summary>
public class TextureViewDescription
{
  public TextureViewType ViewType { get; set; }
  public TextureFormat Format { get; set; }
  public uint MostDetailedMip { get; set; } = 0;
  public uint MipLevels { get; set; } = 1;
  public uint FirstArraySlice { get; set; } = 0;
  public uint ArraySize { get; set; } = 1;
  public ComponentMapping ComponentMapping { get; set; } = ComponentMapping.Identity;

  // Дополнительные поля для DX12 совместимости
  public uint PlaneSlice { get; set; } = 0;
  public float ResourceMinLODClamp { get; set; } = 0.0f;
  public TextureViewFlags Flags { get; set; } = TextureViewFlags.None;

  /// <summary>
  /// Создать описание для Shader Resource View
  /// </summary>
  public static TextureViewDescription CreateSRV(TextureFormat _format, uint _mipLevels = 1, uint _arraySize = 1)
  {
    return new TextureViewDescription
    {
      ViewType = TextureViewType.ShaderResource,
      Format = _format,
      MostDetailedMip = 0,
      MipLevels = _mipLevels,
      FirstArraySlice = 0,
      ArraySize = _arraySize
    };
  }

  /// <summary>
  /// Создать описание для Render Target View
  /// </summary>
  public static TextureViewDescription CreateRTV(TextureFormat _format, uint _mipLevel = 0, uint _arraySize = 1)
  {
    return new TextureViewDescription
    {
      ViewType = TextureViewType.RenderTarget,
      Format = _format,
      MostDetailedMip = _mipLevel,
      MipLevels = 1,
      FirstArraySlice = 0,
      ArraySize = _arraySize
    };
  }

  /// <summary>
  /// Создать описание для Depth Stencil View
  /// </summary>
  public static TextureViewDescription CreateDSV(TextureFormat _format, uint _mipLevel = 0, uint _arraySize = 1)
  {
    return new TextureViewDescription
    {
      ViewType = TextureViewType.DepthStencil,
      Format = _format,
      MostDetailedMip = _mipLevel,
      MipLevels = 1,
      FirstArraySlice = 0,
      ArraySize = _arraySize
    };
  }

  /// <summary>
  /// Создать описание для Unordered Access View
  /// </summary>
  public static TextureViewDescription CreateUAV(TextureFormat _format, uint _mipLevel = 0, uint _arraySize = 1)
  {
    return new TextureViewDescription
    {
      ViewType = TextureViewType.UnorderedAccess,
      Format = _format,
      MostDetailedMip = _mipLevel,
      MipLevels = 1,
      FirstArraySlice = 0,
      ArraySize = _arraySize
    };
  }

  /// <summary>
  /// Валидация описания
  /// </summary>
  public bool Validate(out string _errorMessage)
  {
    _errorMessage = string.Empty;

    if(Format == TextureFormat.Unknown)
    {
      _errorMessage = "Format cannot be Unknown";
      return false;
    }

    if(MipLevels == 0)
    {
      _errorMessage = "MipLevels must be greater than 0";
      return false;
    }

    if(ArraySize == 0)
    {
      _errorMessage = "ArraySize must be greater than 0";
      return false;
    }

    return true;
  }

  public override string ToString()
  {
    return $"TextureView({ViewType}, {Format}, Mip:{MostDetailedMip}-{MostDetailedMip + MipLevels - 1}, Array:{FirstArraySlice}-{FirstArraySlice + ArraySize - 1})";
  }
}