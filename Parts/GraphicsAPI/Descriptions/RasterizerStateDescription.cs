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

  /// <summary>
  /// Стандартное состояние рasterizer с back-face culling
  /// </summary>
  public static RasterizerStateDescription Default => new()
  {
    FillMode = FillMode.Solid,
    CullMode = CullMode.Back,
    FrontCounterClockwise = false,
    DepthBias = 0,
    DepthBiasClamp = 0.0f,
    SlopeScaledDepthBias = 0.0f,
    DepthClipEnable = true,
    ScissorEnable = false,
    MultisampleEnable = false,
    AntialiasedLineEnable = false
  };

  /// <summary>
  /// Back-face culling (стандартное поведение для большинства объектов)
  /// </summary>
  public static RasterizerStateDescription CullBack => new()
  {
    FillMode = FillMode.Solid,
    CullMode = CullMode.Back,
    FrontCounterClockwise = false,
    DepthBias = 0,
    DepthBiasClamp = 0.0f,
    SlopeScaledDepthBias = 0.0f,
    DepthClipEnable = true,
    ScissorEnable = false,
    MultisampleEnable = false,
    AntialiasedLineEnable = false
  };

  /// <summary>
  /// Front-face culling (для внутренних поверхностей, skybox)
  /// </summary>
  public static RasterizerStateDescription CullFront => new()
  {
    FillMode = FillMode.Solid,
    CullMode = CullMode.Front,
    FrontCounterClockwise = false,
    DepthBias = 0,
    DepthBiasClamp = 0.0f,
    SlopeScaledDepthBias = 0.0f,
    DepthClipEnable = true,
    ScissorEnable = false,
    MultisampleEnable = false,
    AntialiasedLineEnable = false
  };

  /// <summary>
  /// Без culling (двухсторонний рендеринг для листьев, fabric и т.д.)
  /// </summary>
  public static RasterizerStateDescription CullNone => new()
  {
    FillMode = FillMode.Solid,
    CullMode = CullMode.None,
    FrontCounterClockwise = false,
    DepthBias = 0,
    DepthBiasClamp = 0.0f,
    SlopeScaledDepthBias = 0.0f,
    DepthClipEnable = true,
    ScissorEnable = false,
    MultisampleEnable = false,
    AntialiasedLineEnable = false
  };

  /// <summary>
  /// Wireframe режим для debug визуализации
  /// </summary>
  public static RasterizerStateDescription Wireframe => new()
  {
    FillMode = FillMode.Wireframe,
    CullMode = CullMode.None,
    FrontCounterClockwise = false,
    DepthBias = 0,
    DepthBiasClamp = 0.0f,
    SlopeScaledDepthBias = 0.0f,
    DepthClipEnable = true,
    ScissorEnable = false,
    MultisampleEnable = false,
    AntialiasedLineEnable = true
  };

  /// <summary>
  /// Shadow mapping bias для устранения shadow acne
  /// </summary>
  public static RasterizerStateDescription ShadowCaster => new()
  {
    FillMode = FillMode.Solid,
    CullMode = CullMode.Back,
    FrontCounterClockwise = false,
    DepthBias = 100000,
    DepthBiasClamp = 0.0f,
    SlopeScaledDepthBias = 1.0f,
    DepthClipEnable = true,
    ScissorEnable = false,
    MultisampleEnable = false,
    AntialiasedLineEnable = false
  };
}