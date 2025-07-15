using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl;
public class DX12PipelineStateCache
{
  private readonly Dictionary<GraphicsPipelineStateDesc, ComPtr<ID3D12PipelineState>> p_graphicsCache = [];
  private readonly Dictionary<ComputePipelineStateDesc, ComPtr<ID3D12PipelineState>> p_computeCache = []; 
  private readonly ComPtr<ID3D12Device> p_device;

  public DX12PipelineStateCache(ComPtr<ID3D12Device> _device) 
  { 
    p_device = _device;
  }

  public ComPtr<ID3D12PipelineState> GetOrCreateGraphicsPipeline(GraphicsPipelineStateDesc _desc)
  {
    if(p_graphicsCache.TryGetValue(_desc, out var pso))
      return pso;

    ComPtr<ID3D12PipelineState> newPso;
    p_device.CreateGraphicsPipelineState(ref _desc, out newPso);
    p_graphicsCache[_desc] = newPso;

    return newPso;
  }

  public ComPtr<ID3D12PipelineState> GetOrCreateComputePipeline(ComputePipelineStateDesc _desc)
  {
    if(p_computeCache.TryGetValue(_desc, out var pso))
      return pso;

    ComPtr<ID3D12PipelineState> newPso;
    p_device.CreateComputePipelineState(ref _desc, out newPso);
    p_computeCache[_desc] = newPso;

    return newPso;
  }

  public void Clear()
  {
    p_graphicsCache.Clear();
    p_computeCache.Clear();
  }
}
