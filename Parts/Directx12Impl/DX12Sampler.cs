using Directx12Impl.Extensions;
using Directx12Impl.Parts;
using Directx12Impl.Parts.Utils;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System.Text;

namespace Directx12Impl;
public class DX12Sampler: ISampler
{
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly SamplerDescription p_description;
  private readonly SamplerDesc p_samplerDesc;
  private readonly DX12DescriptorAllocation p_allocation;
  private bool p_disposed;

  public DX12Sampler(ComPtr<ID3D12Device>? _device,
    SamplerDescription _description,
    DX12DescriptorAllocation _allocation)
  {
    p_device = _device ?? throw new ArgumentNullException(nameof(_device));
    p_description = _description ?? throw new ArgumentNullException(nameof(_description));

    p_samplerDesc = CreateSamplerDesc(_description);
    p_device.CreateSampler(ref p_samplerDesc, _allocation.CpuHandle);
  }

  public SamplerDescription Description => p_description;

  public string Name => p_description.Name;

  public ResourceType ResourceType => ResourceType.Sampler;

  public bool IsDisposed => p_disposed;

  public ulong GetMemorySize()
  {
    return 0;
  }

  public IntPtr GetNativeHandle()
  {
    ThrowIfDisposed();
    return (IntPtr)p_allocation.CpuHandle.Ptr;
  }

  public CpuDescriptorHandle GetDescriptorHandle()
  {
    ThrowIfDisposed();
    return p_allocation.CpuHandle;
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    p_allocation.Dispose();

    p_disposed = true;

    GC.SuppressFinalize(this);
  }

  private unsafe SamplerDesc CreateSamplerDesc(SamplerDescription _desc)
  {
    var samplerDesc = new SamplerDesc
    {
      Filter = DX12Helpers.ConvertFilter(
        _desc.MinFilter,
        _desc.MagFilter,
        _desc.MipFilter,
        _desc.ComparisonFunction != ComparisonFunction.Never),

      AddressU = _desc.AddressModeU.Convert(),
      AddressV = _desc.AddressModeV.Convert(),
      AddressW = _desc.AddressModeW.Convert(),

      MipLODBias = _desc.LODBias,
      MaxAnisotropy = _desc.MaxAnisotropy,
      ComparisonFunc = _desc.ComparisonFunction.Convert(),
      MinLOD = _desc.MinLOD,
      MaxLOD = _desc.MaxLOD,
    };

    samplerDesc.BorderColor[0] = _desc.BorderColor.X;
    samplerDesc.BorderColor[1] = _desc.BorderColor.Y;
    samplerDesc.BorderColor[2] = _desc.BorderColor.Z;
    samplerDesc.BorderColor[3] = _desc.BorderColor.W;

    if(_desc.MaxAnisotropy > 1)
    {
      samplerDesc.Filter = _desc.ComparisonFunction != ComparisonFunction.Never
        ? Filter.ComparisonAnisotropic
        : Filter.Anisotropic;
    }

    return samplerDesc;
  }

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(DX12Sampler));
  }
}
