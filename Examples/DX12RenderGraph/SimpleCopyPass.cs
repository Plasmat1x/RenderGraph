using Core;
using Core.Enums;

using Resources;
using Resources.Enums;

namespace DX12RenderGraph;

/// <summary>
/// Простой пасс для копирования текстуры
/// </summary>
public class SimpleCopyPass: RenderPass
{
  private ResourceHandle _inputTexture;
  private ResourceHandle _outputTexture;

  public SimpleCopyPass(string name) : base(name)
  {
    Category = PassCategory.Utility;
    Priority = PassPriority.Low;
  }

  public void SetInputTexture(ResourceHandle inputTexture)
  {
    _inputTexture = inputTexture;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    if(!_inputTexture.IsValid())
    {
      Console.WriteLine($"[{Name}] Warning: No input texture, creating dummy");
      _inputTexture = builder.CreateColorTarget("DummyInput", 1, 1, TextureFormat.R8G8B8A8_UNORM);
    }

    builder.ReadTexture(_inputTexture);

    var inputDesc = (TextureDescription)builder.GetResourceDescription(_inputTexture);
    _outputTexture = builder.CreateColorTarget(
        "CopyOutput",
        inputDesc.Width,
        inputDesc.Height,
        inputDesc.Format
    );

    builder.WriteTexture(_outputTexture);
    Console.WriteLine($"[{Name}] Setup: Input={_inputTexture}, Output={_outputTexture}");
  }

  public override void Execute(RenderPassContext context)
  {
    Console.WriteLine($"[{Name}] Copying texture from {_inputTexture} to {_outputTexture}");
  }
}
