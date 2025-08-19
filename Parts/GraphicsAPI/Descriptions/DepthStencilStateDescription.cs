using GraphicsAPI.Enums;

namespace GraphicsAPI.Descriptions;

public class DepthStencilStateDescription
{
  public bool DepthEnable { get; set; } = true;
  public bool DepthWriteEnable { get; set; } = true;
  public byte DepthWriteMask { get; set; } = 0xff;
  public ComparisonFunction DepthFunction { get; set; } = ComparisonFunction.Less;
  public bool StencilEnable { get; set; } = false;
  public byte StencilReadMask { get; set; } = 0xff;
  public byte StencilWriteMask { get; set; } = 0xff;
  public StencilOpDescription FrontFace { get; set; } = new();
  public StencilOpDescription BackFace { get; set; } = new();

  /// <summary>
  /// Стандартное состояние depth-stencil с включенным depth test и записью
  /// </summary>
  public static DepthStencilStateDescription Default => new()
  {
    DepthEnable = true,
    DepthWriteMask = 0xff,
    DepthFunction = ComparisonFunction.Less,
    StencilEnable = false,
    StencilReadMask = 0xFF,
    StencilWriteMask = 0xFF,
    FrontFace = new StencilOpDescription
    {
      StencilFailOp = StencilOperation.Keep,
      StencilDepthFailOp = StencilOperation.Keep,
      StencilPassOp = StencilOperation.Keep,
      StencilFunction = ComparisonFunction.Always
    },
    BackFace = new StencilOpDescription
    {
      StencilFailOp = StencilOperation.Keep,
      StencilDepthFailOp = StencilOperation.Keep,
      StencilPassOp = StencilOperation.Keep,
      StencilFunction = ComparisonFunction.Always
    }
  };

  /// <summary>
  /// Depth test включен, но запись отключена (только чтение depth buffer)
  /// </summary>
  public static DepthStencilStateDescription DepthRead => new()
  {
    DepthEnable = true,
    DepthWriteMask = 0x00,
    DepthFunction = ComparisonFunction.Less,
    StencilEnable = false,
    StencilReadMask = 0xFF,
    StencilWriteMask = 0xFF,
    FrontFace = new StencilOpDescription
    {
      StencilFailOp = StencilOperation.Keep,
      StencilDepthFailOp = StencilOperation.Keep,
      StencilPassOp = StencilOperation.Keep,
      StencilFunction = ComparisonFunction.Always
    },
    BackFace = new StencilOpDescription
    {
      StencilFailOp = StencilOperation.Keep,
      StencilDepthFailOp = StencilOperation.Keep,
      StencilPassOp = StencilOperation.Keep,
      StencilFunction = ComparisonFunction.Always
    }
  };

  /// <summary>
  /// Depth test полностью отключен (для UI, прозрачных объектов после сортировки)
  /// </summary>
  public static DepthStencilStateDescription None => new()
  {
    DepthEnable = false,
    DepthWriteMask = 0x00,
    DepthFunction = ComparisonFunction.Less,
    StencilEnable = false,
    StencilReadMask = 0xFF,
    StencilWriteMask = 0xFF,
    FrontFace = new StencilOpDescription
    {
      StencilFailOp = StencilOperation.Keep,
      StencilDepthFailOp = StencilOperation.Keep,
      StencilPassOp = StencilOperation.Keep,
      StencilFunction = ComparisonFunction.Always
    },
    BackFace = new StencilOpDescription
    {
      StencilFailOp = StencilOperation.Keep,
      StencilDepthFailOp = StencilOperation.Keep,
      StencilPassOp = StencilOperation.Keep,
      StencilFunction = ComparisonFunction.Always
    }
  };

  /// <summary>
  /// Reverse depth (Greater comparison) для улучшения precision
  /// </summary>
  public static DepthStencilStateDescription ReverseZ => new()
  {
    DepthEnable = true,
    DepthWriteMask = 0xff,
    DepthFunction = ComparisonFunction.Greater,
    StencilEnable = false,
    StencilReadMask = 0xFF,
    StencilWriteMask = 0xFF,
    FrontFace = new StencilOpDescription
    {
      StencilFailOp = StencilOperation.Keep,
      StencilDepthFailOp = StencilOperation.Keep,
      StencilPassOp = StencilOperation.Keep,
      StencilFunction = ComparisonFunction.Always
    },
    BackFace = new StencilOpDescription
    {
      StencilFailOp = StencilOperation.Keep,
      StencilDepthFailOp = StencilOperation.Keep,
      StencilPassOp = StencilOperation.Keep,
      StencilFunction = ComparisonFunction.Always
    }
  };
}