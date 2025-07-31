using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace MockImpl;

public class MockCommandBuffer: CommandBuffer
{
  public MockCommandBuffer(CommandBufferType _type)
  {
    Type = _type;
  }

  public override bool IsRecording { get; protected set; }
  public override CommandBufferType Type { get; }
  public int CommandCount { get; private set; }


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

  public override void SetRenderTargets(ITextureView[] _colorTargets, ITextureView _depthTarget)
  {
    CommandCount++;
    var colorCount = _colorTargets?.Length ?? 0;
    var hasDepth = _depthTarget != null;
    Console.WriteLine($"    [CMD] SetRenderTargets(Colors: {colorCount}, Depth: {hasDepth})");
  }

  public override void SetRenderTarget(ITextureView _colorTarget, ITextureView _depthTarget = null)
  {
    SetRenderTargets(_colorTarget != null ? new[] { _colorTarget } : null, _depthTarget);
  }

  public override void SetViewport(Viewport _viewport)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetViewport({_viewport.Width}x{_viewport.Height} at {_viewport.X},{_viewport.Y})");
  }

  public override void SetViewports(Viewport[] _viewports)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetViewports({_viewports.Length} viewports)");
  }

  public override void SetScissorRect(Rectangle _rect)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetScissorRect({_rect.Width}x{_rect.Height} at {_rect.X},{_rect.Y})");
  }

  public override void SetScissorRects(Rectangle[] _rects)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetScissorRects({_rects.Length} rects)");
  }

  public override void ClearRenderTarget(ITextureView _target, Vector4 _color)
  {
    CommandCount++;
    var textureName = (_target as MockTextureView)?.Texture.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] ClearRenderTarget({textureName}) -> RGBA({_color.X:F2}, {_color.Y:F2}, {_color.Z:F2}, {_color.W:F2})");
  }

  public override void ClearDepthStencil(ITextureView _target, ClearFlags _flags, float _depth, byte _stencil)
  {
    CommandCount++;
    var textureName = (_target as MockTextureView)?.Texture.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] ClearDepthStencil({textureName}, {_flags}) -> Depth: {_depth:F2}, Stencil: {_stencil}");
  }

  public override void ClearUnorderedAccess(ITextureView _target, Vector4 _value)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] ClearUnorderedAccess(Texture) -> {_value}");
  }

  public override void ClearUnorderedAccess(IBufferView _target, uint _value)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] ClearUnorderedAccess(Buffer) -> {_value}");
  }

  public override void TransitionResource(IResource _resource, ResourceState _newState)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] TransitionResource({_resource.Name}) -> {_newState}");
  }

  public override void TransitionResources(IResource[] _resources, ResourceState[] _newStates)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] TransitionResources({_resources.Length} resources)");
  }

  public override void UAVBarrier(IResource _resource)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] UAVBarrier({_resource.Name})");
  }

  public override void UAVBarriers(IResource[] _resources)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] UAVBarriers({_resources.Length} resources)");
  }

  public override void SetVertexBuffer(IBufferView _buffer, uint _slot = 0)
  {
    CommandCount++;
    var bufferName = (_buffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] SetVertexBuffer({bufferName}, slot: {_slot})");
  }

  public override void SetVertexBuffers(IBufferView[] _buffers, uint _startSlot = 0)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetVertexBuffers({_buffers.Length} buffers, start slot: {_startSlot})");
  }

  public override void SetIndexBuffer(IBufferView _buffer, IndexFormat _format)
  {
    CommandCount++;
    var bufferName = (_buffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] SetIndexBuffer({bufferName}, {_format})");
  }

  public override void SetVertexShader(IShader _shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetVertexShader({_shader?.Name ?? "null"})");
  }

  public override void SetPixelShader(IShader _shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetPixelShader({_shader?.Name ?? "null"})");
  }

  public override void SetComputeShader(IShader _shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetComputeShader({_shader?.Name ?? "null"})");
  }

  public override void SetGeometryShader(IShader _shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetGeometryShader({_shader?.Name ?? "null"})");
  }

  public override void SetHullShader(IShader _shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetHullShader({_shader?.Name ?? "null"})");
  }

  public override void SetDomainShader(IShader _shader)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetDomainShader({_shader?.Name ?? "null"})");
  }

  public override void SetShaderResource(ShaderStage _stage, uint _slot, ITextureView _resource)
  {
    CommandCount++;
    var textureName = (_resource as MockTextureView)?.Texture.Name ?? "null";
    Console.WriteLine($"    [CMD] SetShaderResource({_stage}, slot: {_slot}, {textureName})");
  }

  public override void SetShaderResources(ShaderStage _stage, uint _startSlot, ITextureView[] _resources)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetShaderResources({_stage}, {_resources.Length} resources, start slot: {_startSlot})");
  }

  public override void SetUnorderedAccess(ShaderStage _stage, uint _slot, ITextureView _resource)
  {
    CommandCount++;
    var textureName = (_resource as MockTextureView)?.Texture.Name ?? "null";
    Console.WriteLine($"    [CMD] SetUnorderedAccess({_stage}, slot: {_slot}, {textureName})");
  }

  public override void SetUnorderedAccesses(ShaderStage _stage, uint _startSlot, ITextureView[] _resources)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetUnorderedAccesses({_stage}, {_resources.Length} resources, start slot: {_startSlot})");
  }

  public override void SetConstantBuffer(ShaderStage _stage, uint _slot, IBufferView _buffer)
  {
    CommandCount++;
    var bufferName = (_buffer as MockBufferView)?.Buffer.Name ?? "null";
    Console.WriteLine($"    [CMD] SetConstantBuffer({_stage}, slot: {_slot}, {bufferName})");
  }

  public override void SetConstantBuffers(ShaderStage _stage, uint _startSlot, IBufferView[] _buffers)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetConstantBuffers({_stage}, {_buffers.Length} buffers, start slot: {_startSlot})");
  }

  public override void SetSampler(ShaderStage _stage, uint _slot, ISampler _sampler)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetSampler({_stage}, slot: {_slot}, {_sampler?.Name ?? "null"})");
  }

  public override void SetSamplers(ShaderStage _stage, uint _startSlot, ISampler[] _samplers)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetSamplers({_stage}, {_samplers.Length} samplers, start slot: {_startSlot})");
  }

  public override void SetRenderState(IRenderState _renderState)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetRenderState({_renderState?.Name ?? "null"})");
  }

  public override void SetBlendState(IBlendState _blendState, Vector4 _blendFactor, uint _sampleMask = 0xffffffff)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetBlendState(factor: {_blendFactor}, mask: 0x{_sampleMask:X8})");
  }

  public override void SetDepthStencilState(IDepthStencilState _depthStencilState, uint _stencilRef = 0)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetDepthStencilState(stencil ref: {_stencilRef})");
  }

  public override void SetRasterizerState(IRasterizerState _rasterizerState)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] SetRasterizerState");
  }

  public override void Draw(uint _vertexCount, uint _instanceCount = 1, uint _startVertex = 0, uint _startInstance = 0)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] Draw(vertices: {_vertexCount}, instances: {_instanceCount}, start vertex: {_startVertex}, start instance: {_startInstance})");
  }

  public override void DrawIndexed(uint _indexCount, uint _instanceCount = 1, uint _startIndex = 0, int _baseVertex = 0, uint _startInstance = 0)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] DrawIndexed(indices: {_indexCount}, instances: {_instanceCount}, start index: {_startIndex}, base vertex: {_baseVertex}, start instance: {_startInstance})");
  }

  public override void DrawIndirect(IBufferView _argsBuffer, ulong _offset = 0)
  {
    CommandCount++;
    var bufferName = (_argsBuffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] DrawIndirect({bufferName}, offset: {_offset})");
  }

  public override void DrawIndexedIndirect(IBufferView _argsBuffer, ulong _offset = 0)
  {
    CommandCount++;
    var bufferName = (_argsBuffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] DrawIndexedIndirect({bufferName}, offset: {_offset})");
  }

  public override void Dispatch(uint _groupCountX, uint _groupCountY = 1, uint _groupCountZ = 1)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] Dispatch({_groupCountX}, {_groupCountY}, {_groupCountZ})");
  }

  public override void DispatchIndirect(IBufferView _argsBuffer, ulong _offset = 0)
  {
    CommandCount++;
    var bufferName = (_argsBuffer as MockBufferView)?.Buffer.Name ?? "Unknown";
    Console.WriteLine($"    [CMD] DispatchIndirect({bufferName}, offset: {_offset})");
  }

  public override void CopyTexture(ITexture _src, ITexture _dst)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] CopyTexture({_src.Name} -> {_dst.Name})");
  }

  public override void CopyTextureRegion(ITexture _src, uint _srcMip, uint _srcArray, Box _srcBox, ITexture _dst, uint _dstMip, uint _dstArray, Point3D _dstOffset)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] CopyTextureRegion({_src.Name}[{_srcMip},{_srcArray}] -> {_dst.Name}[{_dstMip},{_dstArray}])");
  }

  public override void CopyBuffer(IBuffer _src, IBuffer _dst)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] CopyBuffer({_src.Name} -> {_dst.Name})");
  }

  public override void CopyBufferRegion(IBuffer _src, ulong _srcOffset, IBuffer _dst, ulong _dstOffset, ulong _size)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] CopyBufferRegion({_src.Name}[{_srcOffset}] -> {_dst.Name}[{_dstOffset}], size: {_size})");
  }

  public override void ResolveTexture(ITexture _src, uint _srcArray, ITexture _dst, uint _dstArray, TextureFormat _format)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] ResolveTexture({_src.Name}[{_srcArray}] -> {_dst.Name}[{_dstArray}], {_format})");
  }

  public override void BeginQuery(IQuery _query)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] BeginQuery({_query.Type})");
  }

  public override void EndQuery(IQuery _query)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] EndQuery({_query.Type})");
  }

  public override void PushDebugGroup(string _name)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] PushDebugGroup({_name})");
  }

  public override void PopDebugGroup()
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] PopDebugGroup");
  }

  public override void InsertDebugMarker(string _name)
  {
    CommandCount++;
    Console.WriteLine($"    [CMD] InsertDebugMarker({_name})");
  }

  public override void SetPrimitiveTopology(PrimitiveTopology _topology)
  {
    Console.WriteLine($"    [CMD] Set primitive topology ({_topology})");
  }

  public override void Dispose()
  {
    Console.WriteLine($"    [CMD] Command buffer disposed ({CommandCount} total commands)");
  }
}