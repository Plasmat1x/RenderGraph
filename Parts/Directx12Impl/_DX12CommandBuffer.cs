//using GraphicsAPI;
//using GraphicsAPI.Enums;
//using GraphicsAPI.Interfaces;

//using Resources;
//using Resources.Enums;

//using Silk.NET.Core.Native;
//using Silk.NET.Direct3D12;
//using Silk.NET.Maths;

//using System.Numerics;

//namespace Directx12Impl;
//public class _DX12CommandBuffer: CommandBuffer
//{
//  private readonly ComPtr<ID3D12Device> p_device;
//  private readonly D3D12 p_d3d12;
//  private readonly CommandListType p_d3d12Type;
//  private readonly CommandBufferType p_type;

//  private ComPtr<ID3D12GraphicsCommandList> p_commandList;
//  private ComPtr<ID3D12CommandAllocator> p_commandAllocator;

//  private ComPtr<ID3D12PipelineState> p_currentPipelineState;
//  private ComPtr<ID3D12RootSignature> p_currentRootSignature;

//  private ComPtr<ID3D12DescriptorHeap> p_currentSrvHeap;
//  private ComPtr<ID3D12DescriptorHeap> p_currentSamplerHeap;

//  private readonly DX12ResourceStateTracker p_stateTracker = new();

//  private readonly CpuDescriptorHandle[] p_currentRenderTargets;
//  private CpuDescriptorHandle? p_currentDepthStencil;
//  private uint p_renderTargetCount;

//  private bool p_disposed;

//  public _DX12CommandBuffer(
//    ComPtr<ID3D12Device> _device,
//    D3D12 _d3d12,
//    CommandBufferType _type,
//    string _name = null)
//  {
//    p_device = _device;
//    p_d3d12 = _d3d12;
//    p_type = _type;
//    Name = _name ?? $"CommandBuffer_{p_type}";

//    p_d3d12Type = DX12Helpers.ConvertCommandListType(_type);
//    p_currentRenderTargets = new CpuDescriptorHandle[8];

//    CreateCommandList();
//  }

//  public override bool IsRecording { get; protected set; }

//  public override CommandBufferType Type => p_type;

//  public override unsafe void Begin()
//  {
//    if(IsRecording)
//      throw new InvalidOperationException("Command buffer is already recording");

//    HResult hr = p_commandAllocator.Reset();
//    if(hr.IsFailure)
//      throw new InvalidOperationException($"Failed to reset command allocator: {hr}");

//    hr = p_commandList.Reset(p_commandAllocator, (ID3D12PipelineState*)IntPtr.Zero);
//    if(hr.IsFailure)
//      throw new InvalidOperationException($"Failed to reset command list: {hr}");

//    IsRecording = true;

//    p_stateTracker.Reset();
//    p_currentPipelineState = null;
//    p_currentRootSignature = null;
//    p_renderTargetCount = 0;
//    p_currentDepthStencil = null;
//  }

//  public override void BeginQuery(IQuery _query)
//  {
//    throw new NotImplementedException();
//  }

//  public override unsafe void ClearDepthStencil(ITextureView _target, GraphicsAPI.Enums.ClearFlags _flags, float _depth, byte _stencil)
//  {
//    if(_target is not _DX12TextureView view)
//      throw new ArgumentException("Invalid texture view type");

//    var handle = view.GetDescriptorHandle();
//    Silk.NET.Direct3D12.ClearFlags d3d12Flags = 0;

//    if((_flags & GraphicsAPI.Enums.ClearFlags.Depth) != 0)
//      d3d12Flags |= Silk.NET.Direct3D12.ClearFlags.Depth;
//    if((_flags & GraphicsAPI.Enums.ClearFlags.Stencil) != 0)
//      d3d12Flags |= Silk.NET.Direct3D12.ClearFlags.Stencil;

//    p_commandList.ClearDepthStencilView(handle, d3d12Flags, _depth, _stencil, 0, (Box2D<int>*)null);
//  }

//  public override unsafe void ClearRenderTarget(ITextureView _target, Vector4 _color)
//  {
//    if(_target is not _DX12TextureView view)
//      throw new ArgumentException("Invalid texture view type");

//    var handle = view.GetDescriptorHandle();
//    var colorArray = new[] { _color.X, _color.Y, _color.Z, _color.W };

//    fixed(float* pColor = colorArray)
//    {
//      p_commandList.ClearRenderTargetView(handle, pColor, 0, (Box2D<int>*)IntPtr.Zero);
//    }
//  }

//  public override void ClearUnorderedAccess(ITextureView _target, Vector4 _value)
//  {
//    throw new NotImplementedException();
//  }

//  public override void ClearUnorderedAccess(IBufferView _target, uint _value)
//  {
//    throw new NotImplementedException();
//  }

//  public override void CopyBuffer(IBuffer _src, IBuffer _dst)
//  {
//    if(_src is not _DX12Buffer srcBuffer || _dst is not _DX12Buffer dstBuffer)
//      throw new ArgumentException("Invalid buffer type");

//    p_stateTracker.FlushResourceBarriers(p_commandList);
//    p_commandList.CopyResource(dstBuffer.GetResource(), srcBuffer.GetResource());
//  }

//  public override void CopyBufferRegion(IBuffer _src, ulong _srcOffset, IBuffer _dst, ulong _dstOffset, ulong _size)
//  {
//    throw new NotImplementedException();
//  }

//  public override void CopyTexture(ITexture _src, ITexture _dst)
//  {
//    if(_src is not _DX12Texture srcTexture || _dst is not _DX12Texture dstTexture)
//      throw new ArgumentException("Invalid texture type");

//    p_stateTracker.FlushResourceBarriers(p_commandList);
//    p_commandList.CopyResource(dstTexture.GetResource(), srcTexture.GetResource());
//  }

//  public override void CopyTextureRegion(ITexture _src, uint _srcMip, uint _srcArray, GraphicsAPI.Box _srcBox, ITexture _dst, uint _dstMip, uint _dstArray, Point3D _dstOffset)
//  {
//    throw new NotImplementedException();
//  }

//  public override void Dispatch(uint _groupCountX, uint _groupCountY = 1, uint _groupCountZ = 1)
//  {
//    p_stateTracker.FlushResourceBarriers(p_commandList);
//    p_commandList.Dispatch(_groupCountX, _groupCountY, _groupCountZ);
//  }

//  public override void DispatchIndirect(IBufferView _argsBuffer, ulong _offset = 0)
//  {
//    throw new NotImplementedException();
//  }

//  public override void Draw(uint _vertexCount, uint _instanceCount = 1, uint _startVertex = 0, uint _startInstance = 0)
//  {
//    p_stateTracker.FlushResourceBarriers(p_commandList);
//    p_commandList.DrawInstanced(_vertexCount, _instanceCount, _startVertex, _startInstance);
//  }

//  public override void DrawIndexed(uint _indexCount, uint _instanceCount = 1, uint _startIndex = 0, int _baseVertex = 0, uint _startInstance = 0)
//  {
//    p_stateTracker.FlushResourceBarriers(p_commandList);
//    p_commandList.DrawIndexedInstanced(_indexCount, _instanceCount, _startIndex, _baseVertex, _startInstance);
//  }

//  public override void DrawIndexedIndirect(IBufferView _argsBuffer, ulong _offset = 0)
//  {
//    throw new NotImplementedException();
//  }

//  public override void DrawIndirect(IBufferView _argsBuffer, ulong _offset = 0)
//  {
//    throw new NotImplementedException();
//  }

//  public override void End()
//  {
//    if(!IsRecording)
//      throw new InvalidOperationException("Command buffer is not recording");

//    p_stateTracker.ResolvePendingResourceBarriers();
//    p_stateTracker.FlushResourceBarriers(p_commandList);
//    p_stateTracker.CommitFinalResourceStates();

//    HResult hr = p_commandList.Close();
//    if(hr.IsFailure)
//      throw new InvalidOperationException($"Failed to close command list: {hr}");

//    IsRecording = false;
//  }

//  public override void EndQuery(IQuery _query)
//  {
//    throw new NotImplementedException();
//  }

//  public override void InsertDebugMarker(string _name)
//  {
//    throw new NotImplementedException();
//  }

//  public override void PopDebugGroup()
//  {
//    throw new NotImplementedException();
//  }

//  public override void PushDebugGroup(string _name)
//  {
//    throw new NotImplementedException();
//  }

//  public override void Reset()
//  {
//    if(IsRecording)
//      End();
//  }

//  public override void ResolveTexture(ITexture _src, uint _srcArray, ITexture _dst, uint _dstArray, TextureFormat _format)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetBlendState(IBlendState _blendState, Vector4 _blendFactor, uint _sampleMask = uint.MaxValue)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetComputeShader(IShader _shader)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetConstantBuffer(ShaderStage _stage, uint _slot, IBufferView _buffer)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetConstantBuffers(ShaderStage _stage, uint _startSlot, IBufferView[] _buffers)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetDepthStencilState(IDepthStencilState _depthStencilState, uint _stencilRef = 0)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetDomainShader(IShader _shader)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetGeometryShader(IShader _shader)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetHullShader(IShader _shader)
//  {
//    throw new NotImplementedException();
//  }

//  public override unsafe void SetIndexBuffer(IBufferView _buffer, IndexFormat _format)
//  {
//    if(_buffer is not _DX12BufferView view)
//      throw new ArgumentException("Invalid buffer view type");

//    var ibView = view.GetIndexBufferView();
//    p_commandList.IASetIndexBuffer(&ibView);
//  }

//  public override void SetPixelShader(IShader _shader)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetRasterizerState(IRasterizerState _rasterizerState)
//  {
//    throw new NotImplementedException();
//  }

//  public override unsafe void SetRenderState(IRenderState _renderState)
//  {
//    if(_renderState is not DX12RenderState dx12RenderState)
//      throw new ArgumentException("Invalid render state type");

//    var pso = dx12RenderState.GetPipelineState();
//    var rootSignature = dx12RenderState.GetRootSignature();

//    p_commandList.SetPipelineState(pso);
//    p_currentPipelineState = pso;

//    if(p_currentRootSignature.Handle != rootSignature.Handle)
//    {
//      if(Type == CommandBufferType.Compute)
//        p_commandList.SetComputeRootSignature(rootSignature);
//      else
//        p_commandList.SetGraphicsRootSignature(rootSignature);

//      p_currentRootSignature = rootSignature;
//    }
//  }

//  public override void SetRenderTarget(ITextureView _colorTarget, ITextureView _depthTarget = null)
//  {
//    SetRenderTargets(
//    _colorTarget != null ? new[] { _colorTarget } : null,
//    _depthTarget);
//  }

//  public override unsafe void SetRenderTargets(ITextureView[] _colorTargets, ITextureView _depthTarget)
//  {
//    p_renderTargetCount = 0;

//    if(_colorTargets != null)
//    {
//      for(int i = 0; i < _colorTargets.Length && i < 8; i++)
//      {
//        if(_colorTargets[i] is _DX12TextureView view)
//        {
//          p_currentRenderTargets[i] = view.GetDescriptorHandle();
//          p_renderTargetCount++;
//        }
//      }
//    }

//    p_currentDepthStencil = null;
//    if(_depthTarget is _DX12TextureView depthView)
//    {
//      p_currentDepthStencil = depthView.GetDescriptorHandle();
//    }

//    CpuDescriptorHandle dsHandle = p_currentDepthStencil.HasValue ? p_currentDepthStencil.Value : default;
//    CpuDescriptorHandle rtHandle = p_currentRenderTargets[0];
//    p_commandList.QueryInterface<ID3D12GraphicsCommandList10>().OMSetRenderTargets(
//        p_renderTargetCount,
//        p_renderTargetCount > 0 ? &rtHandle : null,
//        false,
//        &dsHandle);
//  }

//  public override void SetSampler(ShaderStage _stage, uint _slot, ISampler _sampler)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetSamplers(ShaderStage _stage, uint _startSlot, ISampler[] _samplers)
//  {
//    throw new NotImplementedException();
//  }

//  public override unsafe void SetScissorRect(Resources.Rectangle _rect)
//  {
//    var d3d12Rect = new Silk.NET.Maths.Box2D<int>(
//    _rect.X,
//    _rect.Y,
//    _rect.Width,
//    _rect.Height);

//    p_commandList.QueryInterface<ID3D12GraphicsCommandList10>().RSSetScissorRects(1, &d3d12Rect);
//  }

//  public override void SetScissorRects(Resources.Rectangle[] _rects)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetShaderResource(ShaderStage _stage, uint _slot, ITextureView _resource)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetShaderResources(ShaderStage _stage, uint _startSlot, ITextureView[] _resources)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetUnorderedAccess(ShaderStage _stage, uint _slot, ITextureView _resource)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetUnorderedAccesses(ShaderStage _stage, uint _startSlot, ITextureView[] _resources)
//  {
//    throw new NotImplementedException();
//  }

//  public override unsafe void SetVertexBuffer(IBufferView _buffer, uint _slot = 0)
//  {
//    if(_buffer is not _DX12BufferView view)
//      throw new ArgumentException("Invalid buffer view type");

//    var vbView = view.GetVertexBufferView();
//    p_commandList.IASetVertexBuffers(_slot, 1, &vbView);
//  }

//  public override void SetVertexBuffers(IBufferView[] _buffers, uint _startSlot = 0)
//  {
//    throw new NotImplementedException();
//  }

//  public override void SetVertexShader(IShader _shader)
//  {
//    throw new NotImplementedException();
//  }

//  public override unsafe void SetViewport(Resources.Viewport _viewport)
//  {
//    var d3d12Viewport = new Silk.NET.Direct3D12.Viewport
//    {
//      TopLeftX = _viewport.X,
//      TopLeftY = _viewport.Y,
//      Width = _viewport.Width,
//      Height = _viewport.Height,
//      MinDepth = _viewport.MinDepth,
//      MaxDepth = _viewport.MaxDepth
//    };

//    p_commandList.RSSetViewports(1, &d3d12Viewport);
//  }

//  public override void SetPrimitiveTopology(PrimitiveTopology _topology)
//  {
//    var d3d12Topology = _topology switch
//    {
//      PrimitiveTopology.PointList => D3DPrimitiveTopology.D3DPrimitiveTopologyPointlist,
//      PrimitiveTopology.LineList => D3DPrimitiveTopology.D3DPrimitiveTopologyLinelist,
//      PrimitiveTopology.LineStrip => D3DPrimitiveTopology.D3DPrimitiveTopologyLinestrip,
//      PrimitiveTopology.TriangleList => D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist,
//      PrimitiveTopology.TriangleStrip => D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglestrip,
//      _ => D3DPrimitiveTopology.D3DPrimitiveTopologyUndefined
//    };

//    p_commandList.IASetPrimitiveTopology(d3d12Topology);
//  }

//  public override void SetViewports(Resources.Viewport[] _viewports)
//  {
//    throw new NotImplementedException();
//  }

//  public override unsafe void TransitionResource(IResource _resource, ResourceState _newState)
//  {
//    ID3D12Resource* d3d12Resource = null;

//    switch(_resource)
//    {
//      case _DX12Texture texture:
//        d3d12Resource = texture.GetResource();
//        break;
//      case _DX12Buffer buffer:
//        d3d12Resource = buffer.GetResource();
//        break;
//      default:
//        throw new ArgumentException("Invalid resource type");
//    }

//    var targetState = ConvertResourceState(_newState);

//    p_stateTracker.TransitionResource(d3d12Resource, targetState);

//    switch(_resource)
//    {
//      case _DX12Texture texture:
//        texture.SetCurrentState(targetState);
//        break;
//      case _DX12Buffer buffer:
//        buffer.SetCurrentState(targetState);
//        break;
//    }
//  }

//  public override void TransitionResources(IResource[] _resources, ResourceState[] _newStates)
//  {
//    throw new NotImplementedException();
//  }

//  public override void UAVBarrier(IResource _resource)
//  {
//    throw new NotImplementedException();
//  }

//  public override void UAVBarriers(IResource[] _resources)
//  {
//    throw new NotImplementedException();
//  }

//  public unsafe ID3D12GraphicsCommandList* GetCommandList() => p_commandList;

//  public override void Dispose()
//  {
//    p_commandList.Dispose();
//    p_commandAllocator.Dispose();
//  }

//  private ResourceStates ConvertResourceState(ResourceState _state)
//  {
//    return DX12Helpers.ConvertResourceState(_state);
//  }

//  private CommandListType ConvertCommandListType(CommandBufferType _type)
//  {
//    throw new NotImplementedException();
//  }

//  private ID3D12Resource GetDX12Resource(IResource _resource)
//  {
//    throw new NotImplementedException();
//  }

//  private unsafe void CreateCommandList()
//  {
//    ID3D12CommandAllocator* allocator;
//    HResult hr = p_device.CreateCommandAllocator(
//        p_d3d12Type,
//        SilkMarshal.GuidPtrOf<ID3D12CommandAllocator>(),
//        (void**)&allocator);

//    if(hr.IsFailure)
//      throw new InvalidOperationException($"Failed to create command allocator: {hr}");

//    p_commandAllocator = allocator;

//    ComPtr<ID3D12PipelineState> pso = default;
//    ID3D12GraphicsCommandList* commandList;
//    hr = p_device.CreateCommandList(
//        0,
//        p_d3d12Type,
//        p_commandAllocator,
//        pso,
//        out p_commandList);

//    if(hr.IsFailure)
//    {
//      p_commandAllocator.Release();
//      throw new InvalidOperationException($"Failed to create command list: {hr}");
//    }

//    p_commandList.Close();

//    if(!string.IsNullOrEmpty(Name))
//    {
//      SetDebugName(Name);
//    }
//  }

//  private unsafe void SetDebugName(string _name)
//  {
//    var nameBytes = System.Text.Encoding.Unicode.GetBytes(_name + "\0");
//    fixed(byte* pName = nameBytes)
//    {
//      p_commandList.SetName((char*)pName);
//    }
//  }
//}
