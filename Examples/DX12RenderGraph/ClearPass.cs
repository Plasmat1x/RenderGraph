using Core;
using Core.Enums;

using Directx12Impl;

using System.Numerics;
/// <summary>
/// Пасс для очистки экрана
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
    Console.WriteLine($"[{Name}] Setup called");

    if(_renderTarget.IsValid())
    {
      builder.WriteTexture(_renderTarget);
    }
  }

  public override void Execute(RenderPassContext context)
  {
    Console.WriteLine($"[{Name}] Execute called");

    if(!_renderTarget.IsValid())
      return;

    var commandBuffer = context.CommandBuffer as DX12CommandBuffer;
    var renderTarget = context.GetTexture(_renderTarget);

    if(renderTarget != null)
    {
      var rtv = renderTarget.GetDefaultRenderTargetView();
      commandBuffer.SetRenderTarget(rtv);

      var clearColor = new Vector4(0.1f, 0.2f, 0.4f, 1.0f);
      commandBuffer.ClearRenderTarget(rtv, clearColor);

      Console.WriteLine($"[{Name}] Screen cleared");
    }
  }
}
