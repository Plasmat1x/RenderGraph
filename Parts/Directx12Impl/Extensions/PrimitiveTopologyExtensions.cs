using GraphicsAPI.Enums;

using Silk.NET.Core.Native;
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

  public static D3DPrimitiveTopology ConvertToCmd(this PrimitiveTopology _topology) => _topology switch
  {
    PrimitiveTopology.PointList => D3DPrimitiveTopology.D3DPrimitiveTopologyPointlist,
    PrimitiveTopology.LineList => D3DPrimitiveTopology.D3DPrimitiveTopologyLinelist,
    PrimitiveTopology.LineStrip => D3DPrimitiveTopology.D3DPrimitiveTopologyLinestrip,
    PrimitiveTopology.TriangleList => D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist,
    PrimitiveTopology.TriangleStrip => D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglestrip,
    _ => D3DPrimitiveTopology.D3DPrimitiveTopologyUndefined
  };
}