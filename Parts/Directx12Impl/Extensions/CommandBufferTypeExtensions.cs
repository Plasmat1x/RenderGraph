using GraphicsAPI.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class CommandBufferTypeExtensions
{
  public static CommandListType Convert(this CommandBufferType _type) => _type switch
  {
    CommandBufferType.Direct => CommandListType.Direct,
    CommandBufferType.Bundle => CommandListType.Bundle,
    CommandBufferType.Compute => CommandListType.Compute,
    CommandBufferType.Copy => CommandListType.Copy,
    _ => throw new ArgumentException($"Unsupported command buffer type: {_type}")
  };
}
