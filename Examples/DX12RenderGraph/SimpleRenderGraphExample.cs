using Core;

using Directx12Impl;

using GraphicsAPI.Enums;

using Passes;
using Passes.Enums;

using Resources.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DX12RenderGraph;
/// <summary>
/// Упрощенный пример RenderGraph + DirectX12 без UI окна.
/// Демонстрирует основные принципы работы с RenderGraph и готовыми пассами.
/// </summary>
public class SimpleRenderGraphExample: IDisposable
{

  private DX12GraphicsDevice _device;
  private RenderGraph _renderGraph;

  // Пассы из пакета Passes
  private GeometryPass _geometryPass;
  private BlurPass _blurPass;
  private SimpleRenderToTexturePass _renderToTexturePass;
  private SimpleCopyPass _copyPass;

  private readonly uint _renderWidth = 1280;
  private readonly uint _renderHeight = 720;

  public SimpleRenderGraphExample()
  {
    InitializeDevice();
    InitializeRenderGraph();
    SetupSimplePipeline();
  }

  private void InitializeDevice()
  {
    Console.WriteLine("🚀 Initializing DirectX12 Device...");
    _device = new DX12GraphicsDevice(false);

    Console.WriteLine($"✅ Device: {_device.Name} ({_device.API})");
    Console.WriteLine($"   Max Texture Size: {_device.Capabilities.MaxTexture2DSize}");
  }

  private void InitializeRenderGraph()
  {
    Console.WriteLine("📊 Creating Simple Render Graph...");
    _renderGraph = new RenderGraph(_device);
  }

  /// <summary>
  /// Настраивает простой render pipeline без зависимости от swap chain
  /// </summary>
  private void SetupSimplePipeline()
  {
    Console.WriteLine("🎨 Setting up simple render pipeline...");

    CreateSimplePasses();
    SetupSimpleDependencies();
    AddPassesToGraph();

    Console.WriteLine("✅ Simple render pipeline configured");
  }

  /// <summary>
  /// Создает простые render passes
  /// </summary>
  private void CreateSimplePasses()
  {
    Console.WriteLine("🔧 Creating simple render passes...");

    _renderToTexturePass = new SimpleRenderToTexturePass("RenderToTexture")
    {
      OutputWidth = _renderWidth,
      OutputHeight = _renderHeight,
      OutputFormat = TextureFormat.R8G8B8A8_UNORM
    };

    _geometryPass = new GeometryPass
    {
      ViewportWidth = _renderWidth,
      ViewportHeight = _renderHeight,
      ClearColor = true,
      ClearDepth = true,
      ClearColorValue = new Vector4(0.2f, 0.3f, 0.6f, 1.0f),
      ClearDepthValue = 1.0f
    };

    _blurPass = new BlurPass
    {
      BlurRadius = 5.0f,
      BlurSigma = 2.0f,
      Quality = BlurQuality.Medium,
      BlurDirection = BlurDirection.Both
    };

    _copyPass = new SimpleCopyPass("FinalCopy");

    Console.WriteLine($"✅ Created 4 simple render passes");
  }

  /// <summary>
  /// Настраивает простые зависимости между пассами
  /// </summary>
  private void SetupSimpleDependencies()
  {
    Console.WriteLine("🔗 Setting up simple dependencies...");

    _geometryPass.AddDependency(_renderToTexturePass);
    _blurPass.AddDependency(_geometryPass);
    _copyPass.AddDependency(_blurPass);

    SetupSimpleResourceConnections();

    Console.WriteLine("✅ Simple dependencies configured");
  }

  /// <summary>
  /// Настраивает связи ресурсов между пассами
  /// </summary>
  private void SetupSimpleResourceConnections()
  {
    _renderToTexturePass.OnPassSetup += (pass) =>
    {
      Console.WriteLine("🔗 Connecting RenderToTexture.OutputTexture -> GeometryPass.RenderTarget");
    };

    _geometryPass.OnPassSetup += (pass) =>
    {
      Console.WriteLine("🔗 Connecting GeometryPass.ColorTarget -> BlurPass.InputTexture");
      _blurPass.InputTexture = _geometryPass.ColorTarget;
    };

    _blurPass.OnPassSetup += (pass) =>
    {
      Console.WriteLine("🔗 Connecting BlurPass.OutputTexture -> CopyPass.InputTexture");
      _copyPass.SetInputTexture(_blurPass.OutputTexture);
    };
  }

  /// <summary>
  /// Добавляет все пассы в render graph
  /// </summary>
  private void AddPassesToGraph()
  {
    Console.WriteLine("📋 Adding passes to simple render graph...");

    _renderGraph.AddPass(_renderToTexturePass);
    _renderGraph.AddPass(_geometryPass);
    _renderGraph.AddPass(_blurPass);
    _renderGraph.AddPass(_copyPass);

    Console.WriteLine($"✅ Added {_renderGraph.Passes.Count} passes to render graph");
  }

  /// <summary>
  /// Компилирует и выполняет render graph
  /// </summary>
  public void ExecuteSimplePipeline()
  {
    Console.WriteLine("⚙️ Compiling simple render graph...");

    try
    {
      _renderGraph.Compile();
      Console.WriteLine("✅ Simple render graph compiled successfully!");

      DisplayExecutionOrder();

      ExecuteFrames(3);
    }
    catch(Exception ex)
    {
      Console.WriteLine($"❌ Error during pipeline execution: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
  }

  /// <summary>
  /// Показывает порядок выполнения пассов
  /// </summary>
  private void DisplayExecutionOrder()
  {
    var executionOrder = _renderGraph.ExecutionOrder;
    Console.WriteLine($"\n📝 Simple Execution Order ({executionOrder.Count} passes):");

    for(int i = 0; i < executionOrder.Count; i++)
    {
      var pass = executionOrder[i];
      Console.WriteLine($"   {i + 1}. {pass.Name} (Category: {pass.Category}, Priority: {pass.Priority})");
    }
    Console.WriteLine();
  }

  /// <summary>
  /// Выполняет несколько кадров рендеринга
  /// </summary>
  private void ExecuteFrames(int frameCount)
  {
    Console.WriteLine($"🎬 Executing {frameCount} simple frames...");

    for(int frame = 0; frame < frameCount; frame++)
    {
      Console.WriteLine($"\n--- Simple Frame {frame + 1} ---");

      UpdateSimpleFrameData(frame);

      using var commandBuffer = _device.CreateCommandBuffer(
          CommandBufferType.Direct,
          CommandBufferExecutionMode.Immediate
      );

      _renderGraph.Execute(commandBuffer);

      Console.WriteLine($"✅ Simple frame {frame + 1} completed");
    }

    Console.WriteLine("\n🎉 All simple frames executed successfully!");
  }

  /// <summary>
  /// Обновляет данные кадра
  /// </summary>
  private void UpdateSimpleFrameData(int frameIndex)
  {
    var frameData = _renderGraph.FrameData;

    frameData.FrameIndex = (uint)frameIndex;
    frameData.DeltaTime = 1.0f / 60.0f;
    frameData.TotalTime = frameIndex * frameData.DeltaTime;
    frameData.ScreenWidth = _renderWidth;
    frameData.ScreenHeight = _renderHeight;

    var time = frameData.TotalTime;
    _blurPass.BlurRadius = 3.0f + 2.0f * MathF.Sin(time * 0.5f);

    Console.WriteLine($"   Frame {frameIndex}: BlurRadius = {_blurPass.BlurRadius:F2}");
  }

  /// <summary>
  /// Выводит статистику выполнения
  /// </summary>
  public void PrintStatistics()
  {
    Console.WriteLine("\n📊 Simple Pipeline Statistics:");

    foreach(var pass in _renderGraph.Passes)
    {
      var stats = pass.Statistics;
      Console.WriteLine($"\n🎯 {pass.Name}:");
      Console.WriteLine($"   Setup Time: {stats.LastSetupTime:F2}ms");
      Console.WriteLine($"   Execution Time: {stats.LastExecutionTime:F2}ms");
      //Console.WriteLine($"   Total Executions: {stats.TotalExecutions}");

      if(stats.ErrorCount > 0)
      {
        Console.WriteLine($"   ⚠️ Errors: {stats.ErrorCount}");
      }
    }

    Console.WriteLine($"\n🎯 Render Graph Summary:");
    Console.WriteLine($"   Total Passes: {_renderGraph.Passes.Count}");
    Console.WriteLine($"   Compiled: {_renderGraph.IsCompilded}");
    Console.WriteLine($"   Resource Manager Active: {_renderGraph.ResourceManager != null}");
  }

  public void Dispose()
  {
    Console.WriteLine("🧹 Disposing simple render graph example...");

    _renderGraph?.Dispose();
    _device?.Dispose();

    Console.WriteLine("✅ Simple example disposed successfully");
  }
}
