using Core;

using MockImpl;

namespace ResourcesTests;

public class RenderGraphIntegrationTests: IDisposable
{
  private readonly MockGraphicsDevice p_device;
  private readonly RenderGraph p_renderGraph;

  public RenderGraphIntegrationTests()
  {
    p_device = new MockGraphicsDevice();
    p_renderGraph = new RenderGraph(p_device);
  }

  [Fact]
  public void RenderGraph_Should_Execute_Passes_In_Order()
  {
    var executionOrder = new List<string>();

    var pass1 = new MockRenderPass("Pass1");
    var pass2 = new MockRenderPass("Pass2");
    var pass3 = new MockRenderPass("Pass3");

    pass1.OnExecute = () => executionOrder.Add("Pass1");
    pass2.OnExecute = () => executionOrder.Add("Pass2");
    pass3.OnExecute = () => executionOrder.Add("Pass3");

    pass2.AddDependency(pass1);
    pass3.AddDependency(pass2);

    p_renderGraph.AddPass(pass1);
    p_renderGraph.AddPass(pass2);
    p_renderGraph.AddPass(pass3);
    p_renderGraph.Compile();

    using var commandBuffer = p_device.CreateCommandBuffer();
    p_renderGraph.Execute(commandBuffer);

    Assert.Equal(new[] { "Pass1", "Pass2", "Pass3" }, executionOrder);
  }

  [Fact]
  public void RenderGraph_Should_Execute_Passes_In_Order_Debug()
  {
    var executionOrder = new List<string>();

    var pass1 = new DebugMockRenderPass("Pass1");
    var pass2 = new DebugMockRenderPass("Pass2");
    var pass3 = new DebugMockRenderPass("Pass3");

    pass1.OnExecute = () => executionOrder.Add("Pass1");
    pass2.OnExecute = () => executionOrder.Add("Pass2");
    pass3.OnExecute = () => executionOrder.Add("Pass3");

    pass2.AddDependency(pass1);
    pass3.AddDependency(pass2);

    Console.WriteLine("[DEBUG] Adding passes to render graph...");
    p_renderGraph.AddPass(pass1);
    p_renderGraph.AddPass(pass2);
    p_renderGraph.AddPass(pass3);

    Console.WriteLine("[DEBUG] Compiling render graph...");
    p_renderGraph.Compile();

    var compiledOrder = p_renderGraph.GetExecutionOrder();
    Console.WriteLine($"[DEBUG] Compiled execution order: {string.Join(" -> ", compiledOrder.Select(_p => _p.Name))}");

    Console.WriteLine("[DEBUG] Executing render graph...");
    using var commandBuffer = p_device.CreateCommandBuffer();
    p_renderGraph.Execute(commandBuffer);

    Console.WriteLine($"[DEBUG] Actual execution order: [{string.Join(", ", executionOrder)}]");

    Assert.Equal(new[] { "Pass1", "Pass2", "Pass3" }, executionOrder);
  }

  [Fact]
  public void RenderGraph_Should_Skip_Disabled_Passes()
  {
    var pass1 = new MockRenderPass("EnabledPass");
    var pass2 = new MockRenderPass("DisabledPass") { Enabled = false };
    var pass3 = new MockRenderPass("AnotherEnabledPass");

    var executionOrder = new List<string>();
    pass1.OnExecute = () => executionOrder.Add("Pass1");
    pass2.OnExecute = () => executionOrder.Add("Pass2");
    pass3.OnExecute = () => executionOrder.Add("Pass3");

    p_renderGraph.AddPass(pass1);
    p_renderGraph.AddPass(pass2);
    p_renderGraph.AddPass(pass3);
    p_renderGraph.Compile();

    using var commandBuffer = p_device.CreateCommandBuffer();
    p_renderGraph.Execute(commandBuffer);

    Assert.Equal(new[] { "Pass1", "Pass3" }, executionOrder);
  }

  [Fact]
  public void RenderGraph_Should_Skip_Disabled_Passes_Debug()
  {
    var pass1 = new DebugMockRenderPass("EnabledPass");
    var pass2 = new DebugMockRenderPass("DisabledPass") { Enabled = false };
    var pass3 = new DebugMockRenderPass("AnotherEnabledPass");

    var executionOrder = new List<string>();
    pass1.OnExecute = () => executionOrder.Add("Pass1");
    pass2.OnExecute = () => executionOrder.Add("Pass2");
    pass3.OnExecute = () => executionOrder.Add("Pass3");

    Console.WriteLine("[DEBUG] Adding passes (Pass2 is disabled)...");
    p_renderGraph.AddPass(pass1);
    p_renderGraph.AddPass(pass2);
    p_renderGraph.AddPass(pass3);
    p_renderGraph.Compile();

    Console.WriteLine("[DEBUG] Executing render graph...");
    using var commandBuffer = p_device.CreateCommandBuffer();
    p_renderGraph.Execute(commandBuffer);

    Console.WriteLine($"[DEBUG] Actual execution order: [{string.Join(", ", executionOrder)}]");

    Assert.Equal(new[] { "Pass1", "Pass3" }, executionOrder);
  }

  [Fact]
  public void Simple_Mock_Pass_Should_Execute()
  {
    var executionLog = new List<string>();
    var pass = new DebugMockRenderPass("SimplePass");
    pass.OnExecute = () => executionLog.Add("SimplePass");

    p_renderGraph.AddPass(pass);
    p_renderGraph.Compile();

    using var commandBuffer = p_device.CreateCommandBuffer();
    p_renderGraph.Execute(commandBuffer);

    Assert.Single(executionLog);
    Assert.Equal("SimplePass", executionLog[0]);
    Assert.True(pass.ExecuteCalled);
    Assert.True(pass.Statistics.WasExecutedThisFrame);
  }

  public void Dispose()
  {
    p_renderGraph?.Dispose();
    p_device?.Dispose();
  }
}
