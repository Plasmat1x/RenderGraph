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

public class BlurPass: RenderPass
{
  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }
  private ResourceHandle _intermediateTexture;
  private ResourceHandle _blurParamsBuffer;

  // Параметры размытия
  public float BlurRadius { get; set; } = 5.0f;
  public float BlurSigma { get; set; } = 2.0f;
  public BlurDirection BlurDirection { get; set; } = BlurDirection.Both;
  public BlurQuality Quality { get; set; } = BlurQuality.High;

  private uint _textureWidth;
  private uint _textureHeight;

  // Шейдеры
  private IShader _vertexShader;
  private IShader _horizontalBlurShader;
  private IShader _verticalBlurShader;
  private ISampler _linearSampler;

  public BlurPass() : base("BlurPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Normal;
  }

  public BlurPass(string _name) : base(_name)
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Normal;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    if(!InputTexture.IsValid())
      throw new InvalidOperationException("BlurPass requires valid InputTexture");

    // Получаем размеры входной текстуры
    var inputDesc = (TextureDescription)builder.GetResourceDescription(InputTexture);
    _textureWidth = inputDesc.Width;
    _textureHeight = inputDesc.Height;

    // Читаем входную текстуру
    builder.ReadTexture(InputTexture);

    // Создаем выходную текстуру того же формата и размера
    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "BlurOutput";
    outputDesc.TextureUsage = TextureUsage.RenderTarget;
    outputDesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;

    OutputTexture = builder.CreateTexture("BlurOutput", outputDesc);
    builder.WriteTexture(OutputTexture);

    // Для двухпроходного размытия создаем промежуточную текстуру
    if(BlurDirection == BlurDirection.Both)
    {
      var intermediateDesc = (TextureDescription)outputDesc.Clone();
      intermediateDesc.Name = "BlurIntermediate";
      _intermediateTexture = builder.CreateTexture("BlurIntermediate", intermediateDesc);
      builder.WriteTexture(_intermediateTexture);
      builder.ReadTexture(_intermediateTexture);
    }

    // Создаем буфер параметров размытия
    _blurParamsBuffer = builder.CreateConstantBuffer("BlurParams", 64);
    builder.WriteBuffer(_blurParamsBuffer);

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

    // Обновляем параметры размытия
    UpdateBlurParameters(context);

    // Выполняем размытие в зависимости от направления
    switch(BlurDirection)
    {
      case BlurDirection.Horizontal:
        PerformBlur(context, InputTexture, OutputTexture, BlurDirection.Horizontal);
        break;

      case BlurDirection.Vertical:
        PerformBlur(context, InputTexture, OutputTexture, BlurDirection.Vertical);
        break;

      case BlurDirection.Both:
        // Два прохода: сначала горизонтальное, затем вертикальное
        PerformBlur(context, InputTexture, _intermediateTexture, BlurDirection.Horizontal);
        PerformBlur(context, _intermediateTexture, OutputTexture, BlurDirection.Vertical);
        break;
    }
  }

  private void CreateShaders(RenderGraph renderGraph)
  {
    // В реальной реализации здесь будут загружены шейдеры из файлов
    // Для демо создаем заглушки

    var device = renderGraph.GetGraphicsDevice(); // Предполагаем что такой метод есть

    // Vertex shader для fullscreen quad
    var vsDesc = new ShaderDescription
    {
      Name = "BlurVertexShader",
      Stage = ShaderStage.Vertex,
      Bytecode = new byte[] { 0x44, 0x58, 0x42, 0x43 }, // Fake DXBC
      EntryPoint = "VSMain"
    };
    _vertexShader = device.CreateShader(vsDesc);

    // Pixel shaders для горизонтального и вертикального размытия
    var hBlurDesc = new ShaderDescription
    {
      Name = "HorizontalBlurShader",
      Stage = ShaderStage.Pixel,
      Bytecode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "PSHorizontalBlur"
    };
    _horizontalBlurShader = device.CreateShader(hBlurDesc);

    var vBlurDesc = new ShaderDescription
    {
      Name = "VerticalBlurShader",
      Stage = ShaderStage.Pixel,
      Bytecode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "PSVerticalBlur"
    };
    _verticalBlurShader = device.CreateShader(vBlurDesc);

    // Линейный семплер
    var samplerDesc = new SamplerDescription
    {
      Name = "LinearSampler",
      MinFilter = FilterMode.Linear,
      MagFilter = FilterMode.Linear,
      AddressModeU = AddressMode.Clamp,
      AddressModeV = AddressMode.Clamp
    };
    _linearSampler = device.CreateSampler(samplerDesc);
  }

  private void UpdateBlurParameters(RenderPassContext context)
  {
    var blurBuffer = context.GetBuffer(_blurParamsBuffer);

    // Вычисляем параметры Gaussian blur
    var kernelSize = GetKernelSize();
    var texelSize = new Vector2(1.0f / _textureWidth, 1.0f / _textureHeight);

    var blurParams = new BlurParameters
    {
      BlurRadius = BlurRadius,
      BlurSigma = BlurSigma,
      TexelSize = texelSize,
      KernelSize = kernelSize
    };

    // Записываем параметры в буфер
    var mappedData = blurBuffer.Map(MapMode.WriteDiscard);
    unsafe
    {
      *(BlurParameters*)mappedData = blurParams;
    }
    blurBuffer.Unmap();
  }

  private void PerformBlur(RenderPassContext context, ResourceHandle input, ResourceHandle output, BlurDirection direction)
  {
    var commandBuffer = context.CommandBuffer;

    // Получаем ресурсы
    var inputTexture = context.GetTexture(input);
    var outputTexture = context.GetTexture(output);
    var blurParamsBuffer = context.GetBuffer(_blurParamsBuffer);

    // Получаем views
    var inputView = inputTexture.GetDefaultShaderResourceView();
    var outputView = outputTexture.GetDefaultRenderTargetView();
    var paramsView = blurParamsBuffer.GetDefaultShaderResourceView();

    // Устанавливаем render target
    commandBuffer.SetRenderTarget(outputView);

    // Устанавливаем viewport
    context.SetFullScreenViewport();

    // Устанавливаем шейдеры для размытия
    SetupBlurShaders(commandBuffer, direction);

    // Устанавливаем входную текстуру
    commandBuffer.SetShaderResource(ShaderStage.Pixel, 0, inputView);

    // Устанавливаем семплер
    commandBuffer.SetSampler(ShaderStage.Pixel, 0, _linearSampler);

    // Устанавливаем параметры размытия
    commandBuffer.SetConstantBuffer(ShaderStage.Pixel, 0, paramsView);

    // Рендерим fullscreen quad
    commandBuffer.DrawFullscreenQuad();
  }

  private void SetupBlurShaders(CommandBuffer commandBuffer, BlurDirection direction)
  {
    commandBuffer.SetVertexShader(_vertexShader);

    switch(direction)
    {
      case BlurDirection.Horizontal:
        commandBuffer.SetPixelShader(_horizontalBlurShader);
        break;
      case BlurDirection.Vertical:
        commandBuffer.SetPixelShader(_verticalBlurShader);
        break;
      default:
        commandBuffer.SetPixelShader(_horizontalBlurShader); // Fallback
        break;
    }
  }

  private int GetKernelSize()
  {
    return Quality switch
    {
      BlurQuality.Low => 3,
      BlurQuality.Medium => 7,
      BlurQuality.High => 15,
      _ => 7
    };
  }

  public override void Dispose()
  {
    _vertexShader?.Dispose();
    _horizontalBlurShader?.Dispose();
    _verticalBlurShader?.Dispose();
    _linearSampler?.Dispose();
    base.Dispose();
  }
}
