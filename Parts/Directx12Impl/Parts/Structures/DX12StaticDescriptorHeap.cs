using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts.Structures;

public unsafe class DX12StaticDescriptorHeap: IDisposable
{
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly ComPtr<ID3D12DescriptorHeap> p_heap;
  private readonly uint p_descriptorSize;
  private readonly uint p_maxDescriptors;
  private readonly Stack<uint> p_freeIndices = [];
  private uint p_currentIndex;
  private bool p_disposed;

  public DX12StaticDescriptorHeap(
    ComPtr<ID3D12Device> _device,
    DescriptorHeapType _type,
    uint _maxDescriptors,
    bool _shaderVisible)
  {
    p_device = _device;
    p_maxDescriptors = _maxDescriptors;

    var desc = new DescriptorHeapDesc
    {
      Type = _type,
      NumDescriptors = _maxDescriptors,
      Flags = _shaderVisible
      ? DescriptorHeapFlags.ShaderVisible
      : DescriptorHeapFlags.None,
      NodeMask = 0
    };

    HResult gr = p_device.CreateDescriptorHeap(&desc, out p_heap);

    p_descriptorSize = p_device.GetDescriptorHandleIncrementSize(_type);
  }

  public DX12DescriptorAllocation Allocate(uint _count = 1)
  {
    if(_count == 0 || _count > p_maxDescriptors)
      throw new ArgumentException("Invalid descriptor count");

    if(p_currentIndex + _count <= p_maxDescriptors)
    {
      var allocation = new DX12DescriptorAllocation(
        this, 
        p_currentIndex, 
        _count, 
        p_descriptorSize, 
        GetCPUHandle(p_currentIndex));

      p_currentIndex += _count;

      return allocation;
    }

    //TODO: need defragmentation or search free space
    throw new InvalidOperationException("Descriptor heap is full");
  }

  public CpuDescriptorHandle GetCPUHandle(uint _index)
  {
    var startHandle = p_heap.GetCPUDescriptorHandleForHeapStart();
    return new CpuDescriptorHandle
    {
      Ptr = startHandle.Ptr + _index * p_descriptorSize
    };
  }

  public ComPtr<ID3D12DescriptorHeap> GetHeap() => p_heap;

  public void Free(uint _index, uint _count)
  {
    for(uint i = 0; i < _count; i++)
      p_freeIndices.Push(i);
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    p_heap.Dispose();
    p_disposed = true;
  }
}

