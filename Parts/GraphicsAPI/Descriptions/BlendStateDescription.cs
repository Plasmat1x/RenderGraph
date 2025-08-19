using GraphicsAPI.Enums;

namespace GraphicsAPI.Descriptions;

public class BlendStateDescription
{
  public bool AlphaToCoverageEnable { get; set; } = false;
  public bool IndependentBlendEnable { get; set; } = false;
  public RenderTargetBlendDescription[] RenderTargets { get; set; } = new RenderTargetBlendDescription[8];

  /// <summary>
  /// Стандартное состояние blend без альфа-блендинга (непрозрачные объекты)
  /// </summary>
  public static BlendStateDescription Opaque => new()
  {
    AlphaToCoverageEnable = false,
    IndependentBlendEnable = false,
    RenderTargets = new RenderTargetBlendDescription[]
      {
            new RenderTargetBlendDescription
            {
                BlendEnable = false,
                LogicOpEnable = false,
                SrcBlend = BlendFactor.One,
                DstBlend = BlendFactor.Zero,
                BlendOp = BlendOperation.Add,
                SrcBlendAlpha = BlendFactor.One,
                DstBlendAlpha = BlendFactor.Zero,
                BlendOpAlpha = BlendOperation.Add,
                LogicOp = LogicOperation.Noop,
                WriteMask = ColorWriteMask.All
            }
      }
  };

  /// <summary>
  /// Альфа-блендинг для прозрачных объектов (src_alpha * src + (1-src_alpha) * dst)
  /// </summary>
  public static BlendStateDescription AlphaBlend => new()
  {
    AlphaToCoverageEnable = false,
    IndependentBlendEnable = false,
    RenderTargets = new RenderTargetBlendDescription[]
      {
            new RenderTargetBlendDescription
            {
                BlendEnable = true,
                LogicOpEnable = false,
                SrcBlend = BlendFactor.SrcAlpha,
                DstBlend = BlendFactor.InvSrcAlpha,
                BlendOp = BlendOperation.Add,
                SrcBlendAlpha = BlendFactor.One,
                DstBlendAlpha = BlendFactor.InvSrcAlpha,
                BlendOpAlpha = BlendOperation.Add,
                LogicOp = LogicOperation.Noop,
                WriteMask = ColorWriteMask.All
            }
      }
  };

  /// <summary>
  /// Премультиплицированный альфа-блендинг
  /// </summary>
  public static BlendStateDescription PremultipliedAlpha => new()
  {
    AlphaToCoverageEnable = false,
    IndependentBlendEnable = false,
    RenderTargets = new RenderTargetBlendDescription[]
      {
            new RenderTargetBlendDescription
            {
                BlendEnable = true,
                LogicOpEnable = false,
                SrcBlend = BlendFactor.One,
                DstBlend = BlendFactor.InvSrcAlpha,
                BlendOp = BlendOperation.Add,
                SrcBlendAlpha = BlendFactor.One,
                DstBlendAlpha = BlendFactor.InvSrcAlpha,
                BlendOpAlpha = BlendOperation.Add,
                LogicOp = LogicOperation.Noop,
                WriteMask = ColorWriteMask.All
            }
      }
  };

  /// <summary>
  /// Аддитивный блендинг (src + dst) для эффектов света/огня
  /// </summary>
  public static BlendStateDescription Additive => new()
  {
    AlphaToCoverageEnable = false,
    IndependentBlendEnable = false,
    RenderTargets = new RenderTargetBlendDescription[]
      {
            new RenderTargetBlendDescription
            {
                BlendEnable = true,
                LogicOpEnable = false,
                SrcBlend = BlendFactor.SrcAlpha,
                DstBlend = BlendFactor.One,
                BlendOp = BlendOperation.Add,
                SrcBlendAlpha = BlendFactor.SrcAlpha,
                DstBlendAlpha = BlendFactor.One,
                BlendOpAlpha = BlendOperation.Add,
                LogicOp = LogicOperation.Noop,
                WriteMask = ColorWriteMask.All
            }
      }
  };

  /// <summary>
  /// Non-premultiplied альфа блендинг
  /// </summary>
  public static BlendStateDescription NonPremultiplied => new()
  {
    AlphaToCoverageEnable = false,
    IndependentBlendEnable = false,
    RenderTargets = new RenderTargetBlendDescription[]
      {
            new RenderTargetBlendDescription
            {
                BlendEnable = true,
                LogicOpEnable = false,
                SrcBlend = BlendFactor.SrcAlpha,
                DstBlend = BlendFactor.InvSrcAlpha,
                BlendOp = BlendOperation.Add,
                SrcBlendAlpha = BlendFactor.SrcAlpha,
                DstBlendAlpha = BlendFactor.InvSrcAlpha,
                BlendOpAlpha = BlendOperation.Add,
                LogicOp = LogicOperation.Noop,
                WriteMask = ColorWriteMask.All
            }
      }
  };
}