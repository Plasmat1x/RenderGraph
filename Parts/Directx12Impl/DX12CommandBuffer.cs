using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System.Numerics;

namespace Directx12Impl;
public class DX12CommandBuffer: CommandBuffer
{
  private ComPtr<ID3D12GraphicsCommandList> p_commandList;
  private ComPtr<ID3D12CommandAllocator> p_commandAllocator;
  private ComPtr<ID3D12Device> p_device;
  private CommandBufferType p_type;
  private ComPtr<ID3D12PipelineState> p_currentPipelineState;
  private ComPtr<ID3D12RootSignature> p_currentRootSignature;
  private List<ResourceBarrier> p_resourceBarriers;
  private CpuDescriptorHandle[] p_currentRenderTargets;
  private CpuDescriptorHandle p_currentDepthStencil;
  private bool p_isDirty;

  public DX12CommandBuffer() { }

  public override bool IsRecording { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

  public override CommandBufferType Type => throw new NotImplementedException();

  public override void Begin()
  {
    throw new NotImplementedException();
  }

  public override void BeginQuery(IQuery _query)
  {
    throw new NotImplementedException();
  }

  public override void ClearDepthStencil(ITextureView _target, GraphicsAPI.Enums.ClearFlags _flags, float _depth, byte _stencil)
  {
    throw new NotImplementedException();
  }

  public override void ClearRenderTarget(ITextureView _target, Vector4 _color)
  {
    throw new NotImplementedException();
  }

  public override void ClearUnorderedAccess(ITextureView _target, Vector4 _value)
  {
    throw new NotImplementedException();
  }

  public override void ClearUnorderedAccess(IBufferView _target, uint _value)
  {
    throw new NotImplementedException();
  }

  public override void CopyBuffer(IBuffer _src, IBuffer _dst)
  {
    throw new NotImplementedException();
  }

  public override void CopyBufferRegion(IBuffer _src, ulong _srcOffset, IBuffer _dst, ulong _dstOffset, ulong _size)
  {
    throw new NotImplementedException();
  }

  public override void CopyTexture(ITexture _src, ITexture _dst)
  {
    throw new NotImplementedException();
  }

  public override void CopyTextureRegion(ITexture _src, uint _srcMip, uint _srcArray, GraphicsAPI.Box _srcBox, ITexture _dst, uint _dstMip, uint _dstArray, Point3D _dstOffset)
  {
    throw new NotImplementedException();
  }

  public override void Dispatch(uint _groupCountX, uint _groupCountY = 1, uint _groupCountZ = 1)
  {
    throw new NotImplementedException();
  }

  public override void DispatchIndirect(IBufferView _argsBuffer, ulong _offset = 0)
  {
    throw new NotImplementedException();
  }

  public override void Draw(uint _vertexCount, uint _instanceCount = 1, uint _startVertex = 0, uint _startInstance = 0)
  {
    throw new NotImplementedException();
  }

  public override void DrawIndexed(uint _indexCount, uint _instanceCount = 1, uint _startIndex = 0, int _baseVertex = 0, uint _startInstance = 0)
  {
    throw new NotImplementedException();
  }

  public override void DrawIndexedIndirect(IBufferView _argsBuffer, ulong _offset = 0)
  {
    throw new NotImplementedException();
  }

  public override void DrawIndirect(IBufferView _argsBuffer, ulong _offset = 0)
  {
    throw new NotImplementedException();
  }

  public override void End()
  {
    throw new NotImplementedException();
  }

  public override void EndQuery(IQuery _query)
  {
    throw new NotImplementedException();
  }

  public override void InsertDebugMarker(string _name)
  {
    throw new NotImplementedException();
  }

  public override void PopDebugGroup()
  {
    throw new NotImplementedException();
  }

  public override void PushDebugGroup(string _name)
  {
    throw new NotImplementedException();
  }

  public override void Reset()
  {
    throw new NotImplementedException();
  }

  public override void ResolveTexture(ITexture _src, uint _srcArray, ITexture _dst, uint _dstArray, TextureFormat _format)
  {
    throw new NotImplementedException();
  }

  public override void SetBlendState(IBlendState _blendState, Vector4 _blendFactor, uint _sampleMask = uint.MaxValue)
  {
    throw new NotImplementedException();
  }

  public override void SetComputeShader(IShader _shader)
  {
    throw new NotImplementedException();
  }

  public override void SetConstantBuffer(ShaderStage _stage, uint _slot, IBufferView _buffer)
  {
    throw new NotImplementedException();
  }

  public override void SetConstantBuffers(ShaderStage _stage, uint _startSlot, IBufferView[] _buffers)
  {
    throw new NotImplementedException();
  }

  public override void SetDepthStencilState(IDepthStencilState _depthStencilState, uint _stencilRef = 0)
  {
    throw new NotImplementedException();
  }

  public override void SetDomainShader(IShader _shader)
  {
    throw new NotImplementedException();
  }

  public override void SetGeometryShader(IShader _shader)
  {
    throw new NotImplementedException();
  }

  public override void SetHullShader(IShader _shader)
  {
    throw new NotImplementedException();
  }

  public override void SetIndexBuffer(IBufferView _buffer, IndexFormat _format)
  {
    throw new NotImplementedException();
  }

  public override void SetPixelShader(IShader _shader)
  {
    throw new NotImplementedException();
  }

  public override void SetRasterizerState(IRasterizerState _rasterizerState)
  {
    throw new NotImplementedException();
  }

  public override void SetRenderState(IRenderState _renderState)
  {
    throw new NotImplementedException();
  }

  public override void SetRenderTarget(ITextureView _colorTarget, ITextureView _depthTarget = null)
  {
    throw new NotImplementedException();
  }

  public override void SetRenderTargets(ITextureView[] _colorTargets, ITextureView _depthTarget)
  {
    throw new NotImplementedException();
  }

  public override void SetSampler(ShaderStage _stage, uint _slot, ISampler _sampler)
  {
    throw new NotImplementedException();
  }

  public override void SetSamplers(ShaderStage _stage, uint _startSlot, ISampler[] _samplers)
  {
    throw new NotImplementedException();
  }

  public override void SetScissorRect(Rectangle _rect)
  {
    throw new NotImplementedException();
  }

  public override void SetScissorRects(Rectangle[] _rects)
  {
    throw new NotImplementedException();
  }

  public override void SetShaderResource(ShaderStage _stage, uint _slot, ITextureView _resource)
  {
    throw new NotImplementedException();
  }

  public override void SetShaderResources(ShaderStage _stage, uint _startSlot, ITextureView[] _resources)
  {
    throw new NotImplementedException();
  }

  public override void SetUnorderedAccess(ShaderStage _stage, uint _slot, ITextureView _resource)
  {
    throw new NotImplementedException();
  }

  public override void SetUnorderedAccesses(ShaderStage _stage, uint _startSlot, ITextureView[] _resources)
  {
    throw new NotImplementedException();
  }

  public override void SetVertexBuffer(IBufferView _buffer, uint _slot = 0)
  {
    throw new NotImplementedException();
  }

  public override void SetVertexBuffers(IBufferView[] _buffers, uint _startSlot = 0)
  {
    throw new NotImplementedException();
  }

  public override void SetVertexShader(IShader _shader)
  {
    throw new NotImplementedException();
  }

  public override void SetViewport(Resources.Viewport _viewport)
  {
    throw new NotImplementedException();
  }

  public override void SetViewports(Resources.Viewport[] _viewports)
  {
    throw new NotImplementedException();
  }

  public override void TransitionResource(IResource _resource, ResourceState _newState)
  {
    throw new NotImplementedException();
  }

  public override void TransitionResources(IResource[] _resources, ResourceState[] _newStates)
  {
    throw new NotImplementedException();
  }

  public override void UAVBarrier(IResource _resource)
  {
    throw new NotImplementedException();
  }

  public override void UAVBarriers(IResource[] _resources)
  {
    throw new NotImplementedException();
  }

  public override void Dispose()
  {
    throw new NotImplementedException();
  }

  private ResourceStates ConvertResourceState(ResourceState _state)
  {
    throw new NotImplementedException();
  }

  private CommandListType ConvertCommandListType(CommandBufferType _type)
  {
    throw new NotImplementedException();
  }

  private void FlushResourceBarriers()
  {
    throw new NotImplementedException();
  }

  private ID3D12Resource GetDX12Resource(IResource _resource)
  {
    throw new NotImplementedException();
  }
}
