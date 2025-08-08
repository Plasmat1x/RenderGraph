using Silk.NET.Direct3D12;

namespace Directx12Impl;

public struct ComputePSOCacheKey: IEquatable<ComputePSOCacheKey>
{
  public DX12Shader ComputeShader;
  public ID3D12RootSignature* RootSignature;

  public bool Equals(ComputePSOCacheKey other)
  {
    return ComputeShader == other.ComputeShader &&
           RootSignature == other.RootSignature;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(ComputeShader, (IntPtr)RootSignature);
  }
}