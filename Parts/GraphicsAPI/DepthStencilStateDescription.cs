using GraphicsAPI.Enums;

namespace GraphicsAPI;

public class DepthStencilStateDescription
{
  public bool DepthEnable { get; set; } = true;
  public bool DepthWriteEnable { get; set; } = true;
  public ComparisonFunction DepthFunction { get; set; } = ComparisonFunction.Less;
  public bool StencilEnable { get; set; } = false;
  public byte StencilReadMask { get; set; } = 0xff;
  public byte StencilWriteMask { get; set; } = 0xff;
  public StencilOpDescription FrontFace { get; set; } = new();
  public StencilOpDescription BackFace { get; set; } = new();
}