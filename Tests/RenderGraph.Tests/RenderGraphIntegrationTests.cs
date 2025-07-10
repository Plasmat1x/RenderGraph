using Core;

using MockImpl;

namespace ResourcesTests;

public class RenderGraphIntegrationTests: IDisposable
{
  private readonly MockGraphicsDevice _device;
  private readonly RenderGraph _renderGraph;

  public RenderGraphIntegrationTests()
  {
    _device = new MockGraphicsDevice();
    _renderGraph = new RenderGraph(_device);
  }

  [Fact]
  public void RenderGraph_Should_Execute_Passes_In_Order()
  {
    // Given a compiled render graph with ordered passes
    var executionOrder = new List<string>();

    var pass1 = new MockRenderPass("Pass1");
    var pass2 = new MockRenderPass("Pass2");
    var pass3 = new MockRenderPass("Pass3");

    pass1.OnExecute = () => executionOrder.Add("Pass1");
    pass2.OnExecute = () => executionOrder.Add("Pass2");
    pass3.OnExecute = () => executionOrder.Add("Pass3");

    pass2.AddDependency(pass1);
    pass3.AddDependency(pass2);

    _renderGraph.AddPass(pass1);
    _renderGraph.AddPass(pass2);
    _renderGraph.AddPass(pass3);
    _renderGraph.Compile();

    // When executing
    using var commandBuffer = _device.CreateCommandBuffer();
    _renderGraph.Execute(commandBuffer);

    // Then passes should execute in dependency order
    Assert.Equal(new[] { "Pass1", "Pass2", "Pass3" }, executionOrder);
  }

  [Fact]
  public void RenderGraph_Should_Execute_Passes_In_Order_Debug()
  {
    // Given a compiled render graph with ordered passes
    var executionOrder = new List<string>();

    var pass1 = new DebugMockRenderPass("Pass1");
    var pass2 = new DebugMockRenderPass("Pass2");
    var pass3 = new DebugMockRenderPass("Pass3");

    pass1.OnExecute = () => executionOrder.Add("Pass1");
    pass2.OnExecute = () => executionOrder.Add("Pass2");
    pass3.OnExecute = () => executionOrder.Add("Pass3");

    // ИСПРАВЛЕНИЕ: Устанавливаем зависимости правильно
    pass2.AddDependency(pass1);
    pass3.AddDependency(pass2);

    Console.WriteLine("[DEBUG] Adding passes to render graph...");
    _renderGraph.AddPass(pass1);
    _renderGraph.AddPass(pass2);
    _renderGraph.AddPass(pass3);

    Console.WriteLine("[DEBUG] Compiling render graph...");
    _renderGraph.Compile();

    // Проверяем порядок компиляции
    var compiledOrder = _renderGraph.GetExecutionOrder();
    Console.WriteLine($"[DEBUG] Compiled execution order: {string.Join(" -> ", compiledOrder.Select(p => p.Name))}");

    // When executing
    Console.WriteLine("[DEBUG] Executing render graph...");
    using var commandBuffer = _device.CreateCommandBuffer();
    _renderGraph.Execute(commandBuffer);

    Console.WriteLine($"[DEBUG] Actual execution order: [{string.Join(", ", executionOrder)}]");

    // Then passes should execute in dependency order
    Assert.Equal(new[] { "Pass1", "Pass2", "Pass3" }, executionOrder);
  }

  [Fact]
  public void RenderGraph_Should_Skip_Disabled_Passes()
  {
    // Given a render graph with disabled pass
    var pass1 = new MockRenderPass("EnabledPass");
    var pass2 = new MockRenderPass("DisabledPass") { Enabled = false };
    var pass3 = new MockRenderPass("AnotherEnabledPass");

    var executionOrder = new List<string>();
    pass1.OnExecute = () => executionOrder.Add("Pass1");
    pass2.OnExecute = () => executionOrder.Add("Pass2");
    pass3.OnExecute = () => executionOrder.Add("Pass3");

    _renderGraph.AddPass(pass1);
    _renderGraph.AddPass(pass2);
    _renderGraph.AddPass(pass3);
    _renderGraph.Compile();

    // When executing
    using var commandBuffer = _device.CreateCommandBuffer();
    _renderGraph.Execute(commandBuffer);

    // Then disabled pass should be skipped
    Assert.Equal(new[] { "Pass1", "Pass3" }, executionOrder);
  }

  [Fact]
  public void RenderGraph_Should_Skip_Disabled_Passes_Debug()
  {
    // Given a render graph with disabled pass
    var pass1 = new DebugMockRenderPass("EnabledPass");
    var pass2 = new DebugMockRenderPass("DisabledPass") { Enabled = false };
    var pass3 = new DebugMockRenderPass("AnotherEnabledPass");

    var executionOrder = new List<string>();
    pass1.OnExecute = () => executionOrder.Add("Pass1");
    pass2.OnExecute = () => executionOrder.Add("Pass2");
    pass3.OnExecute = () => executionOrder.Add("Pass3");

    Console.WriteLine("[DEBUG] Adding passes (Pass2 is disabled)...");
    _renderGraph.AddPass(pass1);
    _renderGraph.AddPass(pass2);
    _renderGraph.AddPass(pass3);
    _renderGraph.Compile();

    // When executing
    Console.WriteLine("[DEBUG] Executing render graph...");
    using var commandBuffer = _device.CreateCommandBuffer();
    _renderGraph.Execute(commandBuffer);

    Console.WriteLine($"[DEBUG] Actual execution order: [{string.Join(", ", executionOrder)}]");

    // Then disabled pass should be skipped
    Assert.Equal(new[] { "Pass1", "Pass3" }, executionOrder);
  }

  [Fact]
  public void Simple_Mock_Pass_Should_Execute()
  {
    // Given a simple mock pass
    var executionLog = new List<string>();
    var pass = new DebugMockRenderPass("SimplePass");
    pass.OnExecute = () => executionLog.Add("SimplePass");

    _renderGraph.AddPass(pass);
    _renderGraph.Compile();

    // When executing
    using var commandBuffer = _device.CreateCommandBuffer();
    _renderGraph.Execute(commandBuffer);

    // Then pass should execute
    Assert.Single(executionLog);
    Assert.Equal("SimplePass", executionLog[0]);
    Assert.True(pass.ExecuteCalled);
    Assert.True(pass.Statistics.WasExecutedThisFrame);
  }

  public void Dispose()
  {
    _renderGraph?.Dispose();
    _device?.Dispose();
  }
}
