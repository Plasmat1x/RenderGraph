using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts;

public unsafe class DX12DynamicDescriptorHeap: IDisposable
{
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly ComPtr<ID3D12DescriptorHeap> p_heap;
  private readonly DescriptorHeapType p_type;
  private readonly uint p_descriptorSize;
  private readonly uint p_maxDescriptors;
  private uint p_currentOffset;
  private bool p_disposed;

  public DX12DynamicDescriptorHeap(
    ComPtr<ID3D12Device> _device, 
    DescriptorHeapType _type, 
    uint _maxDescriptors)
  {
    p_device = _device;
    p_type = _type;
    p_maxDescriptors = _maxDescriptors;

    var desc = new DescriptorHeapDesc
    {
      Type = _type,
      NumDescriptors = _maxDescriptors,
      Flags = DescriptorHeapFlags.ShaderVisible,
      NodeMask = 0
    };

    HResult hr = p_device.CreateDescriptorHeap(&desc, out p_heap);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create GPU descriptor heap: {hr}");

    p_descriptorSize = p_device.GetDescriptorHandleIncrementSize(_type);
  }

  public GpuDescriptorHandle CopyDescriptor(CpuDescriptorHandle _srcHandle, uint _count = 1)
  {
    if(p_currentOffset + _count > p_maxDescriptors)
      throw new InvalidOperationException("GPU descriptor heap overflow");

    var destCpuHandle = GetCPUHandle(p_currentOffset);
    var destGpuHandle = GetGPUHandle(p_currentOffset);

    p_device.CopyDescriptorsSimple(_count, destCpuHandle, _srcHandle, p_type);

    p_currentOffset += _count;

    return destGpuHandle;
  }

  public void Reset() => p_currentOffset = 0;

  public ComPtr<ID3D12DescriptorHeap> GetHeap() => p_heap;

  public void Dispose()
  {
    if(p_disposed)
      return;

    p_heap.Dispose();
    p_disposed = true;
  }

  private GpuDescriptorHandle GetGPUHandle(uint _index)
  {
    var startHandle = p_heap.GetGPUDescriptorHandleForHeapStart();
    return new GpuDescriptorHandle
    {
      Ptr = startHandle.Ptr + _index * p_descriptorSize
    };
  }

  private CpuDescriptorHandle GetCPUHandle(uint _index)
  {
    var startHandle = p_heap.GetCPUDescriptorHandleForHeapStart();
    return new CpuDescriptorHandle
    {
      Ptr = startHandle.Ptr + _index * p_descriptorSize
    };
  }
}
