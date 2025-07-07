using Core;

using GraphicsAPI;
using GraphicsAPI.Enums;

using MockImpl;

using Resources.Enums;

public class Program
{
  public static void Main()
  {
    Console.WriteLine("=== Render Graph Demo ===\n");

    try
    {
      RunRenderGraphDemo();
    }
    catch(Exception ex)
    {
      Console.WriteLine($"\n❌ Error: {ex.Message}");
      if(ex.InnerException != null)
      {
        Console.WriteLine($"Inner: {ex.InnerException.Message}");
      }
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
  }

  private static void RunRenderGraphDemo()
  {
    // 1. Создание устройства
    Console.WriteLine("🖥️  Creating graphics device...");
    using var device = new MockGraphicsDevice();
    Console.WriteLine($"   Device: {device.Name} ({device.API})");
    Console.WriteLine($"   Max Texture Size: {device.Capabilities.MaxTexture2DSize}x{device.Capabilities.MaxTexture2DSize}");
    Console.WriteLine($"   Supported Sample Counts: {device.Capabilities.SupportedSampleCounts}");

    // 2. Создание render graph
    Console.WriteLine("\n📊 Creating render graph...");
    using var renderGraph = new RenderGraph(device);

    // 3. Создание проходов
    Console.WriteLine("\n🎨 Creating render passes...");
    var geometryPass = new DemoGeometryPass();
    var blurPass = new DemoBlurPass { BlurRadius = 8.0f };
    var colorCorrectionPass = new DemoColorCorrectionPass { Gamma = 2.2f, Contrast = 1.15f };

    // 4. Настройка связей между проходами
    Console.WriteLine("\n🔗 Setting up pass connections...");
    blurPass.InputTexture = geometryPass.ColorTarget;
    colorCorrectionPass.InputTexture = blurPass.OutputTexture;

    // Показываем зависимости
    Console.WriteLine($"   {geometryPass.Name} -> {blurPass.Name} -> {colorCorrectionPass.Name}");

    // 5. Добавление проходов в граф
    Console.WriteLine("\n📋 Adding passes to render graph...");
    renderGraph.AddPass(geometryPass);
    renderGraph.AddPass(blurPass);
    renderGraph.AddPass(colorCorrectionPass);

    // 6. Компиляция графа
    Console.WriteLine("\n⚙️  Compiling render graph...");
    renderGraph.Compile();

    // Показываем порядок выполнения
    var executionOrder = renderGraph.GetExecutionOrder();
    Console.WriteLine($"\n📝 Execution order ({executionOrder.Count} passes):");
    for(int i = 0; i < executionOrder.Count; i++)
    {
      var pass = executionOrder[i];
      Console.WriteLine($"  {i + 1}. {pass.Name}");
      Console.WriteLine($"     Category: {pass.Category}, Priority: {pass.Priority}");
      Console.WriteLine($"     Inputs: {pass.Inputs.Count}, Outputs: {pass.Outputs.Count}");
    }

    // Показываем память
    Console.WriteLine("\n💾 Initial memory usage:");
    ShowMemoryUsage(renderGraph);

    // 7. Симуляция рендер лупа
    Console.WriteLine("\n🔄 Starting render loop...");

    for(int frame = 0; frame < 3; frame++)
    {
      Console.WriteLine($"\n--- Frame {frame + 1} ---");

      // Обновляем данные кадра
      renderGraph.UpdateFrameData(0.016f, 1920, 1080);

      // Выполняем граф
      using var commandBuffer = device.CreateCommandBuffer();
      renderGraph.Execute(commandBuffer);
      device.ExecuteCommandBuffer(commandBuffer);

      // Показываем статистику
      ShowPassStatistics(renderGraph);

      Thread.Sleep(50); // Пауза между кадрами
    }

    // 8. Финальная статистика
    Console.WriteLine("\n📈 Final Statistics:");
    ShowMemoryUsage(renderGraph);
    ShowRenderGraphStatistics(renderGraph);
    ShowDeviceCapabilities(device);

    // 9. Демонстрация дополнительных возможностей
    Console.WriteLine("\n🔧 Additional Features Demo:");
    DemonstrateAdditionalFeatures(device, renderGraph);
  }

  private static void ShowPassStatistics(RenderGraph renderGraph)
  {
    Console.WriteLine("  📊 Pass Statistics:");
    foreach(var pass in renderGraph.Passes)
    {
      var stats = pass.Statistics;
      Console.WriteLine($"    {pass.Name}: " +
                      $"Setup: {stats.LastSetupTime.TotalMilliseconds:F1}ms, " +
                      $"Execute: {stats.LastExecutionTime.TotalMilliseconds:F1}ms, " +
                      $"Executed: {stats.WasExecutedThisFrame}, " +
                      $"Errors: {stats.ErrorCount}");
    }
  }

  private static void ShowMemoryUsage(RenderGraph renderGraph)
  {
    var memInfo = renderGraph.GetMemoryUsage();
    Console.WriteLine($"  💾 Memory Usage: {memInfo}");
  }

  private static void ShowRenderGraphStatistics(RenderGraph renderGraph)
  {
    var stats = renderGraph.GetStatistics();
    Console.WriteLine($"  📊 Render Graph: {stats.TotalPasses} passes, " +
                    $"{stats.EnabledPasses} enabled, " +
                    $"Utilization: {stats.PassUtilization:P1}, " +
                    $"Compiled: {stats.IsCompiled}, " +
                    $"Frame: {stats.LastFrameIndex}");
  }

  private static void ShowDeviceCapabilities(MockGraphicsDevice device)
  {
    var caps = device.Capabilities;
    Console.WriteLine($"  🔧 Device Capabilities:");
    Console.WriteLine($"    Max Texture 2D: {caps.MaxTexture2DSize}x{caps.MaxTexture2DSize}");
    Console.WriteLine($"    Max Color Attachments: {caps.MaxColorAttachments}");
    Console.WriteLine($"    Geometry Shader: {caps.SupportsGeometryShader}");
    Console.WriteLine($"    Compute Shader: {caps.SupportsComputeShader}");
    Console.WriteLine($"    Tessellation: {caps.SupportsTessellation}");
    Console.WriteLine($"    BC Compression: {caps.SupportsTextureCompressionBC}");
  }

  private static void DemonstrateAdditionalFeatures(MockGraphicsDevice device, RenderGraph renderGraph)
  {
    // Демонстрация создания различных ресурсов
    Console.WriteLine("  Creating additional resources...");

    // Создание шейдера
    var vertexShaderDesc = new ShaderDescription
    {
      Name = "DemoVertexShader",
      Stage = ShaderStage.Vertex,
      Bytecode = new byte[] { 0x44, 0x58, 0x42, 0x43 }, // DXBC signature
      EntryPoint = "VSMain"
    };
    using var vertexShader = device.CreateShader(vertexShaderDesc);

    // Создание семплера
    var samplerDesc = new SamplerDescription
    {
      Name = "LinearSampler",
      MinFilter = FilterMode.Linear,
      MagFilter = FilterMode.Linear,
      AddressModeU = AddressMode.Wrap,
      AddressModeV = AddressMode.Wrap,
      MaxAnisotropy = 16
    };
    using var sampler = device.CreateSampler(samplerDesc);

    // Создание render state
    var renderStateDesc = new RenderStateDescription
    {
      BlendState = new BlendStateDescription
      {
        RenderTargets = new RenderTargetBlendDescription[1]
            {
                    new RenderTargetBlendDescription
                    {
                        BlendEnable = true,
                        SrcBlend = BlendFactor.SrcAlpha,
                        DstBlend = BlendFactor.InvSrcAlpha,
                        BlendOp = BlendOperation.Add
                    }
            }
      },
      DepthStencilState = new DepthStencilStateDescription
      {
        DepthEnable = true,
        DepthWriteEnable = true,
        DepthFunction = ComparisonFunction.Less
      },
      RasterizerState = new RasterizerStateDescription
      {
        FillMode = FillMode.Solid,
        CullMode = CullMode.Back,
        FrontCounterClockwise = false
      }
    };
    using var renderState = device.CreateRenderState(renderStateDesc);

    // Демонстрация fence
    using var fence = device.CreateFence(0);
    fence.Signal(42);
    fence.Wait(42);

    // Информация о памяти
    var memInfo = device.GetMemoryInfo();
    Console.WriteLine($"  Device Memory: Total: {memInfo.TotalMemory / (1024 * 1024 * 1024)} GB, " +
                    $"Available: {memInfo.AvailableMemory / (1024 * 1024 * 1024)} GB, " +
                    $"Used: {memInfo.UsedMemory / (1024 * 1024 * 1024)} GB");

    // Проверка поддержки форматов
    var formats = new[] { TextureFormat.R8G8B8A8_UNORM, TextureFormat.BC3_UNORM, TextureFormat.D32_FLOAT };
    foreach(var format in formats)
    {
      var rtSupport = device.SupportsFormat(format, FormatUsage.RenderTarget);
      var dsSupport = device.SupportsFormat(format, FormatUsage.DepthStencil);
      Console.WriteLine($"  Format {format}: RT={rtSupport}, DS={dsSupport}, BPP={device.GetFormatBytesPerPixel(format)}");
    }

    Console.WriteLine("  ✅ Additional features demonstration completed!");
  }
}