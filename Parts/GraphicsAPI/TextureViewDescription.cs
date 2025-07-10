using GraphicsAPI.Enums;

using Resources.Enums;

namespace GraphicsAPI;

public class TextureViewDescription
{
  public TextureViewType ViewType { get; set; }
  public TextureFormat Format { get; set; }
  public uint MostDetailedMip { get; set; } = 0;
  public uint MipLevels { get; set; } = 1;
  public uint FirstArraySlice { get; set; } = 0;
  public uint ArraySize { get; set; } = 1;
  public ComponentMapping ComponentMapping { get; set; } = ComponentMapping.Identity;
}