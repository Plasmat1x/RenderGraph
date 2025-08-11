using GraphicsAPI.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class BlendOperationExtensions
{
  public static BlendOp Convert(this BlendOperation _operation) => _operation switch
  {
    BlendOperation.Add => BlendOp.Add,
    BlendOperation.Subtract => BlendOp.Subtract,
    BlendOperation.ReverseSubtract => BlendOp.RevSubtract,
    BlendOperation.Min => BlendOp.Min,
    BlendOperation.Max => BlendOp.Max,
    _ => BlendOp.Add
  };
}