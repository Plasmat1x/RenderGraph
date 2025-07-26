namespace GraphicsAPI.Descriptions;

public class BlendStateDescription
{
  public bool AlphaToCoverageEnable { get; set; } = false;
  public bool IndependentBlendEnable { get; set; } = false;
  public RenderTargetBlendDescription[] RenderTargets { get; set; } = new RenderTargetBlendDescription[8];
}