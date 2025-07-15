using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl;
public class DX12DescriptrorAllocator
{
  private ComPtr<ID3D12DescriptorHeap> p_heap;
  private uint p_descriptorSize;
  private uint p_numDescriptors;
  private uint p_numAllocated;
  private Stack<uint> p_freeList;

  public DX12DescriptrorAllocator() { }

  public CpuDescriptorHandle Allocate()
  {
    throw new NotImplementedException();
  }

  public void Free(CpuDescriptorHandle _descriptor)
  {
    throw new NotImplementedException();
  }

  public void Reset()
  {
    throw new NotImplementedException();
  }

  public ID3D12DescriptorHeap GetHeap()
  {
    throw new NotImplementedException();
  }
}
