using Core;
using Core.Enums;

using Resources;

namespace ResourcesTests;

public class TestBlurPass: RenderPass
{
  public TestBlurPass() : base("TestBlurPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Normal;
  }

  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }


  public override void Setup(RenderGraphBuilder _builder)
  {
    if(!InputTexture.IsValid())
      throw new InvalidOperationException("TestBlurPass requires valid InputTexture");

    _builder.ReadTexture(InputTexture);

    var inputDesc = (TextureDescription)_builder.GetResourceDescription(InputTexture);
    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "BlurOutput";

    OutputTexture = _builder.CreateTexture("BlurOutput", outputDesc);
    _builder.WriteTexture(OutputTexture);
  }

  public override void Execute(RenderPassContext _context)
  {
  }
}