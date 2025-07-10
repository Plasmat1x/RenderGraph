using Core;
using Core.Enums;
using GraphicsAPI.Enums;
using Resources;

public class DemoBlurPass: RenderPass
{
  public DemoBlurPass() : base("DemoBlurPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Normal;
  }
  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }
  public float BlurRadius { get; set; } = 5.0f;


  public override void Setup(RenderGraphBuilder _builder)
  {
    Console.WriteLine($"[PASS] Setting up {Name}");

    if(!InputTexture.IsValid())
      throw new InvalidOperationException("BlurPass requires valid InputTexture");

    _builder.ReadTexture(InputTexture);

    var inputDesc = (TextureDescription)_builder.GetResourceDescription(InputTexture);
    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "BlurredOutput";

    OutputTexture = _builder.CreateTexture("BlurredOutput", outputDesc);
    _builder.WriteTexture(OutputTexture);

    _builder.SetResourceLifetime(OutputTexture, ResourceLifetime.Persistent);
  }

  public override void Execute(RenderPassContext _context)
  {
    Console.WriteLine($"[PASS] Executing {Name} (radius: {BlurRadius})");

    var commandBuffer = _context.CommandBuffer;
    var inputTexture = _context.GetTexture(InputTexture);
    var outputTexture = _context.GetTexture(OutputTexture);

    commandBuffer.SetRenderTarget(outputTexture.GetDefaultRenderTargetView());
    _context.SetFullScreenViewport();

    commandBuffer.SetShaderResource(ShaderStage.Pixel, 0, inputTexture.GetDefaultShaderResourceView());
    commandBuffer.DrawFullscreenQuad();
  }
}