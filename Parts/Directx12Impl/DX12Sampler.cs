using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;
public class DX12Sampler: ISampler
{
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly SamplerDescription p_description;
  private readonly SamplerDesc p_samplerDesc;
  private readonly Action<CpuDescriptorHandle> p_releaseDescriptorCallback;
  private CpuDescriptorHandle p_descriptor;
  private bool p_disposed;

  public DX12Sampler(ComPtr<ID3D12Device>? _device,
    SamplerDescription _description, 
    CpuDescriptorHandle _descriptor, 
    Action<CpuDescriptorHandle> _releaseDescriptorCallback)
  {
    p_device = _device ?? throw new ArgumentNullException(nameof(_device));
    p_description = _description ?? throw new ArgumentNullException(nameof(_description));
    p_descriptor = _descriptor;
    p_releaseDescriptorCallback = _releaseDescriptorCallback;

    p_samplerDesc = CreateSamplerDesc(_description);
    p_device.CreateSampler(ref p_samplerDesc, _descriptor);
  }

  public SamplerDescription Description => p_description;

  public string Name => p_description.Name;

  public ResourceType ResourceType => ResourceType.Sampler;

  public bool IsDisposed => p_disposed;

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  private SamplerDesc CreateSamplerDesc(SamplerDescription _desc)
  {
    throw new NotImplementedException();
  }

  private Filter ConvertFilter(FilterMode _filter)
  {
    throw new NotImplementedException();
  }

  private TextureAddressMode ConvertAddressMode(AddressMode _mode)
  {
    throw new NotImplementedException();
  }
}
