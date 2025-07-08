using Core;
using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using MockImpl;

using Resources;
using Resources.Enums;

using System.Xml.Linq;

public class DemoBlurPass: RenderPass
{
  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }
  public float BlurRadius { get; set; } = 5.0f;

  public DemoBlurPass() : base("DemoBlurPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Normal;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    Console.WriteLine($"[PASS] Setting up {Name}");

    if(!InputTexture.IsValid())
      throw new InvalidOperationException("BlurPass requires valid InputTexture");

    builder.ReadTexture(InputTexture);

    var inputDesc = (TextureDescription)builder.GetResourceDescription(InputTexture);
    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "BlurredOutput";

    OutputTexture = builder.CreateTexture("BlurredOutput", outputDesc);
    builder.WriteTexture(OutputTexture);

    builder.SetResourceLifetime(OutputTexture, ResourceLifetime.Persistent);
  }

  public override void Execute(RenderPassContext context)
  {
    Console.WriteLine($"[PASS] Executing {Name} (radius: {BlurRadius})");

    var commandBuffer = context.CommandBuffer;
    var inputTexture = context.GetTexture(InputTexture);
    var outputTexture = context.GetTexture(OutputTexture);

    commandBuffer.SetRenderTarget(outputTexture.GetDefaultRenderTargetView());
    context.SetFullScreenViewport();

    commandBuffer.SetShaderResource(ShaderStage.Pixel, 0, inputTexture.GetDefaultShaderResourceView());
    commandBuffer.DrawFullscreenQuad();
  }
}