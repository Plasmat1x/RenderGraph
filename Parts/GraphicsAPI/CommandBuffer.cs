using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace GraphicsAPI;

/// <summary>
/// Базовый класс command buffer с общей функциональностью
/// </summary>
public abstract class CommandBuffer: IDisposable
{
  public abstract bool IsRecording { get; protected set; }
  public abstract CommandBufferType Type { get; }
  public string Name { get; set; } = string.Empty;

  // Управление записью
  public abstract void Begin();
  public abstract void End();
  public abstract void Reset();

  // Render targets и viewport
  public abstract void SetRenderTargets(ITextureView[] colorTargets, ITextureView depthTarget);
  public abstract void SetRenderTarget(ITextureView colorTarget, ITextureView depthTarget = null);
  public abstract void SetViewport(Viewport viewport);
  public abstract void SetViewports(Viewport[] viewports);
  public abstract void SetScissorRect(Rectangle rect);
  public abstract void SetScissorRects(Rectangle[] rects);

  // Очистка
  public abstract void ClearRenderTarget(ITextureView target, Vector4 color);
  public abstract void ClearDepthStencil(ITextureView target, ClearFlags flags, float depth, byte stencil);
  public abstract void ClearUnorderedAccess(ITextureView target, Vector4 value);
  public abstract void ClearUnorderedAccess(IBufferView target, uint value);

  // Состояния ресурсов
  public abstract void TransitionResource(IResource resource, ResourceState newState);
  public abstract void TransitionResources(IResource[] resources, ResourceState[] newStates);
  public abstract void UAVBarrier(IResource resource);
  public abstract void UAVBarriers(IResource[] resources);

  // Привязка ресурсов
  public abstract void SetVertexBuffer(IBufferView buffer, uint slot = 0);
  public abstract void SetVertexBuffers(IBufferView[] buffers, uint startSlot = 0);
  public abstract void SetIndexBuffer(IBufferView buffer, IndexFormat format);

  // Шейдеры
  public abstract void SetVertexShader(IShader shader);
  public abstract void SetPixelShader(IShader shader);
  public abstract void SetComputeShader(IShader shader);
  public abstract void SetGeometryShader(IShader shader);
  public abstract void SetHullShader(IShader shader);
  public abstract void SetDomainShader(IShader shader);

  // Shader resources
  public abstract void SetShaderResource(ShaderStage stage, uint slot, ITextureView resource);
  public abstract void SetShaderResources(ShaderStage stage, uint startSlot, ITextureView[] resources);
  public abstract void SetUnorderedAccess(ShaderStage stage, uint slot, ITextureView resource);
  public abstract void SetUnorderedAccesses(ShaderStage stage, uint startSlot, ITextureView[] resources);
  public abstract void SetConstantBuffer(ShaderStage stage, uint slot, IBufferView buffer);
  public abstract void SetConstantBuffers(ShaderStage stage, uint startSlot, IBufferView[] buffers);
  public abstract void SetSampler(ShaderStage stage, uint slot, ISampler sampler);
  public abstract void SetSamplers(ShaderStage stage, uint startSlot, ISampler[] samplers);

  // Рендер состояния
  public abstract void SetRenderState(IRenderState renderState);
  public abstract void SetBlendState(IBlendState blendState, Vector4 blendFactor, uint sampleMask = 0xffffffff);
  public abstract void SetDepthStencilState(IDepthStencilState depthStencilState, uint stencilRef = 0);
  public abstract void SetRasterizerState(IRasterizerState rasterizerState);

  // Draw calls
  public abstract void Draw(uint vertexCount, uint instanceCount = 1, uint startVertex = 0, uint startInstance = 0);
  public abstract void DrawIndexed(uint indexCount, uint instanceCount = 1, uint startIndex = 0, int baseVertex = 0, uint startInstance = 0);
  public abstract void DrawIndirect(IBufferView argsBuffer, ulong offset = 0);
  public abstract void DrawIndexedIndirect(IBufferView argsBuffer, ulong offset = 0);

  // Compute
  public abstract void Dispatch(uint groupCountX, uint groupCountY = 1, uint groupCountZ = 1);
  public abstract void DispatchIndirect(IBufferView argsBuffer, ulong offset = 0);

  // Копирование
  public abstract void CopyTexture(ITexture src, ITexture dst);
  public abstract void CopyTextureRegion(ITexture src, uint srcMip, uint srcArray, Box srcBox, ITexture dst, uint dstMip, uint dstArray, Point3D dstOffset);
  public abstract void CopyBuffer(IBuffer src, IBuffer dst);
  public abstract void CopyBufferRegion(IBuffer src, ulong srcOffset, IBuffer dst, ulong dstOffset, ulong size);

  // Resolve (для multisampling)
  public abstract void ResolveTexture(ITexture src, uint srcArray, ITexture dst, uint dstArray, TextureFormat format);

  // Queries
  public abstract void BeginQuery(IQuery query);
  public abstract void EndQuery(IQuery query);

  // Debug
  public abstract void PushDebugGroup(string name);
  public abstract void PopDebugGroup();
  public abstract void InsertDebugMarker(string name);

  // Утилиты
  public void DrawFullscreenQuad()
  {
    Draw(3, 1);
  }

  public void SetRenderTarget(ITexture colorTarget, ITexture depthTarget = null)
  {
    var colorView = colorTarget?.GetDefaultRenderTargetView();
    var depthView = depthTarget?.GetDefaultDepthStencilView();
    SetRenderTarget(colorView, depthView);
  }

  public void SetShaderResource(ShaderStage stage, uint slot, ITexture texture)
  {
    var view = texture?.GetDefaultShaderResourceView();
    SetShaderResource(stage, slot, view);
  }

  public void SetUnorderedAccess(ShaderStage stage, uint slot, ITexture texture)
  {
    var view = texture?.GetDefaultUnorderedAccessView();
    SetUnorderedAccess(stage, slot, view);
  }

  public void SetConstantBuffer(ShaderStage stage, uint slot, IBuffer buffer)
  {
    var view = buffer?.GetDefaultShaderResourceView();
    SetConstantBuffer(stage, slot, view);
  }

  public abstract void Dispose();
}
