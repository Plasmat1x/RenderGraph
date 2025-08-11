using GraphicsAPI.Descriptions;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts;

public struct PSOCacheKey: IEquatable<PSOCacheKey>
{
  public DX12Shader VertexShader;
  public DX12Shader PixelShader;
  public DX12Shader GeometryShader;
  public DX12Shader HullShader;
  public DX12Shader DomainShader;
  public RenderStateDescription RenderStateDescription;
  public PipelineStateDescription PipelineStateDescription;
  public ComPtr<ID3D12RootSignature> RootSignature;

  public bool Equals(PSOCacheKey _other)
  {
    return VertexShader == _other.VertexShader &&
           PixelShader == _other.PixelShader &&
           GeometryShader == _other.GeometryShader &&
           HullShader == _other.HullShader &&
           DomainShader == _other.DomainShader &&
           RenderStateDescription == _other.RenderStateDescription &&
           PipelineStateDescription == _other.PipelineStateDescription;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(VertexShader, PixelShader,
        GeometryShader, HullShader, DomainShader, RenderStateDescription, PipelineStateDescription);
  }
}
