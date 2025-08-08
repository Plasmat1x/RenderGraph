using GraphicsAPI.Enums;

namespace Directx12Impl;

internal class CachedShaderInfo
{
  public string Hash { get; set; }
  public string Name { get; set; }
  public ShaderStage Stage { get; set; }
  public string ShaderModel { get; set; }
  public DateTime CachedTime { get; set; }
}