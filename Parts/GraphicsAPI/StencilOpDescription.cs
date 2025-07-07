using GraphicsAPI.Enums;

namespace GraphicsAPI;

public class StencilOpDescription
{
  public StencilOperation StencilFailOp { get; set; } = StencilOperation.Keep;
  public StencilOperation StencilDepthFailOp { get; set; } = StencilOperation.Keep;
  public StencilOperation StencilPassOp { get; set; } = StencilOperation.Keep;
  public ComparisonFunction StencilFunction { get; set; } = ComparisonFunction.Always;
}