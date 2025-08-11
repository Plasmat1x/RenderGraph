using GraphicsAPI.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class StencilOperationExtensions
{
  public static StencilOp Convert(this StencilOperation _operation) => _operation switch
  {
    StencilOperation.Keep => StencilOp.Keep,
    StencilOperation.Zero => StencilOp.Zero,
    StencilOperation.Replace => StencilOp.Replace,
    StencilOperation.IncrementSaturated => StencilOp.IncrSat,
    StencilOperation.DecrementSaturated => StencilOp.DecrSat,
    StencilOperation.Invert => StencilOp.Invert,
    StencilOperation.Increment => StencilOp.Incr,
    StencilOperation.Decrement => StencilOp.Decr,
    _ => StencilOp.Keep
  };
}