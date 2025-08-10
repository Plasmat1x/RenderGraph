using Silk.NET.Direct3D12;

namespace Directx12Impl;

public unsafe struct ComputePSOCacheKey: IEquatable<ComputePSOCacheKey>
{
  public DX12Shader ComputeShader;
  public ID3D12RootSignature* RootSignature;

  public bool Equals(ComputePSOCacheKey _other)
  {
    return ComputeShader == _other.ComputeShader &&
           RootSignature == _other.RootSignature;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(ComputeShader, (IntPtr)RootSignature);
  }
}