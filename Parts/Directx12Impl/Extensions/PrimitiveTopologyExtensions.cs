using GraphicsAPI.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class PrimitiveTopologyExtensions
{
  public static PrimitiveTopologyType Convert(this PrimitiveTopology _topology) => _topology switch
  {
    PrimitiveTopology.PointList => PrimitiveTopologyType.Point,
    PrimitiveTopology.LineList or PrimitiveTopology.LineStrip => PrimitiveTopologyType.Line,
    PrimitiveTopology.TriangleList or PrimitiveTopology.TriangleStrip => PrimitiveTopologyType.Triangle,
    _ => PrimitiveTopologyType.Undefined
  };
}