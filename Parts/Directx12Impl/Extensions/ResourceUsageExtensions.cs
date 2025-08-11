using Resources.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class ResourceUsageExtensions
{
  public static HeapType Convert(this ResourceUsage _usage) => _usage switch
  {
    ResourceUsage.Default => HeapType.Default,
    ResourceUsage.Immutable => HeapType.Default,
    ResourceUsage.Dynamic => HeapType.Upload,
    ResourceUsage.Staging => HeapType.Readback,
    _ => HeapType.Default,
  };
}
