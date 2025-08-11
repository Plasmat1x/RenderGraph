using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts.Structures;

public partial class DX12RootSignatureCache
{
  private class RootSignatureDescComparer: IEqualityComparer<RootSignatureDesc>
  {
    public unsafe bool Equals(RootSignatureDesc _x, RootSignatureDesc _y)
    {
      return _x.NumParameters == _y.NumParameters &&
             _x.NumStaticSamplers == _y.NumStaticSamplers &&
             _x.Flags == _y.Flags &&
             _x.PStaticSamplers == _y.PStaticSamplers &&
             _x.PParameters == _y.PParameters;
    }

    public int GetHashCode(RootSignatureDesc _obj)
    {
      return HashCode.Combine(
          _obj.NumParameters,
          _obj.NumStaticSamplers,
          _obj.Flags);
    }
  }
}
