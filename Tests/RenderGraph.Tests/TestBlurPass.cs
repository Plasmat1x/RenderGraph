using Core;
using Core.Enums;

using Resources;

namespace ResourcesTests;

public class TestBlurPass: RenderPass
{
  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }

  public TestBlurPass() : base("TestBlurPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Normal;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    if(!InputTexture.IsValid())
      throw new InvalidOperationException("TestBlurPass requires valid InputTexture");

    builder.ReadTexture(InputTexture);

    var inputDesc = (TextureDescription)builder.GetResourceDescription(InputTexture);
    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "BlurOutput";

    OutputTexture = builder.CreateTexture("BlurOutput", outputDesc);
    builder.WriteTexture(OutputTexture);
  }

  public override void Execute(RenderPassContext context)
  {
    // Mock execution
  }
}