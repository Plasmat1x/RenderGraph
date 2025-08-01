using Silk.NET.Direct3D12;

using System.Runtime.InteropServices;

namespace Directx12Impl;

public class DescriptorAllocation: IDisposable
{
  private readonly StaticDescriptorHeap p_heap;
  private readonly uint p_baseIndex;
  private readonly uint p_count;
  private readonly uint p_descriptorSize;
  private readonly CpuDescriptorHandle p_cpuHandle;
  private bool p_disposed;

  public CpuDescriptorHandle CpuHandle;
  public GpuDescriptorHandle GpuHandle;
  public uint Index;
  public bool IsValid => CpuHandle.Ptr != 0;

  internal DescriptorAllocation(
    StaticDescriptorHeap _heap,
    uint _baseIndex,
    uint _count,
    uint _descriptorSize,
    CpuDescriptorHandle _cpuHandle)
  {
    p_heap = _heap;
    p_baseIndex = _baseIndex;
    p_count = _count;
    p_descriptorSize = _descriptorSize;
    p_cpuHandle = _cpuHandle;
  }

  public DescriptorAllocation(CpuDescriptorHandle _cpuHandle, GpuDescriptorHandle _gpuHandle, uint _index)
  {
    CpuHandle = _cpuHandle;
    GpuHandle = _gpuHandle;
    Index = _index;
  }

  public DescriptorAllocation(CpuDescriptorHandle _cpuHandle, uint _index)
  {
    CpuHandle = _cpuHandle;
    GpuHandle = default;
    Index = _index;
  }

  public uint Count => p_count;

  public CpuDescriptorHandle GetHandle(uint _index = 0)
  {
    if(_index >= p_count)
      throw new ArgumentOutOfRangeException(nameof(_index));

    return new CpuDescriptorHandle
    {
      Ptr = p_cpuHandle.Ptr + _index * p_descriptorSize,
    };
  }

  public void Dispose()
  {
    if(p_disposed) 
      return;

    p_heap.Free(p_baseIndex, p_count);
    p_disposed = true;
  }
}
