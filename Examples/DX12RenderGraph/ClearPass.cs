using Core;
using Core.Enums;
/// <summary>
/// Простой пасс для очистки экрана
/// </summary>
public class ClearPass: RenderPass
{
  private ResourceHandle _renderTarget;

  public ClearPass(string name) : base(name)
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.High;
    AlwaysExecute = true;
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

    Console.WriteLine($"[{Name}] Clearing render target...");

    // В реальной реализации здесь была бы очистка через command buffer
    // var commandBuffer = context.CommandBuffer as DX12CommandBuffer;
    // var renderTarget = context.GetTexture(_renderTarget);
    // commandBuffer.ClearRenderTarget(renderTarget.GetDefaultRenderTargetView(), clearColor);
  }
}
