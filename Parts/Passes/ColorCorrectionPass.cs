using Core;
using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Passes.Enums;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace Passes;

public class ColorCorrectionPass: RenderPass
{
  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }
  private ResourceHandle _colorCorrectionBuffer;

  // Параметры цветокоррекции
  public float Gamma { get; set; } = 2.2f;
  public float Contrast { get; set; } = 1.0f;
  public float Brightness { get; set; } = 0.0f;
  public float Saturation { get; set; } = 1.0f;
  public Vector3 ColorBalance { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);
  public bool EnableToneMapping { get; set; } = true;
  public ToneMappingType ToneMappingType { get; set; } = ToneMappingType.ACES;
  public float Exposure { get; set; } = 1.0f;

  // Шейдеры и состояния
  private IShader _vertexShader;
  private IShader _pixelShader;
  private ISampler _pointSampler;

  public ColorCorrectionPass() : base("ColorCorrectionPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Low;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    if(!InputTexture.IsValid())
      throw new InvalidOperationException("ColorCorrectionPass requires valid InputTexture");

    // Читаем входную текстуру
    builder.ReadTexture(InputTexture);

    // Получаем описание входной текстуры
    var inputDesc = (TextureDescription)builder.GetResourceDescription(InputTexture);

    // Создаем выходную текстуру
    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "ColorCorrectedOutput";
    outputDesc.TextureUsage = TextureUsage.RenderTarget;
    outputDesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;

    OutputTexture = builder.CreateTexture("ColorCorrectedOutput", outputDesc);
    builder.WriteTexture(OutputTexture);

    // Создаем буфер параметров цветокоррекции
    _colorCorrectionBuffer = builder.CreateConstantBuffer("ColorCorrectionParams", 128);
    builder.WriteBuffer(_colorCorrectionBuffer);

    // Устанавливаем lifetime
    builder.SetResourceLifetime(OutputTexture, ResourceLifetime.Persistent);
  }

  public override void OnGraphCompiled(RenderGraph renderGraph)
  {
    // Создаем шейдеры после компиляции графа
    CreateShaders(renderGraph);
  }

  public override void Execute(RenderPassContext context)
  {
    var commandBuffer = context.CommandBuffer;

    // Получаем ресурсы
    var inputTexture = context.GetTexture(InputTexture);
    var outputTexture = context.GetTexture(OutputTexture);

    // Получаем views
    var inputView = inputTexture.GetDefaultShaderResourceView();
    var outputView = outputTexture.GetDefaultRenderTargetView();

    // Обновляем параметры цветокоррекции
    UpdateColorCorrectionParameters(context);

    // Устанавливаем render target
    commandBuffer.SetRenderTarget(outputView);

    // Устанавливаем viewport
    context.SetFullScreenViewport();

    // Устанавливаем шейдеры
    SetupColorCorrectionShaders(commandBuffer);

    // Устанавливаем входную текстуру
    commandBuffer.SetShaderResource(ShaderStage.Pixel, 0, inputView);

    // Устанавливаем семплер
    commandBuffer.SetSampler(ShaderStage.Pixel, 0, _pointSampler);

    // Устанавливаем параметры
    var colorCorrectionBuffer = context.GetBuffer(_colorCorrectionBuffer);
    var paramsView = colorCorrectionBuffer.GetDefaultShaderResourceView();
    commandBuffer.SetConstantBuffer(ShaderStage.Pixel, 0, paramsView);

    // Рендерим fullscreen quad
    commandBuffer.DrawFullscreenQuad();
  }

  private void CreateShaders(RenderGraph renderGraph)
  {
    var device = renderGraph.GetGraphicsDevice();

    // Vertex shader для fullscreen quad
    var vsDesc = new ShaderDescription
    {
      Name = "ColorCorrectionVertexShader",
      Stage = ShaderStage.Vertex,
      Bytecode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "VSMain"
    };
    _vertexShader = device.CreateShader(vsDesc);

    // Pixel shader для цветокоррекции
    var psDesc = new ShaderDescription
    {
      Name = "ColorCorrectionPixelShader",
      Stage = ShaderStage.Pixel,
      Bytecode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "PSColorCorrection"
    };
    _pixelShader = device.CreateShader(psDesc);

    // Point семплер для точного пиксельного доступа
    var samplerDesc = new SamplerDescription
    {
      Name = "PointSampler",
      MinFilter = FilterMode.Point,
      MagFilter = FilterMode.Point,
      AddressModeU = AddressMode.Clamp,
      AddressModeV = AddressMode.Clamp
    };
    _pointSampler = device.CreateSampler(samplerDesc);
  }

  private void UpdateColorCorrectionParameters(RenderPassContext context)
  {
    var buffer = context.GetBuffer(_colorCorrectionBuffer);

    var colorParams = new ColorCorrectionParameters
    {
      Gamma = Gamma,
      Contrast = Contrast,
      Brightness = Brightness,
      Saturation = Saturation,
      ColorBalance = ColorBalance,
      Exposure = Exposure,
      EnableToneMapping = EnableToneMapping ? 1 : 0,
      ToneMappingType = (int)ToneMappingType
    };

    // Записываем параметры в буфер
    var mappedData = buffer.Map(MapMode.WriteDiscard);
    unsafe
    {
      *(ColorCorrectionParameters*)mappedData = colorParams;
    }
    buffer.Unmap();
  }

  private void SetupColorCorrectionShaders(CommandBuffer commandBuffer)
  {
    commandBuffer.SetVertexShader(_vertexShader);
    commandBuffer.SetPixelShader(_pixelShader);
  }

  public override void Dispose()
  {
    _vertexShader?.Dispose();
    _pixelShader?.Dispose();
    _pointSampler?.Dispose();
    base.Dispose();
  }
}