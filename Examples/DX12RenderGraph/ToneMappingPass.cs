using Core;
using Core.Enums;
/// <summary>
/// Пример кастомного пасса для tone mapping
/// Демонстрирует создание собственных пассов, наследующихся от RenderPass
/// </summary>
public class ToneMappingPass: RenderPass
{
  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTarget { get; private set; }

  public float Exposure { get; set; } = 1.0f;
  public float Gamma { get; set; } = 2.2f;
  public TonemappingMode TonemappingMode { get; set; } = TonemappingMode.ACES;

  public ToneMappingPass(string name) : base(name)
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Low;
  }

  public void SetOutputTarget(ResourceHandle outputTarget)
  {
    OutputTarget = outputTarget;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    if(!InputTexture.IsValid())
      throw new InvalidOperationException("ToneMappingPass requires valid InputTexture");

    if(!OutputTarget.IsValid())
      throw new InvalidOperationException("ToneMappingPass requires valid OutputTarget");

    builder.ReadTexture(InputTexture);

    builder.WriteTexture(OutputTarget);

    Console.WriteLine($"[{Name}] Setup completed - Input: {InputTexture}, Output: {OutputTarget}");
  }

  public override void Execute(RenderPassContext context)
  {
    Console.WriteLine($"[{Name}] Executing tone mapping pass...");

    // В реальной реализации здесь был бы:
    // 1. Установка render target
    // 2. Загрузка и применение tone mapping шейдера
    // 3. Установка параметров (exposure, gamma, etc.)
    // 4. Рендеринг full-screen quad с tone mapping шейдером

    // Для примера просто логируем выполнение
    Console.WriteLine($"[{Name}] Applied {TonemappingMode} tone mapping with exposure={Exposure}, gamma={Gamma}");
  }
}