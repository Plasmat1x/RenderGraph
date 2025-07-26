using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;
public unsafe class DX12DescriptorHeapManager: IDisposable
{
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly StaticDescriptorHeap p_rtvHeap;
  private readonly StaticDescriptorHeap p_dsvHeap;
  private readonly StaticDescriptorHeap p_srvHeap;
  private readonly StaticDescriptorHeap p_samplerHeap;

  private readonly DynamicDescriptorHeap p_gpuSrvHeap;
  private readonly DynamicDescriptorHeap p_gpuSamplerHeap;

  private bool p_disposed;

  public DX12DescriptorHeapManager(ComPtr<ID3D12Device> _device)
  {
    p_device = _device;

    p_rtvHeap = new StaticDescriptorHeap(
      _device,
      DescriptorHeapType.Rtv,
      256,
      false);

    p_dsvHeap = new StaticDescriptorHeap(
      _device,
      DescriptorHeapType.Dsv,
      128,
      false);

    p_srvHeap = new StaticDescriptorHeap(
      _device,
      DescriptorHeapType.CbvSrvUav,
      4096,
      false);

    p_samplerHeap = new StaticDescriptorHeap(
      _device,
      DescriptorHeapType.Sampler,
      256,
      false);

    p_gpuSrvHeap = new DynamicDescriptorHeap(
      _device,
      DescriptorHeapType.CbvSrvUav,
      1024);

    p_gpuSamplerHeap = new DynamicDescriptorHeap(
      _device,
      DescriptorHeapType.Sampler,
      64);
  }

  public DescriptorAllocation AllocateRTV(uint _count = 1)
  {
    return p_rtvHeap.Allocate(_count);
  }

  public DescriptorAllocation AllocateDSV(uint _count = 1)
  {
    return p_dsvHeap.Allocate(_count);
  }

  public DescriptorAllocation AllocateCBVSRVUAV(uint _count = 1)
  {
    return p_srvHeap.Allocate(_count);
  }

  public DescriptorAllocation AllocateSampler(uint _count = 1)
  {
    return p_samplerHeap.Allocate(_count);
  }

  public void SetDescriptorHeaps(ComPtr<ID3D12GraphicsCommandList> _commandList)
  {
    var heaps = stackalloc ID3D12DescriptorHeap*[2];
    heaps[0] = p_gpuSrvHeap.GetHeap();
    heaps[1] = p_gpuSamplerHeap.GetHeap();

    _commandList.SetDescriptorHeaps(2, heaps);
  }

  public GpuDescriptorHandle CopyToGPUHeap(CpuDescriptorHandle _cpuHandle, uint _count = 1)
  {
    return p_gpuSrvHeap.CopyDescriptor(_cpuHandle, _count);
  }

  public GpuDescriptorHandle CopySamplerToGPUHeap(CpuDescriptorHandle _cpuHandle, uint _count = 1)
  {
    return p_gpuSamplerHeap.CopyDescriptor(_cpuHandle, _count);
  }

  public void ResetForNewFrame()
  {
    p_gpuSamplerHeap.Reset();
    p_gpuSrvHeap.Reset();
  }

  public void Dispose()
  {
    if(p_disposed) 
      return;

    p_rtvHeap?.Dispose(); 
    p_dsvHeap?.Dispose();
    p_srvHeap?.Dispose();
    p_samplerHeap?.Dispose();
    p_gpuSrvHeap?.Dispose();
    p_gpuSamplerHeap?.Dispose();

    p_disposed = true;
  }
}
