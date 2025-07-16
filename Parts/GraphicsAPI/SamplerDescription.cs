using GraphicsAPI.Enums;

using System.Numerics;

namespace GraphicsAPI;

public class SamplerDescription
{

  public static SamplerDescription CreatePointWrap(string _name = "PointWrap") => new SamplerDescription
  {
    Name = _name,
    MinFilter = FilterMode.Point,
    MagFilter = FilterMode.Point,
    MipFilter = FilterMode.Point,
    AddressModeU = AddressMode.Wrap,
    AddressModeV = AddressMode.Wrap,
    AddressModeW = AddressMode.Wrap,
  };

  public static SamplerDescription CreatePointClamp(string _name = "PointClamp") => new SamplerDescription
  {
    Name = _name,
    MinFilter = FilterMode.Point,
    MagFilter = FilterMode.Point,
    MipFilter = FilterMode.Point,
    AddressModeU = AddressMode.Clamp,
    AddressModeV = AddressMode.Clamp,
    AddressModeW = AddressMode.Clamp,
  };
  public static SamplerDescription CreateLinearWrap(string _name = "LineraWrap") => new SamplerDescription
  {
    Name = _name,
    MinFilter = FilterMode.Linear,
    MagFilter = FilterMode.Linear,
    MipFilter = FilterMode.Linear,
    AddressModeU = AddressMode.Wrap,
    AddressModeV = AddressMode.Wrap,
    AddressModeW = AddressMode.Wrap,
  };

  public static SamplerDescription CreateLinearClamp(string _name = "LinearClamp") => new SamplerDescription
  {
    Name = _name,
    MinFilter = FilterMode.Linear,
    MagFilter = FilterMode.Linear,
    MipFilter = FilterMode.Linear,
    AddressModeU = AddressMode.Clamp,
    AddressModeV = AddressMode.Clamp,
    AddressModeW = AddressMode.Clamp,
  };

  public static SamplerDescription CreateAnisatropic(uint _maxAnisotropy = 16, string _name = "Anisotropic") => new SamplerDescription
  {
    Name = _name,
    MinFilter = FilterMode.Anisotropic,
    MagFilter = FilterMode.Anisotropic,
    MipFilter = FilterMode.Linear,
    MaxAnisotropy = Math.Clamp(_maxAnisotropy, 1u, 16u),
    AddressModeU = AddressMode.Wrap,
    AddressModeV = AddressMode.Wrap,
    AddressModeW = AddressMode.Wrap,
  };

  public static SamplerDescription CreateComparisonSampler(ComparisonFunction _func = ComparisonFunction.LessEqual, string _name = "ComparisonSampler") => new SamplerDescription
  {
    Name = _name,
    MinFilter = FilterMode.Linear,
    MagFilter = FilterMode.Linear,
    MipFilter = FilterMode.Linear,
    ComparisonFunction = _func,
    AddressModeU = AddressMode.Clamp,
    AddressModeV = AddressMode.Clamp,
    AddressModeW = AddressMode.Clamp,
  };

  public static SamplerDescription CreateShadowSampler(string name = "ShadowSampler") => CreateComparisonSampler(ComparisonFunction.LessEqual, name);

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