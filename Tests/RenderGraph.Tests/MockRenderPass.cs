using Core;
using Core.Enums;

namespace ResourcesTests;

// ================================================================================================
// MOCK CLASSES FOR TESTING
// ================================================================================================

public class MockRenderPass: RenderPass
{
  public Action OnExecute { get; set; }
  public bool SetupCalled { get; private set; }
  public bool ExecuteCalled { get; private set; }

  public MockRenderPass(string name) : base(name)
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.Normal;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    SetupCalled = true;
    Console.WriteLine($"[TEST] MockRenderPass '{Name}' Setup called");
    // Mock passes don't create resources by default
  }

  public override void Execute(RenderPassContext context)
  {
    ExecuteCalled = true;
    Console.WriteLine($"[TEST] MockRenderPass '{Name}' Execute called");
    OnExecute?.Invoke();
  }

  public override bool CanExecute()
  {
    var canExecute = base.CanExecute();
    Console.WriteLine($"[TEST] MockRenderPass '{Name}' CanExecute: {canExecute}, Enabled: {Enabled}");
    return canExecute;
  }
}
