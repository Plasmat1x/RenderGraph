namespace Directx12Impl;

public struct PSOCacheKey: IEquatable<PSOCacheKey>
{
  public DX12Shader VertexShader;
  public DX12Shader PixelShader;
  public DX12Shader GeometryShader;
  public DX12Shader HullShader;
  public DX12Shader DomainShader;
  public DX12RenderState RenderState;

  public bool Equals(PSOCacheKey other)
  {
    return VertexShader == other.VertexShader &&
           PixelShader == other.PixelShader &&
           GeometryShader == other.GeometryShader &&
           HullShader == other.HullShader &&
           DomainShader == other.DomainShader &&
           RenderState == other.RenderState;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(VertexShader, PixelShader,
        GeometryShader, HullShader, DomainShader, RenderState);
  }
}
