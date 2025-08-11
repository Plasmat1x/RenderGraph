using GraphicsAPI.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class ResourceStateExtensions
{
  public static ResourceStates Convert(this ResourceState _state) => _state switch
  {
    ResourceState.Common => ResourceStates.Common,
    ResourceState.RenderTarget => ResourceStates.RenderTarget,
    ResourceState.UnorderedAccess => ResourceStates.UnorderedAccess,
    ResourceState.DepthWrite => ResourceStates.DepthWrite,
    ResourceState.DepthRead => ResourceStates.DepthRead,
    ResourceState.ShaderResource => ResourceStates.PixelShaderResource | ResourceStates.NonPixelShaderResource,
    ResourceState.StreamOut => ResourceStates.StreamOut,
    ResourceState.IndirectArgument => ResourceStates.IndirectArgument,
    ResourceState.CopyDest => ResourceStates.CopyDest,
    ResourceState.CopySource => ResourceStates.CopySource,
    ResourceState.ResolveDest => ResourceStates.ResolveDest,
    ResourceState.ResolveSource => ResourceStates.ResolveSource,
    ResourceState.Present => ResourceStates.Present,
    _ => throw new ArgumentException($"Unsupported resource state: {_state}")
  };
}
