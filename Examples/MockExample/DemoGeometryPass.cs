using Core;
using Core.Enums;

using GraphicsAPI.Enums;

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

  public override void Setup(RenderGraphBuilder _builder)
  {
    Console.WriteLine($"[PASS] Setting up {Name}");

    ColorTarget = _builder.CreateColorTarget("MainColor", 1920, 1080);
    DepthTarget = _builder.CreateDepthTarget("MainDepth", 1920, 1080);

    _builder.WriteTexture(ColorTarget);
    _builder.WriteTextureAsDepth(DepthTarget);

    _builder.SetResourceLifetime(ColorTarget, ResourceLifetime.Persistent);
    _builder.SetResourceLifetime(DepthTarget, ResourceLifetime.Persistent);
  }

  public override void Execute(RenderPassContext _context)
  {
    Console.WriteLine($"[PASS] Executing {Name}");

    var commandBuffer = _context.CommandBuffer;
    var colorTexture = _context.GetTexture(ColorTarget);
    var depthTexture = _context.GetTexture(DepthTarget);

    commandBuffer.SetRenderTarget(colorTexture.GetDefaultRenderTargetView(), depthTexture.GetDefaultDepthStencilView());
    _context.SetFullScreenViewport();

    commandBuffer.ClearRenderTarget(colorTexture.GetDefaultRenderTargetView(), ClearColor);
    commandBuffer.ClearDepthStencil(depthTexture.GetDefaultDepthStencilView(), ClearFlags.DepthAndStencil, 1.0f, 0);

    for(int i = 0; i < 3; i++)
    {
      commandBuffer.DrawIndexed(1200, 1);
    }
  }
}
