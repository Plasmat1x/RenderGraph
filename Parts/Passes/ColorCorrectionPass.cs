using Core;
using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Passes.Enums;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace Passes;

public class ColorCorrectionPass: RenderPass
{
  private ResourceHandle p_colorCorrectionBuffer;
  private IShader p_vertexShader;
  private IShader p_pixelShader;
  private ISampler p_pointSampler;

  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }

  public ColorCorrectionPass() : base("ColorCorrectionPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Low;
  }

  public float Gamma { get; set; } = 2.2f;
  public float Contrast { get; set; } = 1.0f;
  public float Brightness { get; set; } = 0.0f;
  public float Saturation { get; set; } = 1.0f;
  public Vector3 ColorBalance { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);
  public bool EnableToneMapping { get; set; } = true;
  public ToneMappingType ToneMappingType { get; set; } = ToneMappingType.ACES;
  public float Exposure { get; set; } = 1.0f;

  public override void Setup(RenderGraphBuilder _builder)
  {
    if(!InputTexture.IsValid())
      throw new InvalidOperationException("ColorCorrectionPass requires valid InputTexture");

    _builder.ReadTexture(InputTexture);

    var inputDesc = (TextureDescription)_builder.GetResourceDescription(InputTexture);

    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "ColorCorrectedOutput";
    outputDesc.TextureUsage = TextureUsage.RenderTarget;
    outputDesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;

    OutputTexture = _builder.CreateTexture("ColorCorrectedOutput", outputDesc);
    _builder.WriteTexture(OutputTexture);

    p_colorCorrectionBuffer = _builder.CreateConstantBuffer("ColorCorrectionParams", 128);
    _builder.WriteBuffer(p_colorCorrectionBuffer);

    _builder.SetResourceLifetime(OutputTexture, ResourceLifetime.Persistent);
  }

  public override void OnGraphCompiled(RenderGraph _renderGraph) => CreateShaders(_renderGraph);

  public override void Execute(RenderPassContext _context)
  {
    var commandBuffer = _context.CommandBuffer;

    var inputTexture = _context.GetTexture(InputTexture);
    var outputTexture = _context.GetTexture(OutputTexture);

    var inputView = inputTexture.GetDefaultShaderResourceView();
    var outputView = outputTexture.GetDefaultRenderTargetView();

    UpdateColorCorrectionParameters(_context);
    commandBuffer.SetRenderTarget(outputView);
    _context.SetFullScreenViewport();
    SetupColorCorrectionShaders(commandBuffer);
    commandBuffer.SetShaderResource(ShaderStage.Pixel, 0, inputView);
    commandBuffer.SetSampler(ShaderStage.Pixel, 0, p_pointSampler);

    var colorCorrectionBuffer = _context.GetBuffer(p_colorCorrectionBuffer);
    var paramsView = colorCorrectionBuffer.GetDefaultShaderResourceView();

    commandBuffer.SetConstantBuffer(ShaderStage.Pixel, 0, paramsView);

    commandBuffer.DrawFullscreenQuad();
  }

  public override void Dispose()
  {
    p_vertexShader?.Dispose();
    p_pixelShader?.Dispose();
    p_pointSampler?.Dispose();
    base.Dispose();
  }

  private void CreateShaders(RenderGraph _renderGraph)
  {
    var device = _renderGraph.GetGraphicsDevice();

    var vsDesc = new ShaderDescription
    {
      Name = "ColorCorrectionVertexShader",
      Stage = ShaderStage.Vertex,
      Bytecode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "VSMain"
    };
    p_vertexShader = device.CreateShader(vsDesc);

    var psDesc = new ShaderDescription
    {
      Name = "ColorCorrectionPixelShader",
      Stage = ShaderStage.Pixel,
      Bytecode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "PSColorCorrection"
    };
    p_pixelShader = device.CreateShader(psDesc);

    var samplerDesc = new SamplerDescription
    {
      Name = "PointSampler",
      MinFilter = FilterMode.Point,
      MagFilter = FilterMode.Point,
      AddressModeU = AddressMode.Clamp,
      AddressModeV = AddressMode.Clamp
    };
    p_pointSampler = device.CreateSampler(samplerDesc);
  }

  private void UpdateColorCorrectionParameters(RenderPassContext _context)
  {
    var buffer = _context.GetBuffer(p_colorCorrectionBuffer);

    var colorParams = new ColorCorrectionParameters
    {
      Gamma = Gamma,
      Contrast = Contrast,
      Brightness = Brightness,
      Saturation = Saturation,
      ColorBalance = ColorBalance,
      Exposure = Exposure,
      EnableToneMapping = EnableToneMapping ? 1 : 0,
      ToneMappingType = (int)ToneMappingType
    };

    var mappedData = buffer.Map(MapMode.WriteDiscard);
    unsafe
    {
      *(ColorCorrectionParameters*)mappedData = colorParams;
    }
    buffer.Unmap();
  }

  private void SetupColorCorrectionShaders(CommandBuffer _commandBuffer)
  {
    _commandBuffer.SetVertexShader(p_vertexShader);
    _commandBuffer.SetPixelShader(p_pixelShader);
  }
}