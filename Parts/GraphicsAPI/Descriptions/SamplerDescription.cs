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

  /// <summary>
  /// Point sampling (nearest neighbor) без filtering
  /// </summary>
  public static SamplerDescription PointWrap => new()
  {
    MinFilter = FilterMode.Point,
    MagFilter = FilterMode.Point,
    MipFilter = FilterMode.Point,
    AddressModeU = AddressMode.Wrap,
    AddressModeV = AddressMode.Wrap,
    AddressModeW = AddressMode.Wrap,
    LODBias = 0.0f,
    MaxAnisotropy = 1,
    ComparisonFunction = ComparisonFunction.Always,
    BorderColor = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f ),
    MinLOD = 0.0f,
    MaxLOD = float.MaxValue
  };

  /// <summary>
  /// Point sampling с clamp addressing
  /// </summary>
  public static SamplerDescription PointClamp => new()
  {
    MinFilter = FilterMode.Point,
    MagFilter = FilterMode.Point,
    MipFilter = FilterMode.Point,
    AddressModeU = AddressMode.Clamp,
    AddressModeV = AddressMode.Clamp,
    AddressModeW = AddressMode.Clamp,
    LODBias = 0.0f,
    MaxAnisotropy = 1,
    ComparisonFunction = ComparisonFunction.Always,
    BorderColor = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f ),
    MinLOD = 0.0f,
    MaxLOD = float.MaxValue
  };

  /// <summary>
  /// Linear filtering с wrap addressing (стандартный для большинства текстур)
  /// </summary>
  public static SamplerDescription LinearWrap => new()
  {
    MinFilter = FilterMode.Linear,
    MagFilter = FilterMode.Linear,
    MipFilter = FilterMode.Linear,
    AddressModeU = AddressMode.Wrap,
    AddressModeV = AddressMode.Wrap,
    AddressModeW = AddressMode.Wrap,
    LODBias = 0.0f,
    MaxAnisotropy = 1,
    ComparisonFunction = ComparisonFunction.Always,
    BorderColor = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f ),
    MinLOD = 0.0f,
    MaxLOD = float.MaxValue
  };

  /// <summary>
  /// Linear filtering с clamp addressing
  /// </summary>
  public static SamplerDescription LinearClamp => new()
  {
    MinFilter = FilterMode.Linear,
    MagFilter = FilterMode.Linear,
    MipFilter = FilterMode.Linear,
    AddressModeU = AddressMode.Clamp,
    AddressModeV = AddressMode.Clamp,
    AddressModeW = AddressMode.Clamp,
    LODBias = 0.0f,
    MaxAnisotropy = 1,
    ComparisonFunction = ComparisonFunction.Always,
    BorderColor = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f ),
    MinLOD = 0.0f,
    MaxLOD = float.MaxValue
  };

  /// <summary>
  /// Anisotropic filtering для высококачественного сэмплинга
  /// </summary>
  public static SamplerDescription AnisotropicWrap => new()
  {
    MinFilter = FilterMode.Anisotropic,
    MagFilter = FilterMode.Anisotropic,
    MipFilter = FilterMode.Anisotropic,
    AddressModeU = AddressMode.Wrap,
    AddressModeV = AddressMode.Wrap,
    AddressModeW = AddressMode.Wrap,
    LODBias = 0.0f,
    MaxAnisotropy = 16,
    ComparisonFunction = ComparisonFunction.Always,
    BorderColor = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f ),
    MinLOD = 0.0f,
    MaxLOD = float.MaxValue
  };

  /// <summary>
  /// Anisotropic filtering с clamp addressing
  /// </summary>
  public static SamplerDescription AnisotropicClamp => new()
  {
    MinFilter = FilterMode.Anisotropic,
    MagFilter = FilterMode.Anisotropic,
    MipFilter = FilterMode.Anisotropic,
    AddressModeU = AddressMode.Clamp,
    AddressModeV = AddressMode.Clamp,
    AddressModeW = AddressMode.Clamp,
    LODBias = 0.0f,
    MaxAnisotropy = 16,
    ComparisonFunction = ComparisonFunction.Always,
    BorderColor = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f ),
    MinLOD = 0.0f,
    MaxLOD = float.MaxValue
  };

  /// <summary>
  /// Shadow map comparison sampler
  /// </summary>
  public static SamplerDescription ShadowMap => new()
  {
    MinFilter = FilterMode.Linear,
    MagFilter = FilterMode.Linear,
    MipFilter = FilterMode.Point,
    AddressModeU = AddressMode.Border,
    AddressModeV = AddressMode.Border,
    AddressModeW = AddressMode.Border,
    LODBias = 0.0f,
    MaxAnisotropy = 1,
    ComparisonFunction = ComparisonFunction.LessEqual,
    BorderColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f ),
    MinLOD = 0.0f,
    MaxLOD = float.MaxValue
  };
}
