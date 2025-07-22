namespace GraphicsAPI.Descriptions;

public class RenderStateDescription
{
  public string Name { get; set; }
  public BlendStateDescription BlendState { get; set; } = new();
  public DepthStencilStateDescription DepthStencilState { get; set; } = new();
  public RasterizerStateDescription RasterizerState { get; set; } = new();
}
