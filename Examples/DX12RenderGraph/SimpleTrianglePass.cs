using Core;
using Core.Enums;
/// <summary>
/// Простой пасс для рендеринга треугольника  
/// </summary>
public class SimpleTrianglePass: RenderPass
{
  private ResourceHandle _renderTarget;

  public SimpleTrianglePass(string name) : base(name)
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.Normal;
  }

  public void SetRenderTarget(ResourceHandle renderTarget)
  {
    _renderTarget = renderTarget;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    if(_renderTarget.IsValid())
    {
      builder.WriteTexture(_renderTarget);
    }
    Console.WriteLine($"[{Name}] Setup completed");
  }

  public override void Execute(RenderPassContext context)
  {
    if(!_renderTarget.IsValid())
      return;

    Console.WriteLine($"[{Name}] Rendering triangle...");

  }
}