using Silk.NET.Direct3D12;

namespace Directx12Impl;

/// <summary>
/// Структура для хранения handle'ов дескрипторов
/// </summary>
public struct DX12DescriptorHandle
{
  public CpuDescriptorHandle CpuHandle;
  public GpuDescriptorHandle GpuHandle;
  public bool IsValid => CpuHandle.Ptr != 0;

  public DX12DescriptorHandle(CpuDescriptorHandle _cpuHandle, GpuDescriptorHandle _gpuHandle)
  {
    CpuHandle = _cpuHandle;
    GpuHandle = _gpuHandle;
  }

  public DX12DescriptorHandle(CpuDescriptorHandle _cpuHandle)
  {
    CpuHandle = _cpuHandle;
    GpuHandle = default;
  }
}
