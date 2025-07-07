using Core;
using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Passes.Enums;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace Passes;

public class ColorCorrectionPass: RenderPass
{
  private ResourceHandle p_colorCorrectionBuffer;

  public ColorCorrectionPass() : base("ColorCorrectionPass")
  {
  }

  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }


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

  public override void Execute(RenderPassContext _context)
  {
    var commandBuffer = _context.CommandBuffer;

    var inputTexture = _context.GetTexture(InputTexture);
    var outputTexture = _context.GetTexture(OutputTexture);

    UpdateColorCorrectionParameters(_context);

    commandBuffer.SetRenderTargets(new[] { outputTexture }, null);

    _context.SetFullScreenViewport();

    SetupColorCorrectionShaders(commandBuffer);

    commandBuffer.SetShaderResource(0, inputTexture);

    var colorCorrectionBuffer = _context.GetBuffer(p_colorCorrectionBuffer);
    commandBuffer.SetConstantBuffer(0, colorCorrectionBuffer);

    commandBuffer.DrawFullscreenQuad();
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

    var mappedData = buffer.Map();
    unsafe
    {
      *(ColorCorrectionParameters*)mappedData = colorParams;
    }
    buffer.Unmap();
  }

  private void SetupColorCorrectionShaders(CommandBuffer _commandBuffer)
  {
    _commandBuffer.SetVertexShader(GetColorCorrectionVertexShader());
    _commandBuffer.SetPixelShader(GetColorCorrectionPixelShader());
  }

  private IShader GetColorCorrectionVertexShader() => null;
  private IShader GetColorCorrectionPixelShader() => null;
}