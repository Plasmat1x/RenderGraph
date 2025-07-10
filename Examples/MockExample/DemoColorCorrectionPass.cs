using Core;
using Core.Enums;

using GraphicsAPI.Enums;

using Passes;

using Resources;

namespace MockImpl;

public class DemoColorCorrectionPass: RenderPass
{
  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }
  public float Gamma { get; set; } = 2.2f;
  public float Contrast { get; set; } = 1.1f;

  public DemoColorCorrectionPass() : base("DemoColorCorrectionPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Low;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    Console.WriteLine($"[PASS] Setting up {Name}");

    if(!InputTexture.IsValid())
      throw new InvalidOperationException("ColorCorrectionPass requires valid InputTexture");

    builder.ReadTexture(InputTexture);

    var inputDesc = (TextureDescription)builder.GetResourceDescription(InputTexture);
    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "FinalOutput";

    OutputTexture = builder.CreateTexture("FinalOutput", outputDesc);
    builder.WriteTexture(OutputTexture);

    builder.SetResourceLifetime(OutputTexture, ResourceLifetime.Persistent);
  }

  public override void Execute(RenderPassContext context)
  {
    Console.WriteLine($"[PASS] Executing {Name} (gamma: {Gamma:F1}, contrast: {Contrast:F1})");

    var commandBuffer = context.CommandBuffer;
    var inputTexture = context.GetTexture(InputTexture);
    var outputTexture = context.GetTexture(OutputTexture);

    commandBuffer.SetRenderTarget(outputTexture.GetDefaultRenderTargetView());
    context.SetFullScreenViewport();

    commandBuffer.SetShaderResource(ShaderStage.Pixel, 0, inputTexture.GetDefaultShaderResourceView());
    commandBuffer.DrawFullscreenQuad();
  }
}
