using GraphicsAPI.Descriptions;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;

using System.Linq.Expressions;

using System.Security.Cryptography;

namespace Directx12Impl;
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

  public unsafe ID3D12PipelineState* GetOrCreatePSO(PSOCacheKey key)
  {
    lock(p_cacheLock)
    {
      if(p_graphicsCache.TryGetValue(key, out var pso))
        return pso;

      pso = CreateGraphicsPSO(key);
      p_graphicsCache[key] = pso;
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

    ShaderValidator.ValidatePipelineShaders(
        _key.VertexShader,
        _key.PixelShader,
        _key.GeometryShader,
        _key.HullShader,
        _key.DomainShader);

    // Создание Input Layout из рефлексии VS
    var vsReflection = _key.VertexShader.GetReflection();
    var inputLayout = InputLayoutDescription.FromReflection(vsReflection);

    // Здесь создается PSO аналогично DX12RenderState.CreateGraphicsPipelineState
    // ... код создания PSO ...

    return null; // Заглушка
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
