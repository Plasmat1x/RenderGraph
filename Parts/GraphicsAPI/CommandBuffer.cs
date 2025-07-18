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

  public abstract void Begin();
  public abstract void End();
  public abstract void Reset();
  public abstract void SetRenderTargets(ITextureView[] _colorTargets, ITextureView _depthTarget);
  public abstract void SetRenderTarget(ITextureView _colorTarget, ITextureView _depthTarget = null);
  public abstract void SetViewport(Viewport _viewport);
  public abstract void SetViewports(Viewport[] _viewports);
  public abstract void SetScissorRect(Rectangle _rect);
  public abstract void SetScissorRects(Rectangle[] _rects);
  public abstract void ClearRenderTarget(ITextureView _target, Vector4 _color);
  public abstract void ClearDepthStencil(ITextureView _target, ClearFlags _flags, float _depth, byte _stencil);
  public abstract void ClearUnorderedAccess(ITextureView _target, Vector4 _value);
  public abstract void ClearUnorderedAccess(IBufferView _target, uint _value);
  public abstract void TransitionResource(IResource _resource, ResourceState _newState);
  public abstract void TransitionResources(IResource[] _resources, ResourceState[] _newStates);
  public abstract void UAVBarrier(IResource _resource);
  public abstract void UAVBarriers(IResource[] _resources);
  public abstract void SetVertexBuffer(IBufferView _buffer, uint _slot = 0);
  public abstract void SetVertexBuffers(IBufferView[] _buffers, uint _startSlot = 0);
  public abstract void SetIndexBuffer(IBufferView _buffer, IndexFormat _format);
  public abstract void SetVertexShader(IShader _shader);
  public abstract void SetPixelShader(IShader _shader);
  public abstract void SetComputeShader(IShader _shader);
  public abstract void SetGeometryShader(IShader _shader);
  public abstract void SetHullShader(IShader _shader);
  public abstract void SetDomainShader(IShader _shader);
  public abstract void SetShaderResource(ShaderStage _stage, uint _slot, ITextureView _resource);
  public abstract void SetShaderResources(ShaderStage _stage, uint _startSlot, ITextureView[] _resources);
  public abstract void SetUnorderedAccess(ShaderStage _stage, uint _slot, ITextureView _resource);
  public abstract void SetUnorderedAccesses(ShaderStage _stage, uint _startSlot, ITextureView[] _resources);
  public abstract void SetConstantBuffer(ShaderStage _stage, uint _slot, IBufferView _buffer);
  public abstract void SetConstantBuffers(ShaderStage _stage, uint _startSlot, IBufferView[] _buffers);
  public abstract void SetSampler(ShaderStage _stage, uint _slot, ISampler _sampler);
  public abstract void SetSamplers(ShaderStage _stage, uint _startSlot, ISampler[] _samplers);
  public abstract void SetRenderState(IRenderState _renderState);
  public abstract void SetPrimitiveTopology(PrimitiveTopology _topology);
  public abstract void SetBlendState(IBlendState _blendState, Vector4 _blendFactor, uint _sampleMask = 0xffffffff);
  public abstract void SetDepthStencilState(IDepthStencilState _depthStencilState, uint _stencilRef = 0);
  public abstract void SetRasterizerState(IRasterizerState _rasterizerState);
  public abstract void Draw(uint _vertexCount, uint _instanceCount = 1, uint _startVertex = 0, uint _startInstance = 0);
  public abstract void DrawIndexed(uint _indexCount, uint _instanceCount = 1, uint _startIndex = 0, int _baseVertex = 0, uint _startInstance = 0);
  public abstract void DrawIndirect(IBufferView _argsBuffer, ulong _offset = 0);
  public abstract void DrawIndexedIndirect(IBufferView _argsBuffer, ulong _offset = 0);
  public abstract void Dispatch(uint _groupCountX, uint _groupCountY = 1, uint _groupCountZ = 1);
  public abstract void DispatchIndirect(IBufferView _argsBuffer, ulong _offset = 0);
  public abstract void CopyTexture(ITexture _src, ITexture _dst);
  public abstract void CopyTextureRegion(ITexture _src, uint _srcMip, uint _srcArray, Box _srcBox, ITexture _dst, uint _dstMip, uint _dstArray, Point3D _dstOffset);
  public abstract void CopyBuffer(IBuffer _src, IBuffer _dst);
  public abstract void CopyBufferRegion(IBuffer _src, ulong _srcOffset, IBuffer _dst, ulong _dstOffset, ulong _size);
  public abstract void ResolveTexture(ITexture _src, uint _srcArray, ITexture _dst, uint _dstArray, TextureFormat _format);
  public abstract void BeginQuery(IQuery _query);
  public abstract void EndQuery(IQuery _query);
  public abstract void PushDebugGroup(string _name);
  public abstract void PopDebugGroup();
  public abstract void InsertDebugMarker(string _name);
  public void DrawFullscreenQuad()
  {
    SetPrimitiveTopology(PrimitiveTopology.TriangleList);
    Draw(3, 1, 0, 0);
  }

  public void SetRenderTarget(ITexture _colorTarget, ITexture _depthTarget = null)
  {
    var colorView = _colorTarget?.GetDefaultRenderTargetView();
    var depthView = _depthTarget?.GetDefaultDepthStencilView();
    SetRenderTarget(colorView, depthView);
  }

  public void SetShaderResource(ShaderStage _stage, uint _slot, ITexture _texture)
  {
    var view = _texture?.GetDefaultShaderResourceView();
    SetShaderResource(_stage, _slot, view);
  }

  public void SetUnorderedAccess(ShaderStage _stage, uint _slot, ITexture _texture)
  {
    var view = _texture?.GetDefaultUnorderedAccessView();
    SetUnorderedAccess(_stage, _slot, view);
  }

  public void SetConstantBuffer(ShaderStage _stage, uint _slot, IBuffer _buffer)
  {
    var view = _buffer?.GetDefaultShaderResourceView();
    SetConstantBuffer(_stage, _slot, view);
  }

  public abstract void Dispose();
}
