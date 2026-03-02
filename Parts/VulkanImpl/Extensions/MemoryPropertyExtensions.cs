using Resources.Enums;
using Silk.NET.Vulkan;

namespace VulkanImpl.Extensions;

public static class MemoryPropertyExtensions
{
  public static MemoryPropertyFlags ToVk(this ResourceUsage _usage) => _usage switch
  {
    ResourceUsage.Dynamic => MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
    ResourceUsage.Default => MemoryPropertyFlags.DeviceLocalBit,
    ResourceUsage.Staging => MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCachedBit,
    ResourceUsage.Immutable => MemoryPropertyFlags.DeviceLocalBit,
    _ => MemoryPropertyFlags.DeviceLocalBit
  };
}
