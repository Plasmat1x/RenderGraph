using Core;

using GraphicsAPI;
using GraphicsAPI.Enums;

using MockImpl;

using Resources.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

// Главное консольное приложение
public class Program
{
  public static void Main()
  {
    Console.OutputEncoding = Encoding.UTF8;
    Console.WriteLine("=== Render Graph Demo BEGIN ===\n");

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

    Console.WriteLine("\n=== Render Graph Demo END ===\n");
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

    // 4. Добавление проходов в граф
    Console.WriteLine("\n📋 Adding passes to render graph...");
    renderGraph.AddPass(geometryPass);
    renderGraph.AddPass(blurPass);
    renderGraph.AddPass(colorCorrectionPass);

    // 5. Установка зависимостей между проходами
    Console.WriteLine("\n🔗 Setting up pass dependencies...");
    blurPass.AddDependency(geometryPass);
    colorCorrectionPass.AddDependency(blurPass);

    Console.WriteLine($"   Dependencies: {geometryPass.Name} -> {blurPass.Name} -> {colorCorrectionPass.Name}");

    Console.WriteLine("\n🔧 Setting up resource assignment callbacks...");

    geometryPass.OnPassSetup += (pass) =>
    {
      Console.WriteLine("  🔗 Assigning GeometryPass.ColorTarget to BlurPass.InputTexture");
      blurPass.InputTexture = geometryPass.ColorTarget;
    };

    blurPass.OnPassSetup += (pass) =>
    {
      Console.WriteLine("  🔗 Assigning BlurPass.OutputTexture to ColorCorrectionPass.InputTexture");
      colorCorrectionPass.InputTexture = blurPass.OutputTexture;
    };

    // 6. Компиляция графа (проходы автоматически найдут ресурсы по именам)
    Console.WriteLine("\n⚙️  Compiling render graph...");
    renderGraph.Compile();

    // 7. Показываем порядок выполнения
    var executionOrder = renderGraph.GetExecutionOrder();
    Console.WriteLine($"\n📝 Execution order ({executionOrder.Count} passes):");
    for(int i = 0; i < executionOrder.Count; i++)
    {
      var pass = executionOrder[i];
      Console.WriteLine($"  {i + 1}. {pass.Name}");
      Console.WriteLine($"     Category: {pass.Category}, Priority: {pass.Priority}");
      Console.WriteLine($"     Inputs: {pass.Inputs.Count}, Outputs: {pass.Outputs.Count}");

      // Показываем зависимости
      if(pass.Dependencies.Count > 0)
      {
        var deps = string.Join(", ", pass.Dependencies.Select(d => d.Name));
        Console.WriteLine($"     Dependencies: {deps}");
      }
    }

    // 8. Показываем начальное использование памяти
    Console.WriteLine("\n💾 Initial memory usage:");
    ShowMemoryUsage(renderGraph);

    // 9. Симуляция рендер лупа
    Console.WriteLine("\n🔄 Starting render loop...");

    for(int frame = 0; frame < 3; frame++)
    {
      Console.WriteLine($"\n--- Frame {frame + 1} ---");

      // Обновляем данные кадра
      renderGraph.UpdateFrameData(0.016f, 1920, 1080);

      // Устанавливаем камеру для демонстрации
      var viewMatrix = Matrix4x4.CreateLookAt(
          new Vector3(0, 0, 10),  // position
          new Vector3(0, 0, 0),   // target
          new Vector3(0, 1, 0)    // up
      );
      var projMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
          MathF.PI / 4.0f,        // 45 degrees
          1920.0f / 1080.0f,      // aspect ratio
          0.1f,                   // near
          100.0f                  // far
      );

      renderGraph.SetViewMatrix(viewMatrix);
      renderGraph.SetProjectionMatrix(projMatrix);
      renderGraph.SetCameraPosition(new Vector3(0, 0, 10));

      // Выполняем граф
      using var commandBuffer = device.CreateCommandBuffer();
      renderGraph.Execute(commandBuffer);
      device.ExecuteCommandBuffer(commandBuffer);

      // Показываем статистику
      ShowPassStatistics(renderGraph);

      Thread.Sleep(100); // Пауза между кадрами для наглядности
    }

    // 10. Финальная статистика
    Console.WriteLine("\n📈 Final Statistics:");
    ShowMemoryUsage(renderGraph);
    ShowRenderGraphStatistics(renderGraph);
    ShowDeviceCapabilities(device);

    // 11. Демонстрация дополнительных возможностей
    Console.WriteLine("\n🔧 Additional Features Demo:");
    DemonstrateAdditionalFeatures(device, renderGraph);

    // 12. Проверка валидации
    Console.WriteLine("\n✅ Validation:");
    TestRenderGraphValidation(device);
  }

  private static void ShowPassStatistics(RenderGraph renderGraph)
  {
    Console.WriteLine("  📊 Pass Statistics:");
    foreach(var pass in renderGraph.Passes)
    {
      var stats = pass.Statistics;
      var status = pass.Enabled ? "✅" : "❌";
      Console.WriteLine($"    {status} {pass.Name}: " +
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

    if(memInfo.TotalAllocated > 0)
    {
      var textureRatio = (float)memInfo.TextureMemory / memInfo.TotalAllocated * 100f;
      var bufferRatio = (float)memInfo.BufferMemory / memInfo.TotalAllocated * 100f;
      Console.WriteLine($"      Breakdown: Textures {textureRatio:F1}%, Buffers {bufferRatio:F1}%");
    }
  }

  private static void ShowRenderGraphStatistics(RenderGraph renderGraph)
  {
    var stats = renderGraph.GetStatistics();
    Console.WriteLine($"  📊 Render Graph Statistics:");
    Console.WriteLine($"      Total Passes: {stats.TotalPasses}");
    Console.WriteLine($"      Enabled: {stats.EnabledPasses}, Disabled: {stats.DisabledPasses}");
    Console.WriteLine($"      Pass Utilization: {stats.PassUtilization:P1}");
    Console.WriteLine($"      Compiled: {stats.IsCompiled}");
    Console.WriteLine($"      Last Frame: {stats.LastFrameIndex}");
  }

  private static void ShowDeviceCapabilities(MockGraphicsDevice device)
  {
    var caps = device.Capabilities;
    Console.WriteLine($"  🔧 Device Capabilities:");
    Console.WriteLine($"      Max Texture 2D: {caps.MaxTexture2DSize}x{caps.MaxTexture2DSize}");
    Console.WriteLine($"      Max Texture 3D: {caps.MaxTexture3DSize}x{caps.MaxTexture3DSize}x{caps.MaxTexture3DSize}");
    Console.WriteLine($"      Max Color Attachments: {caps.MaxColorAttachments}");
    Console.WriteLine($"      Max Vertex Attributes: {caps.MaxVertexAttributes}");
    Console.WriteLine($"      Geometry Shader: {caps.SupportsGeometryShader}");
    Console.WriteLine($"      Compute Shader: {caps.SupportsComputeShader}");
    Console.WriteLine($"      Tessellation: {caps.SupportsTessellation}");
    Console.WriteLine($"      Multi-Draw Indirect: {caps.SupportsMultiDrawIndirect}");
    Console.WriteLine($"      BC Compression: {caps.SupportsTextureCompressionBC}");
    Console.WriteLine($"      ETC Compression: {caps.SupportsTextureCompressionETC}");
    Console.WriteLine($"      ASTC Compression: {caps.SupportsTextureCompressionASTC}");
  }

  private static void DemonstrateAdditionalFeatures(MockGraphicsDevice device, RenderGraph renderGraph)
  {
    Console.WriteLine("  🎮 Creating additional resources...");

    // Создание различных шейдеров
    var shaderTypes = new[] { ShaderStage.Vertex, ShaderStage.Pixel, ShaderStage.Compute, ShaderStage.Geometry };
    foreach(var stage in shaderTypes)
    {
      var shaderDesc = new ShaderDescription
      {
        Name = $"Demo{stage}Shader",
        Stage = stage,
        Bytecode = new byte[] { 0x44, 0x58, 0x42, 0x43, 0x01, 0x02, 0x03, 0x04 }, // Fake DXBC
        EntryPoint = stage == ShaderStage.Vertex ? "VSMain" :
                     stage == ShaderStage.Pixel ? "PSMain" :
                     stage == ShaderStage.Compute ? "CSMain" : "GSMain"
      };
      using var shader = device.CreateShader(shaderDesc);

      // Демонстрация рефлексии
      var reflection = shader.GetReflection();
      Console.WriteLine($"      {stage} Shader: {reflection.ConstantBuffers.Count} CBs, {reflection.Resources.Count} resources");
    }

    // Создание различных семплеров
    var samplerConfigs = new[]
    {
            ("Linear", FilterMode.Linear, AddressMode.Wrap),
            ("Point", FilterMode.Point, AddressMode.Clamp),
            ("Anisotropic", FilterMode.Anisotropic, AddressMode.Mirror)
        };

    foreach(var (name, filter, address) in samplerConfigs)
    {
      var samplerDesc = new SamplerDescription
      {
        Name = $"{name}Sampler",
        MinFilter = filter,
        MagFilter = filter,
        AddressModeU = address,
        AddressModeV = address,
        MaxAnisotropy = filter == FilterMode.Anisotropic ? 16u : 1u
      };
      using var sampler = device.CreateSampler(samplerDesc);
    }

    // Создание render states
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
                        BlendOp = BlendOperation.Add,
                        WriteMask = ColorWriteMask.All
                    }
            }
      },
      DepthStencilState = new DepthStencilStateDescription
      {
        DepthEnable = true,
        DepthWriteEnable = true,
        DepthFunction = ComparisonFunction.Less,
        StencilEnable = false
      },
      RasterizerState = new RasterizerStateDescription
      {
        FillMode = FillMode.Solid,
        CullMode = CullMode.Back,
        FrontCounterClockwise = false,
        DepthClipEnable = true,
        ScissorEnable = false,
        MultisampleEnable = false
      }
    };
    using var renderState = device.CreateRenderState(renderStateDesc);

    // Демонстрация fence синхронизации
    Console.WriteLine("  🔄 Testing GPU synchronization...");
    using var fence = device.CreateFence(0);
    fence.Signal(100);
    Console.WriteLine($"      Fence signaled with value: {fence.Value}");
    fence.Wait(100, 1000); // Wait max 1 second
    Console.WriteLine($"      Fence wait completed, is signaled: {fence.IsSignaled}");

    // Информация о памяти устройства
    var memInfo = device.GetMemoryInfo();
    Console.WriteLine($"  💾 Device Memory Information:");
    Console.WriteLine($"      Total: {memInfo.TotalMemory / (1024 * 1024 * 1024)} GB");
    Console.WriteLine($"      Available: {memInfo.AvailableMemory / (1024 * 1024 * 1024)} GB");
    Console.WriteLine($"      Used: {memInfo.UsedMemory / (1024 * 1024 * 1024)} GB");
    Console.WriteLine($"      Budget: {memInfo.Budget / (1024 * 1024 * 1024)} GB");
    Console.WriteLine($"      Current Usage: {memInfo.CurrentUsage / (1024 * 1024)} MB");

    // Проверка поддержки форматов
    Console.WriteLine("  🎨 Format Support Check:");
    var formatsToTest = new[]
    {
            TextureFormat.R8G8B8A8_UNORM,
            TextureFormat.R16G16B16A16_FLOAT,
            TextureFormat.R32G32B32A32_FLOAT,
            TextureFormat.BC1_UNORM,
            TextureFormat.BC3_UNORM,
            TextureFormat.BC7_UNORM,
            TextureFormat.D32_FLOAT,
            TextureFormat.D24_UNORM_S8_UINT
        };

    foreach(var format in formatsToTest)
    {
      var rtSupport = device.SupportsFormat(format, FormatUsage.RenderTarget);
      var dsSupport = device.SupportsFormat(format, FormatUsage.DepthStencil);
      var srSupport = device.SupportsFormat(format, FormatUsage.ShaderResource);
      var uaSupport = device.SupportsFormat(format, FormatUsage.UnorderedAccess);
      var bpp = device.GetFormatBytesPerPixel(format);
      var samples = device.GetSupportedSampleCounts(format);

      Console.WriteLine($"      {format}:");
      Console.WriteLine($"        BPP: {bpp}, RT: {rtSupport}, DS: {dsSupport}, SR: {srSupport}, UA: {uaSupport}");
      Console.WriteLine($"        Sample Counts: {samples}");
    }

    Console.WriteLine("  ✅ Additional features demonstration completed!");
  }

  private static void TestRenderGraphValidation(MockGraphicsDevice device)
  {
    Console.WriteLine("  🧪 Testing render graph validation...");

    try
    {
      using var testGraph = new RenderGraph(device);

      // Тест 1: Пустой граф
      Console.WriteLine("      Test 1: Empty graph compilation...");
      testGraph.Compile();
      Console.WriteLine("      ✅ Empty graph compiled successfully");

      // Тест 2: Граф с одним проходом
      Console.WriteLine("      Test 2: Single pass graph...");
      var singlePass = new DemoGeometryPass();
      testGraph.AddPass(singlePass);
      testGraph.Compile();
      Console.WriteLine("      ✅ Single pass graph compiled successfully");

      // Тест 3: Проверка валидации прохода
      Console.WriteLine("      Test 3: Pass validation...");
      if(singlePass.Validate(out string validationError))
      {
        Console.WriteLine("      ✅ Pass validation successful");
      }
      else
      {
        Console.WriteLine($"      ❌ Pass validation failed: {validationError}");
      }

      // Тест 4: Статистика пустого выполнения
      Console.WriteLine("      Test 4: Empty execution statistics...");
      var stats = testGraph.GetStatistics();
      Console.WriteLine($"      Pass utilization: {stats.PassUtilization:P1}");
      Console.WriteLine($"      Memory usage: {testGraph.GetMemoryUsage().GetFormattedSize(testGraph.GetMemoryUsage().TotalAllocated)}");

      Console.WriteLine("  ✅ All validation tests passed!");
    }
    catch(Exception ex)
    {
      Console.WriteLine($"  ❌ Validation test failed: {ex.Message}");
    }
  }
}