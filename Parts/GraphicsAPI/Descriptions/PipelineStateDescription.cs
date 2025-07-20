using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace GraphicsAPI.Descriptions;

public class PipelineStateDescription
{
  public IShader VertexShader { get; set; }
  public IShader PixelShader { get; set; }
  public IShader DomainShader { get; set; }
  public IShader HullShader { get; set; }
  public IShader GeometryShader { get; set; }
  public IShader ComputeShader { get; set; }

  public InputLayoutDescription InputLayout { get; set; }
  public PrimitiveTopology PrimitiveTopology { get; set; } = PrimitiveTopology.TriangleList;

  public TextureFormat[] RenderTargetFormats { get; set; } = new[] { TextureFormat.R8G8B8A8_UNORM };
  public TextureFormat DepthStencilFormat { get; set; } = TextureFormat.D24_UNORM_S8_UINT;

  public uint RenderTargetCount { get; set; } = 1;
  public uint SampleCount { get; set; } = 1;
  public uint SampleQuality { get; set; } = 0;
  public uint SampleMask { get; set; } = uint.MaxValue;
}