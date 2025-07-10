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

  public override void Setup(RenderGraphBuilder _builder)
  {
    ColorTarget = _builder.CreateColorTarget("GeometryColor", 1920, 1080);
    DepthTarget = _builder.CreateDepthTarget("GeometryDepth", 1920, 1080);

    _builder.WriteTexture(ColorTarget);
    _builder.WriteTextureAsDepth(DepthTarget);
  }

  public override void Execute(RenderPassContext _context)
  {
  }
}
