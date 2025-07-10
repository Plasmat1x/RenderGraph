namespace GraphicsAPI;

public class RenderStateDescription
{
  public BlendStateDescription BlendState { get; set; } = new();
  public DepthStencilStateDescription DepthStencilState { get; set; } = new();
  public RasterizerStateDescription RasterizerState { get; set; } = new();
}