using Core;
using Core.Enums;

using Resources.Enums;

namespace DX12RenderGraph;

/// <summary>
/// Простой пасс для рендеринга в текстуру
/// </summary>
public class SimpleRenderToTexturePass: RenderPass
{
  public ResourceHandle OutputTexture { get; private set; }

  public uint OutputWidth { get; set; } = 1280;
  public uint OutputHeight { get; set; } = 720;
  public TextureFormat OutputFormat { get; set; } = TextureFormat.R8G8B8A8_UNORM;

  public SimpleRenderToTexturePass(string name) : base(name)
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.High;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    OutputTexture = builder.CreateColorTarget(
        "RenderToTextureOutput",
        OutputWidth,
        OutputHeight,
        OutputFormat
    );

    builder.WriteTexture(OutputTexture);
    Console.WriteLine($"[{Name}] Setup: Created {OutputWidth}x{OutputHeight} output texture");
  }

  public override void Execute(RenderPassContext context)
  {
    Console.WriteLine($"[{Name}] Rendering to texture...");
  }
}
