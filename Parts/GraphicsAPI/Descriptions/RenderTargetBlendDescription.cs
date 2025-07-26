using GraphicsAPI.Enums;

namespace GraphicsAPI.Descriptions;

public class RenderTargetBlendDescription
{
  public bool BlendEnable { get; set; } = false;
  public BlendFactor SrcBlend { get; set; } = BlendFactor.One;
  public BlendFactor DstBlend { get; set; } = BlendFactor.Zero;
  public BlendOperation BlendOp { get; set; } = BlendOperation.Add;
  public BlendFactor SrcBlendAlpha { get; set; } = BlendFactor.One;
  public BlendFactor DstBlendAlpha { get; set; } = BlendFactor.Zero;
  public BlendOperation BlendOpAlpha { get; set; } = BlendOperation.Add;
  public ColorWriteMask WriteMask { get; set; } = ColorWriteMask.All;
}