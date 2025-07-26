using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System;

namespace Directx12Impl.Managers;
public class DescriptorAllocator
{
  private readonly ComPtr<ID3D12DescriptorHeap> p_heap;
  private readonly uint p_descriptorSize;
  private readonly uint p_maxDescriptors;
  private uint p_currentIndex;
  private Stack<uint> p_freeIndices = [];

  public DescriptorAllocator(ComPtr<ID3D12DescriptorHeap> _heap, uint _descriptorSize, uint _maxDescriptors) 
  {
    p_heap = _heap;
    p_descriptorSize = _descriptorSize;
    p_maxDescriptors = _maxDescriptors;
  }

  public CpuDescriptorHandle Allocate()
  {
    uint index;

    if(p_freeIndices.Count > 0)
    {
      index = p_freeIndices.Pop();
    }
    else if(p_currentIndex < p_maxDescriptors)
    {
      index = p_currentIndex++;
    }
    else
    {
      throw new InvalidOperationException("Descriptor heap is full");
    }

    var startHandle = p_heap.GetCPUDescriptorHandleForHeapStart();
    return new CpuDescriptorHandle
    {
      Ptr = startHandle.Ptr + index * p_descriptorSize
    };
  }

  public void Free(uint _index)
  {
    p_freeIndices.Push(_index);
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
