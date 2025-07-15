using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl;

public class DX12RootSignatureCache
{
  private Dictionary<RootSignatureDesc, ComPtr<ID3D12RootSignature>> p_chache;
  private ComPtr<ID3D12Device> p_device;

  public DX12RootSignatureCache() { }

  public ID3D12RootSignature GetOrCreate(RootSignatureDesc _desc)
  {
    throw new NotImplementedException();
  }

  public void Clear()
  {
    throw new NotImplementedException();
  }
}
