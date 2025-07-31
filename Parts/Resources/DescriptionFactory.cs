using Resources.Enums;

namespace Resources;

/// <summary>
/// Утилиты для создания описаний
/// </summary>
public static class DescriptionFactory
{
  /// <summary>
  /// Создать стандартное color render target описание
  /// </summary>
  public static TextureDescription CreateColorRenderTarget(uint _width, uint _height, TextureFormat _format = TextureFormat.R8G8B8A8_UNORM, string _name = "")
  {
    return new TextureDescription
    {
      Name = _name,
      Width = _width,
      Height = _height,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Format = _format,
      SampleCount = 1,
      TextureUsage = TextureUsage.RenderTarget,
      BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
      Usage = ResourceUsage.Default
    };
  }

  /// <summary>
  /// Создать стандартное depth buffer описание
  /// </summary>
  public static TextureDescription CreateDepthBuffer(uint _width, uint _height, TextureFormat _format = TextureFormat.D24_UNORM_S8_UINT, string _name = "")
  {
    return new TextureDescription
    {
      Name = _name,
      Width = _width,
      Height = _height,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Format = _format,
      SampleCount = 1,
      TextureUsage = TextureUsage.DepthStencil,
      BindFlags = BindFlags.DepthStencil,
      Usage = ResourceUsage.Default
    };
  }

  /// <summary>
  /// Создать описание для compute текстуры
  /// </summary>
  public static TextureDescription CreateComputeTexture(uint _width, uint _height, TextureFormat _format = TextureFormat.R32G32B32A32_FLOAT, string _name = "")
  {
    return new TextureDescription
    {
      Name = _name,
      Width = _width,
      Height = _height,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Format = _format,
      SampleCount = 1,
      TextureUsage = TextureUsage.UnorderedAccess,
      BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
      Usage = ResourceUsage.Default
    };
  }
}