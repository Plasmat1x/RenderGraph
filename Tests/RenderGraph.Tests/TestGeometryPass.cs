using Core;
using Core.Enums;

namespace ResourcesTests;

public class TestGeometryPass: RenderPass
{
  public ResourceHandle ColorTarget { get; private set; }
  public ResourceHandle DepthTarget { get; private set; }

  public TestGeometryPass() : base("TestGeometryPass")
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.High;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    ColorTarget = builder.CreateColorTarget("GeometryColor", 1920, 1080);
    DepthTarget = builder.CreateDepthTarget("GeometryDepth", 1920, 1080);

    builder.WriteTexture(ColorTarget);
    builder.WriteTextureAsDepth(DepthTarget);
  }

  public override void Execute(RenderPassContext context)
  {
    // Mock execution
  }
}
