using Core;
using Core.Enums;

using GraphicsAPI.Enums;

using Resources;

namespace MockImpl;

public class DemoColorCorrectionPass: RenderPass
{
  public DemoColorCorrectionPass() : base("DemoColorCorrectionPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Low;
  }

  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }
  public float Gamma { get; set; } = 2.2f;
  public float Contrast { get; set; } = 1.1f;

  public override void Setup(RenderGraphBuilder _builder)
  {
    Console.WriteLine($"[PASS] Setting up {Name}");

    if(!InputTexture.IsValid())
      throw new InvalidOperationException("ColorCorrectionPass requires valid InputTexture");

    _builder.ReadTexture(InputTexture);

    var inputDesc = (TextureDescription)_builder.GetResourceDescription(InputTexture);
    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "FinalOutput";

    OutputTexture = _builder.CreateTexture("FinalOutput", outputDesc);
    _builder.WriteTexture(OutputTexture);

    _builder.SetResourceLifetime(OutputTexture, ResourceLifetime.Persistent);
  }

  public override void Execute(RenderPassContext _context)
  {
    Console.WriteLine($"[PASS] Executing {Name} (gamma: {Gamma:F1}, contrast: {Contrast:F1})");

    var commandBuffer = _context.CommandBuffer;
    var inputTexture = _context.GetTexture(InputTexture);
    var outputTexture = _context.GetTexture(OutputTexture);

    commandBuffer.SetRenderTarget(outputTexture.GetDefaultRenderTargetView());
    _context.SetFullScreenViewport();

    commandBuffer.SetShaderResource(ShaderStage.Pixel, 0, inputTexture.GetDefaultShaderResourceView());
    commandBuffer.DrawFullscreenQuad();
  }
}
