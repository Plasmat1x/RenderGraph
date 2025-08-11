using Directx12Impl.Parts;
using Directx12Impl.Parts.Data;
using Directx12Impl.Parts.Structures;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Directx12Impl;
public unsafe class DX12RenderState: IRenderState
{
  private readonly DX12PipelineStateCache p_pipelineStateCache;
  private readonly DX12RootSignatureCache p_rootSignatureCache;

  private ComPtr<ID3D12PipelineState> p_pipelineState;
  private ComPtr<ID3D12RootSignature> p_rootSignature;
  private RenderStateDescription p_description;
  private PipelineStateDescription p_pipelineDescription;

  private bool p_disposed;

  public DX12RenderState(
    RenderStateDescription _desc,
    PipelineStateDescription _pipelineDescription,
    DX12RootSignatureCache _rootSignatureCache,
    DX12PipelineStateCache _pipelineStateCache)
  {
    p_description = _desc ?? throw new ArgumentNullException(nameof(_desc));
    p_pipelineDescription = _pipelineDescription ?? throw new ArgumentNullException(nameof(_pipelineDescription));
    p_rootSignatureCache = _rootSignatureCache ?? throw new ArgumentNullException(nameof(_rootSignatureCache));
    p_pipelineStateCache = _pipelineStateCache ?? throw new ArgumentNullException(nameof(_pipelineStateCache));

    CreatePipelineState();
  }

  public RenderStateDescription Description => p_description;

  public string Name => p_description.Name;

  public ResourceType ResourceType => ResourceType.RenderState;

  public bool IsDisposed => p_disposed;

  public ComPtr<ID3D12PipelineState> GetPipelineState()
  {
    ThrowIfDisposed();
    return p_pipelineState;
  }

  public ComPtr<ID3D12RootSignature> GetRootSignature()
  {
    ThrowIfDisposed();
    return p_rootSignature;
  }

  public unsafe IntPtr GetNativeHandle()
  {
    ThrowIfDisposed();
    return (IntPtr)p_pipelineState.Handle;
  }

  public ulong GetMemorySize()
  {
    // PSO size is not directly queryable
    return 0;
  }

  public unsafe void Dispose()
  {
    if(p_disposed)
      return;

    if(p_pipelineState.Handle != null)
      p_pipelineState.Dispose();


    if(p_rootSignature.Handle != null)
      p_rootSignature.Dispose();

    p_disposed = true;
    GC.SuppressFinalize(this);
  }

  private unsafe void CreatePipelineState()
  {
    if(p_pipelineDescription.ComputeShader != null)
    {
      p_rootSignature = p_rootSignatureCache.GetDefaultComputeRootSignature();
      CreateComputePipelineState();
    }
    else
    {
      p_rootSignature = p_rootSignatureCache.GetOrCreateFromShaderReflection();
      CreateGraphicsPipelineState();
    }
  }

  private unsafe void CreateGraphicsPipelineState()
  {
    var vs = p_pipelineDescription.VertexShader as DX12Shader;
    var ps = p_pipelineDescription.PixelShader as DX12Shader;
    var ds = p_pipelineDescription.DomainShader as DX12Shader;
    var hs = p_pipelineDescription.HullShader as DX12Shader;
    var gs = p_pipelineDescription.GeometryShader as DX12Shader;

    if(vs == null)
      throw new ArgumentException("Vertex shader is required for graphics pipeline");

    var key = new PSOCacheKey
    {
      VertexShader = vs,
      PixelShader = ps,
      DomainShader = ds,
      GeometryShader = gs,
      HullShader = hs,
      PipelineStateDescription = p_pipelineDescription,
      RenderStateDescription = p_description,
      RootSignature = p_rootSignature,
    };

    p_pipelineState = p_pipelineStateCache.GetOrCreatePSO(key);
  }

  private void CreateComputePipelineState()
  {
    var cs = p_pipelineDescription.ComputeShader as DX12Shader;

    if(cs == null)
      throw new ArgumentException("Compute shader is required for compute pipeline");

    var psoDesc = new ComputePSOCacheKey
    {
      ComputeShader = cs,
      RootSignature = p_rootSignature
    };

    p_pipelineState = p_pipelineStateCache.GetOrCreateComputePSO(psoDesc);
  }

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(DX12RenderState));
  }
}
