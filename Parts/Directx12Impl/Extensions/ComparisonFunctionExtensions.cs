using GraphicsAPI.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class ComparisonFunctionExtensions
{
  public static ComparisonFunc Convert(this ComparisonFunction _func) => _func switch
  {
    ComparisonFunction.Never => ComparisonFunc.Never,
    ComparisonFunction.Less => ComparisonFunc.Less,
    ComparisonFunction.Equal => ComparisonFunc.Equal,
    ComparisonFunction.LessEqual => ComparisonFunc.LessEqual,
    ComparisonFunction.Greater => ComparisonFunc.Greater,
    ComparisonFunction.NotEqual => ComparisonFunc.NotEqual,
    ComparisonFunction.GreaterEqual => ComparisonFunc.GreaterEqual,
    ComparisonFunction.Always => ComparisonFunc.Always,
    _ => throw new ArgumentException($"Unsupported comparison function: {_func}")
  };
}
