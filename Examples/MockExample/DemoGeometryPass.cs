using Core;
using Core.Enums;

using GraphicsAPI.Enums;

using Passes;

using System.Numerics;

namespace MockImpl;

public class DemoGeometryPass: RenderPass
{
  public ResourceHandle ColorTarget { get; private set; }
  public ResourceHandle DepthTarget { get; private set; }
  public Vector4 ClearColor { get; set; } = new Vector4(0.2f, 0.3f, 0.4f, 1.0f);

  public DemoGeometryPass() : base("DemoGeometryPass")
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.High;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    Console.WriteLine($"[PASS] Setting up {Name}");

    ColorTarget = builder.CreateColorTarget("MainColor", 1920, 1080);
    DepthTarget = builder.CreateDepthTarget("MainDepth", 1920, 1080);

    builder.WriteTexture(ColorTarget);
    builder.WriteTextureAsDepth(DepthTarget);

    builder.SetResourceLifetime(ColorTarget, ResourceLifetime.Persistent);
    builder.SetResourceLifetime(DepthTarget, ResourceLifetime.Persistent);
  }

  public override void Execute(RenderPassContext context)
  {
    Console.WriteLine($"[PASS] Executing {Name}");

    var commandBuffer = context.CommandBuffer;
    var colorTexture = context.GetTexture(ColorTarget);
    var depthTexture = context.GetTexture(DepthTarget);

    commandBuffer.SetRenderTarget(colorTexture.GetDefaultRenderTargetView(), depthTexture.GetDefaultDepthStencilView());
    context.SetFullScreenViewport();

    commandBuffer.ClearRenderTarget(colorTexture.GetDefaultRenderTargetView(), ClearColor);
    commandBuffer.ClearDepthStencil(depthTexture.GetDefaultDepthStencilView(), ClearFlags.DepthStencil, 1.0f, 0);

    // Simulate rendering some geometry
    for(int i = 0; i < 3; i++)
    {
      commandBuffer.DrawIndexed(1200, 1); // Simulate drawing 3 objects
    }
  }
}
