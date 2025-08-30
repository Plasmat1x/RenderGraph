using Directx12Impl.Parts.Structures;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts;

public class DX12DescriptorAllocation: IDisposable
{
  private readonly DX12StaticDescriptorHeap p_heap;
  private readonly uint p_baseIndex;
  private readonly uint p_count;
  private readonly uint p_descriptorSize;
  private readonly CpuDescriptorHandle p_cpuHandle;
  private bool p_disposed;

  public CpuDescriptorHandle CpuHandle => p_cpuHandle;
  public GpuDescriptorHandle GpuHandle;
  public uint Index;
  public bool IsValid => CpuHandle.Ptr != 0;

  internal DX12DescriptorAllocation(
    DX12StaticDescriptorHeap _heap,
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

  public DX12DescriptorAllocation(CpuDescriptorHandle _cpuHandle, GpuDescriptorHandle _gpuHandle, uint _index)
  {
    p_cpuHandle = _cpuHandle;
    GpuHandle = _gpuHandle;
    Index = _index;
  }

  public DX12DescriptorAllocation(CpuDescriptorHandle _cpuHandle, uint _index)
  {
    p_cpuHandle = _cpuHandle;
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
