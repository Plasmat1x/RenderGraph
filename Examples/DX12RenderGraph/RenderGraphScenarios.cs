using Core;

using Directx12Impl;

using Passes;
using Passes.Enums;

using System.Numerics;

namespace DX12RenderGraph;

/// <summary>
/// Демонстрирует различные сценарии использования RenderGraph
/// </summary>
public class RenderGraphScenarios
{
  /// <summary>
  /// Сценарий 1: Простейший pipeline с одним пассом
  /// </summary>
  public static void RunSinglePassScenario()
  {
    Console.WriteLine("\n=== Scenario 1: Single Pass ===");

    using var device = new DX12GraphicsDevice(true);
    using var renderGraph = new RenderGraph(device);

    var singlePass = new SimpleRenderToTexturePass("SinglePass")
    {
      OutputWidth = 1920,
      OutputHeight = 1080
    };

    renderGraph.AddPass(singlePass);
    renderGraph.Compile();

    using var cmd = device.CreateCommandBuffer();
    renderGraph.Execute(cmd);
    var stats = renderGraph.GetStatistics();
    Console.WriteLine($"      Pass utilization: {stats.PassUtilization:P1}");
    Console.WriteLine($"      Memory usage: {renderGraph.GetMemoryUsage().GetFormattedSize(renderGraph.GetMemoryUsage().TotalAllocated)}");

    Console.WriteLine("✅ Single pass scenario completed");
  }

  /// <summary>
  /// Сценарий 2: Линейный pipeline с несколькими пассами
  /// </summary>
  public static void RunLinearPipelineScenario()
  {
    Console.WriteLine("\n=== Scenario 2: Linear Pipeline ===");

    using var device = new DX12GraphicsDevice(true);
    using var renderGraph = new RenderGraph(device);

    var pass1 = new SimpleRenderToTexturePass("Pass1");
    var pass2 = new SimpleCopyPass("Pass2");
    var pass3 = new SimpleCopyPass("Pass3");

    pass2.AddDependency(pass1);
    pass3.AddDependency(pass2);

    pass1.OnPassSetup += (_) => pass2.SetInputTexture(pass1.Outputs.First());
    pass2.OnPassSetup += (_) => pass3.SetInputTexture(pass2.Outputs.First());

    renderGraph.AddPass(pass1);
    renderGraph.AddPass(pass2);
    renderGraph.AddPass(pass3);
    renderGraph.Compile();

    using var cmd = device.CreateCommandBuffer();
    renderGraph.Execute(cmd);

    var stats = renderGraph.GetStatistics();
    Console.WriteLine($"      Pass utilization: {stats.PassUtilization:P1}");
    Console.WriteLine($"      Memory usage: {renderGraph.GetMemoryUsage().GetFormattedSize(renderGraph.GetMemoryUsage().TotalAllocated)}");

    Console.WriteLine("✅ Linear pipeline scenario completed");
  }

  /// <summary>
  /// Сценарий 3: Использование готовых пассов из Passes
  /// </summary>
  public static void RunPassesPackageScenario()
  {
    Console.WriteLine("\n=== Scenario 3: Using Passes Package ===");

    using var device = new DX12GraphicsDevice(true);
    using var renderGraph = new RenderGraph(device);

    var geometryPass = new GeometryPass
    {
      ViewportWidth = 1920,
      ViewportHeight = 1080,
      ClearColor = true,
      ClearColorValue = new Vector4(0.1f, 0.1f, 0.2f, 1.0f)
    };

    var blurPass = new BlurPass
    {
      BlurRadius = 10.0f,
      Quality = BlurQuality.High
    };

    blurPass.AddDependency(geometryPass);
    geometryPass.OnPassSetup += (_) => blurPass.InputTexture = geometryPass.ColorTarget;

    renderGraph.AddPass(geometryPass);
    renderGraph.AddPass(blurPass);
    renderGraph.Compile();

    using var cmd = device.CreateCommandBuffer();
    renderGraph.Execute(cmd);

    var stats = renderGraph.GetStatistics();
    Console.WriteLine($"      Pass utilization: {stats.PassUtilization:P1}");
    Console.WriteLine($"      Memory usage: {renderGraph.GetMemoryUsage().GetFormattedSize(renderGraph.GetMemoryUsage().TotalAllocated)}");

    Console.WriteLine($"GeometryPass inputs: {geometryPass.Inputs.Count}, outputs: {geometryPass.Outputs.Count}");
    Console.WriteLine($"BlurPass inputs: {blurPass.Inputs.Count}, outputs: {blurPass.Outputs.Count}");

    Console.WriteLine("✅ Passes package scenario completed");
  }
}