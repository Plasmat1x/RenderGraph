namespace GraphicsAPI.Commands.enums;

/// <summary>
/// Типы команд
/// </summary>
public enum CommandType
{
  // Render Target Commands
  SetRenderTargets,

  // Viewport Commands
  SetViewport,
  SetViewports,
  SetScissorRect,
  SetScissorRects,

  // Clear Commands
  ClearRenderTarget,
  ClearDepthStencil,
  ClearUnorderedAccess,

  // Resource Transitions
  TransitionResource,
  TransitionResources,
  UAVBarrier,
  UAVBarriers,

  // Vertex/Index Buffers
  SetVertexBuffer,
  SetVertexBuffers,
  SetIndexBuffer,

  // Shaders
  SetShader,

  // Shader Resources
  SetShaderResource,
  SetShaderResources,
  SetUnorderedAccess,
  SetUnorderedAccesses,
  SetConstantBuffer,
  SetConstantBuffers,
  SetSampler,
  SetSamplers,

  // Render States
  SetRenderState,
  SetBlendState,
  SetDepthStencilState,
  SetRasterizerState,
  SetPrimitiveTopology,

  // Draw Commands
  Draw,
  DrawIndexed,
  DrawIndirect,
  DrawIndexedIndirect,

  // Compute Commands
  Dispatch,
  DispatchIndirect,

  // Copy Commands
  CopyTexture,
  CopyTextureRegion,
  CopyBuffer,
  CopyBufferRegion,
  ResolveTexture,

  // Query Commands
  BeginQuery,
  EndQuery,

  // Debug Commands
  PushDebugGroup,
  PopDebugGroup,
  InsertDebugMarker,
  BeginEvent
}
