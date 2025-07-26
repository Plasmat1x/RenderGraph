using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Descriptions;

public class InputLayoutDescription
{
  public static InputLayoutDescription FromShader(IShader _shader)
  {
    throw new NotImplementedException("For extract input layout from shader need shader reflection implementation");
  }

  public List<InputElementDescription> Elements { get; set; } = [];
}
