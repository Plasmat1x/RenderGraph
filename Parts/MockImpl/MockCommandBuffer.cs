using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace MockImpl;

public class MockCommandBuffer: CommandBuffer
{
  public override bool IsRecording { get; protected set; }
  public override CommandBufferType Type { get; }
  public int CommandCount { get; private set; }

  public MockCommandBuffer(CommandBufferType type)
  {
    Type = type;
  }

  public override void Begin()
  {
    IsRecording = true;
    CommandCount = 0;
    Console.WriteLine($"    [CMD] Begin command buffer ({Type})");
  }

  public override void End()
  {
    IsRecording = false;
    Console.WriteLine($"    [CMD] End command buffer ({CommandCount} commands)");
  }

  public override void Reset()
  {
    CommandCount = 0;
    IsRecording = false;
    Console.WriteLine($"    [CMD] Reset command buffer");
  }

  public override void SetRenderTargets(ITextureView[] colorTargets, ITextureView depthTarget)
  {
    CommandCount++;
    var colorCount = colorTargets?.Length ?? 0;
    var hasDepth = depthTarget != null;
    Console.WriteLine($"    [CMD] SetRenderTargets(Colors: {colorCount}, Depth: {hasDepth})");
  }

  public override void SetRenderTarget(ITextureView colorTarget, ITextureView depthTarget = null)
  {
    SetRenderTargets(colorTarget != null ? new[] { colorTarget } : null, depthTarget);
  }

  public override void SetViewport(Viewport viewport)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetViewport({viewport.Width}x{viewport.Height} at {viewport.X},{viewport.Y})");
  }

  public override void SetViewports(Viewport[] viewports)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetViewports({viewports.Length} viewports)");
  }

  public override void SetScissorRect(Rectangle rect)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetScissorRect({rect.Width}x{rect.Height} at {rect.X},{rect.Y})");
  }

  public override void SetScissorRects(Rectangle[] rects)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetScissorRects({rects.Length} rects)");
  }

  public override void ClearRenderTarget(ITextureView target, Vector4 color)
  {
    CommandCount++;
    var textureName = (target as MockTextureView)?.Texture.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] ClearRenderTarget({textureName}) -> RGBA({color.X:F2}, {color.Y:F2}, {color.Z:F2}, {color.W:F2})");
  }

  public override void ClearDepthStencil(ITextureView target, ClearFlags flags, float depth, byte stencil)
  {
    CommandCount++;
    var textureName = (target as MockTextureView)?.Texture.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] ClearDepthStencil({textureName}, {flags}) -> Depth: {depth:F2}, Stencil: {stencil}");
  }

  public override void ClearUnorderedAccess(ITextureView target, Vector4 value)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] ClearUnorderedAccess(Texture) -> {value}");
  }

  public override void ClearUnorderedAccess(IBufferView target, uint value)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] ClearUnorderedAccess(Buffer) -> {value}");
  }

  public override void TransitionResource(IResource resource, ResourceState newState)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] TransitionResource({resource.Name}) -> {newState}");
  }

  public override void TransitionResources(IResource[] resources, ResourceState[] newStates)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] TransitionResources({resources.Length} resources)");
  }

  public override void UAVBarrier(IResource resource)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] UAVBarrier({resource.Name})");
  }

  public override void UAVBarriers(IResource[] resources)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] UAVBarriers({resources.Length} resources)");
  }

  public override void SetVertexBuffer(IBufferView buffer, uint slot = 0)
  {
    CommandCount++;
    var bufferName = (buffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] SetVertexBuffer({bufferName}, slot: {slot})");
  }

  public override void SetVertexBuffers(IBufferView[] buffers, uint startSlot = 0)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetVertexBuffers({buffers.Length} buffers, start slot: {startSlot})");
  }

  public override void SetIndexBuffer(IBufferView buffer, IndexFormat format)
  {
    CommandCount++;
    var bufferName = (buffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] SetIndexBuffer({bufferName}, {format})");
  }

  // Shader methods
  public override void SetVertexShader(IShader shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetVertexShader({shader?.Name ?? "null"})");
  }

  public override void SetPixelShader(IShader shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetPixelShader({shader?.Name ?? "null"})");
  }

  public override void SetComputeShader(IShader shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetComputeShader({shader?.Name ?? "null"})");
  }

  public override void SetGeometryShader(IShader shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetGeometryShader({shader?.Name ?? "null"})");
  }

  public override void SetHullShader(IShader shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetHullShader({shader?.Name ?? "null"})");
  }

  public override void SetDomainShader(IShader shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetDomainShader({shader?.Name ?? "null"})");
  }

  // Resource binding methods
  public override void SetShaderResource(ShaderStage stage, uint slot, ITextureView resource)
  {
    CommandCount++;
    var textureName = (resource as MockTextureView)?.Texture.Name ?? "null";
    Console.WriteLine($"    [CMD] SetShaderResource({stage}, slot: {slot}, {textureName})");
  }

  public override void SetShaderResources(ShaderStage stage, uint startSlot, ITextureView[] resources)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetShaderResources({stage}, {resources.Length} resources, start slot: {startSlot})");
  }

  public override void SetUnorderedAccess(ShaderStage stage, uint slot, ITextureView resource)
  {
    CommandCount++;
    var textureName = (resource as MockTextureView)?.Texture.Name ?? "null";
    Console.WriteLine($"    [CMD] SetUnorderedAccess({stage}, slot: {slot}, {textureName})");
  }

  public override void SetUnorderedAccesses(ShaderStage stage, uint startSlot, ITextureView[] resources)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetUnorderedAccesses({stage}, {resources.Length} resources, start slot: {startSlot})");
  }

  public override void SetConstantBuffer(ShaderStage stage, uint slot, IBufferView buffer)
  {
    CommandCount++;
    var bufferName = (buffer as MockBufferView)?.Buffer.Name ?? "null";
    Console.WriteLine($"    [CMD] SetConstantBuffer({stage}, slot: {slot}, {bufferName})");
  }

  public override void SetConstantBuffers(ShaderStage stage, uint startSlot, IBufferView[] buffers)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetConstantBuffers({stage}, {buffers.Length} buffers, start slot: {startSlot})");
  }

  public override void SetSampler(ShaderStage stage, uint slot, ISampler sampler)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetSampler({stage}, slot: {slot}, {sampler?.Name ?? "null"})");
  }

  public override void SetSamplers(ShaderStage stage, uint startSlot, ISampler[] samplers)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetSamplers({stage}, {samplers.Length} samplers, start slot: {startSlot})");
  }

  // Render state methods
  public override void SetRenderState(IRenderState renderState)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetRenderState({renderState?.Name ?? "null"})");
  }

  public override void SetBlendState(IBlendState blendState, Vector4 blendFactor, uint sampleMask = 0xffffffff)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetBlendState(factor: {blendFactor}, mask: 0x{sampleMask:X8})");
  }

  public override void SetDepthStencilState(IDepthStencilState depthStencilState, uint stencilRef = 0)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetDepthStencilState(stencil ref: {stencilRef})");
  }

  public override void SetRasterizerState(IRasterizerState rasterizerState)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetRasterizerState");
  }

  // Draw methods
  public override void Draw(uint vertexCount, uint instanceCount = 1, uint startVertex = 0, uint startInstance = 0)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] Draw(vertices: {vertexCount}, instances: {instanceCount}, start vertex: {startVertex}, start instance: {startInstance})");
  }

  public override void DrawIndexed(uint indexCount, uint instanceCount = 1, uint startIndex = 0, int baseVertex = 0, uint startInstance = 0)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] DrawIndexed(indices: {indexCount}, instances: {instanceCount}, start index: {startIndex}, base vertex: {baseVertex}, start instance: {startInstance})");
  }

  public override void DrawIndirect(IBufferView argsBuffer, ulong offset = 0)
  {
    CommandCount++;
    var bufferName = (argsBuffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] DrawIndirect({bufferName}, offset: {offset})");
  }

  public override void DrawIndexedIndirect(IBufferView argsBuffer, ulong offset = 0)
  {
    CommandCount++;
    var bufferName = (argsBuffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] DrawIndexedIndirect({bufferName}, offset: {offset})");
  }

  // Compute methods
  public override void Dispatch(uint groupCountX, uint groupCountY = 1, uint groupCountZ = 1)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] Dispatch({groupCountX}, {groupCountY}, {groupCountZ})");
  }

  public override void DispatchIndirect(IBufferView argsBuffer, ulong offset = 0)
  {
    CommandCount++;
    var bufferName = (argsBuffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] DispatchIndirect({bufferName}, offset: {offset})");
  }

  // Copy methods
  public override void CopyTexture(ITexture src, ITexture dst)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] CopyTexture({src.Name} -> {dst.Name})");
  }

  public override void CopyTextureRegion(ITexture src, uint srcMip, uint srcArray, Box srcBox, ITexture dst, uint dstMip, uint dstArray, Point3D dstOffset)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] CopyTextureRegion({src.Name}[{srcMip},{srcArray}] -> {dst.Name}[{dstMip},{dstArray}])");
  }

  public override void CopyBuffer(IBuffer src, IBuffer dst)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] CopyBuffer({src.Name} -> {dst.Name})");
  }

  public override void CopyBufferRegion(IBuffer src, ulong srcOffset, IBuffer dst, ulong dstOffset, ulong size)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] CopyBufferRegion({src.Name}[{srcOffset}] -> {dst.Name}[{dstOffset}], size: {size})");
  }

  public override void ResolveTexture(ITexture src, uint srcArray, ITexture dst, uint dstArray, TextureFormat format)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] ResolveTexture({src.Name}[{srcArray}] -> {dst.Name}[{dstArray}], {format})");
  }

  // Query methods
  public override void BeginQuery(IQuery query)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] BeginQuery({query.Type})");
  }

  public override void EndQuery(IQuery query)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] EndQuery({query.Type})");
  }

  // Debug methods
  public override void PushDebugGroup(string name)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] PushDebugGroup({name})");
  }

  public override void PopDebugGroup()
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] PopDebugGroup");
  }

  public override void InsertDebugMarker(string name)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] InsertDebugMarker({name})");
  }

  public override void Dispose()
  {
    Console.WriteLine($"    [CMD] Command buffer disposed ({CommandCount} total commands)");
  }
}