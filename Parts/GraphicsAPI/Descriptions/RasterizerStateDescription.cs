using GraphicsAPI.Enums;

namespace GraphicsAPI.Descriptions;

public class RasterizerStateDescription
{
  public FillMode FillMode { get; set; } = FillMode.Solid;
  public CullMode CullMode { get; set; } = CullMode.Back;
  public bool FrontCounterClockwise { get; set; } = false;
  public int DepthBias { get; set; } = 0;
  public float DepthBiasClamp { get; set; } = 0.0f;
  public float SlopeScaledDepthBias { get; set; } = 0.0f;
  public bool DepthClipEnable { get; set; } = true;
  public bool ScissorEnable { get; set; } = false;
  public bool MultisampleEnable { get; set; } = false;
  public bool AntialiasedLineEnable { get; set; } = false;
}