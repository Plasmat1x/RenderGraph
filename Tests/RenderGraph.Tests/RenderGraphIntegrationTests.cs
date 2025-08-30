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

    var pass1 = new MockRenderPass("Pass1") { AlwaysExecute = true };
    var pass2 = new MockRenderPass("Pass2") { AlwaysExecute = true };
    var pass3 = new MockRenderPass("Pass3") { AlwaysExecute = true };

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

    Assert.Equal(new[] { "Pass1", "Pass2", "Pass3" }, executionOrder.ToArray());
  }

  [Fact]
  public void RenderGraph_Should_Skip_Disabled_Passes()
  {
    var pass1 = new MockRenderPass("EnabledPass") { AlwaysExecute = true };
    var pass2 = new MockRenderPass("DisabledPass") { Enabled = false };
    var pass3 = new MockRenderPass("AnotherEnabledPass") { AlwaysExecute = true };

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

    Assert.Contains("Pass1", executionOrder);
    Assert.Contains("Pass3", executionOrder);
    Assert.DoesNotContain("Pass2", executionOrder);
    Assert.Equal(2, executionOrder.Count);
  }

  [Fact]
  public void RenderGraph_Should_Cull_Unused_Passes()
  {
    p_renderGraph.Reset();

    var executionOrder = new List<string>();

    var unusedPass = new MockRenderPass("UnusedPass") { AlwaysExecute = false };
    var usedPass = new MockRenderPass("UsedPass") { AlwaysExecute = true };

    unusedPass.OnExecute = () => executionOrder.Add("UnusedPass");
    usedPass.OnExecute = () => executionOrder.Add("UsedPass");

    p_renderGraph.AddPass(unusedPass);
    p_renderGraph.AddPass(usedPass);
    p_renderGraph.Compile();

    Assert.False(unusedPass.Enabled, "Unused pass should be disabled after compilation");
    Assert.True(usedPass.Enabled, "Used pass should remain enabled");

    using var commandBuffer = p_device.CreateCommandBuffer();
    p_renderGraph.Execute(commandBuffer);

    Assert.Equal(new[] { "UsedPass" }, executionOrder);
  }

  [Fact]
  public void Simple_Mock_Pass_Should_Execute()
  {
    var executionLog = new List<string>();
    var pass = new DebugMockRenderPass("SimplePass") { AlwaysExecute = true, Enabled = true };
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

  [Fact]
  public void Debug_Pass_States_Before_And_After_Execution()
  {
    var pass1 = new MockRenderPass("Pass1");
    var pass2 = new MockRenderPass("Pass2");

    pass2.AddDependency(pass1);

    var executionOrder = new List<string>();
    pass1.OnExecute = () => executionOrder.Add("Pass1");
    pass2.OnExecute = () => executionOrder.Add("Pass2");

    p_renderGraph.AddPass(pass1);
    p_renderGraph.AddPass(pass2);
    p_renderGraph.Compile();

    pass1.Statistics.Reset();
    pass1.Statistics.StartFrame();
    pass2.Statistics.Reset();
    pass2.Statistics.StartFrame();

    using var commandBuffer = p_device.CreateCommandBuffer();
    p_renderGraph.Execute(commandBuffer);

    Assert.Equal(new[] { "Pass1", "Pass2" }, executionOrder);
  }

  public void Dispose()
  {
    p_renderGraph?.Dispose();
    p_device?.Dispose();
  }
}
