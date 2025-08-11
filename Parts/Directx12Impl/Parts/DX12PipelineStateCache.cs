using Directx12Impl.Extensions;
using Directx12Impl.Tools;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Directx12Impl.Parts;
public unsafe class DX12PipelineStateCache: IDisposable
{
  private readonly Dictionary<PSOCacheKey, ComPtr<ID3D12PipelineState>> p_graphicsCache = [];
  private readonly Dictionary<ComputePSOCacheKey, ComPtr<ID3D12PipelineState>> p_computeCache = [];
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly object p_cacheLock = new();
  private bool p_disposed;

  public DX12PipelineStateCache(ComPtr<ID3D12Device> _device) 
  { 
    p_device = _device;
  }

  //public ComPtr<ID3D12PipelineState> GetOrCreateGraphicsPipeline(GraphicsPipelineStateDesc _desc)
  //{
  //  if(p_graphicsCache.TryGetValue(_desc, out var pso))
  //    return pso;

  //  ComPtr<ID3D12PipelineState> newPso;
  //  p_device.CreateGraphicsPipelineState(ref _desc, out newPso);
  //  p_graphicsCache[_desc] = newPso;

  //  return newPso;
  //}

  //public ComPtr<ID3D12PipelineState> GetOrCreateComputePipeline(ComputePipelineStateDesc _desc)
  //{
  //  if(p_computeCache.TryGetValue(_desc, out var pso))
  //    return pso;

  //  ComPtr<ID3D12PipelineState> newPso;
  //  p_device.CreateComputePipelineState(ref _desc, out newPso);
  //  p_computeCache[_desc] = newPso;

  //  return newPso;
  //}


  //public void Clear()
  //{
  //  p_graphicsCache.Clear();
  //  p_computeCache.Clear();
  //}

  public unsafe ID3D12PipelineState* GetOrCreatePSO(PSOCacheKey _key)
  {
    lock(p_cacheLock)
    {
      if(p_graphicsCache.TryGetValue(_key, out var pso))
        return pso;

      pso = CreateGraphicsPSO(_key);
      p_graphicsCache[_key] = pso;
      return pso;
    }
  }

  public unsafe ID3D12PipelineState* GetOrCreateComputePSO(ComputePSOCacheKey _key)
  {
    lock(p_cacheLock)
    {
      if(p_computeCache.TryGetValue(_key, out var pso))
        return pso;

      pso = CreateComputePSO(_key);
      p_computeCache[_key] = pso;
      return pso;
    }
  }

  private unsafe ID3D12PipelineState* CreateGraphicsPSO(PSOCacheKey _key)
  {
    // Валидация шейдеров
    if(_key.VertexShader == null)
      throw new ArgumentException("Vertex shader is required");

    DX12ShaderValidator.ValidatePipelineShaders(
        _key.VertexShader,
        _key.PixelShader,
        _key.GeometryShader,
        _key.HullShader,
        _key.DomainShader);

    // Создание Input Layout из рефлексии VS
    var vsReflection = _key.VertexShader.GetReflection();
    var inputLayout = InputLayoutDescription.FromReflection(vsReflection);

    var vs = _key.VertexShader;
    var ps = _key.PixelShader;
    var ds = _key.DomainShader;
    var hs = _key.HullShader;
    var gs = _key.GeometryShader;

    if(vs == null)
      throw new ArgumentException("Vertex shader is required for graphics pipeline");

    var inputElements = _key.PipelineStateDescription.InputLayout.Convert();

    var psoDesc = new GraphicsPipelineStateDesc
    {
      PRootSignature = _key.RootSignature,
      VS = vs.GetD3D12Bytecode(),
      PS = ps?.GetD3D12Bytecode() ?? default,
      DS = ds?.GetD3D12Bytecode() ?? default,
      HS = hs?.GetD3D12Bytecode() ?? default,
      GS = gs?.GetD3D12Bytecode() ?? default,
      StreamOutput = default, // TODO: Stream output support
      BlendState = _key.RenderStateDescription.BlendState.Convert(),
      SampleMask = _key.PipelineStateDescription.SampleMask,
      RasterizerState = _key.RenderStateDescription.RasterizerState.Convert(),
      DepthStencilState = _key.RenderStateDescription.DepthStencilState.Convert(),
      InputLayout = new InputLayoutDesc
      {
        PInputElementDescs = inputElements.Length > 0 ? (InputElementDesc*)SilkMarshal.Allocate(inputElements.Length) : null,
        NumElements = (uint)inputElements.Length
      },
      IBStripCutValue = IndexBufferStripCutValue.ValueDisabled,
      PrimitiveTopologyType = _key.PipelineStateDescription.PrimitiveTopology.Convert(),
      NumRenderTargets = _key.PipelineStateDescription.RenderTargetCount,
      DSVFormat = _key.PipelineStateDescription.DepthStencilFormat.Convert(),
      SampleDesc = new SampleDesc
      {
        Count = _key.PipelineStateDescription.SampleCount,
        Quality = _key.PipelineStateDescription.SampleQuality
      },
      NodeMask = 0,
      Flags = PipelineStateFlags.None
    };

    for(var i = 0; i < 8; i++)
    {
      if(_key.PipelineStateDescription.RenderTargetFormats != null && i < _key.PipelineStateDescription.RenderTargetFormats.Length)
      {
        psoDesc.RTVFormats[i] = _key.PipelineStateDescription.RenderTargetFormats[i].Convert();
      }
      else
      {
        psoDesc.RTVFormats[i] = Format.FormatUnknown;
      }
    }

    ID3D12PipelineState* pipelineState;
    fixed(Guid* pGuid = &ID3D12PipelineState.Guid)
    {
      var hr = p_device.CreateGraphicsPipelineState(&psoDesc, pGuid, (void**)&pipelineState);
      DX12Helpers.ThrowIfFailed(hr, "Failed to create graphics pipeline state");
    }

    return pipelineState;
  }

  private unsafe ID3D12PipelineState* CreateComputePSO(ComputePSOCacheKey _key)
  {
    if(_key.ComputeShader == null)
      throw new ArgumentException("Compute shader is required");

    var psoDesc = new ComputePipelineStateDesc
    {
      PRootSignature = _key.RootSignature,
      CS = _key.ComputeShader.GetD3D12Bytecode(),
      NodeMask = 0,
      CachedPSO = default,
      Flags = PipelineStateFlags.None
    };

    ID3D12PipelineState* pipelineState;
    fixed(Guid* pGuid = &ID3D12PipelineState.Guid)
    {
      HResult hr = p_device.CreateComputePipelineState(&psoDesc, pGuid, (void**)&pipelineState);

      if(hr.IsFailure)
      {
        throw new InvalidOperationException($"Failed to create compute PSO: 0x{hr.Value:X8}");
      }
    }
    return pipelineState;
  }

  public void Clear()
  {
    lock(p_cacheLock)
    {
      foreach(var pso in p_graphicsCache.Values)
          pso.Dispose();

      p_graphicsCache.Clear();

      foreach(var pso in p_computeCache.Values)
        pso.Dispose();

      p_computeCache.Clear();
    }
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    Clear();

    p_disposed = true;
  }
}
