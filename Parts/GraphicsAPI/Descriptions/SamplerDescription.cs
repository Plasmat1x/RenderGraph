using GraphicsAPI.Enums;

using System.Numerics;

namespace GraphicsAPI.Descriptions;

public class SamplerDescription
{
  public string Name { get; set; } = string.Empty;
  public FilterMode MinFilter { get; set; } = FilterMode.Linear;
  public FilterMode MagFilter { get; set; } = FilterMode.Linear;
  public FilterMode MipFilter { get; set; } = FilterMode.Linear;
  public AddressMode AddressModeU { get; set; } = AddressMode.Clamp;
  public AddressMode AddressModeV { get; set; } = AddressMode.Clamp;
  public AddressMode AddressModeW { get; set; } = AddressMode.Clamp;
  public float MinLOD { get; set; } = 0.0f;
  public float MaxLOD { get; set; } = float.MaxValue;
  public float LODBias { get; set; } = 0.0f;
  public uint MaxAnisotropy { get; set; } = 1;
  public ComparisonFunction ComparisonFunction { get; set; } = ComparisonFunction.Never;
  public Vector4 BorderColor { get; set; } = new Vector4(0, 0, 0, 0);
}
