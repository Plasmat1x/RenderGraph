using Core;
using Core.Enums;

namespace ResourcesTests;

public class DebugMockRenderPass: RenderPass
{
  public DebugMockRenderPass(string _name) : base(_name)
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.Normal;
    Console.WriteLine($"[DEBUG] Created MockRenderPass: {_name}");
  }

  public Action OnExecute { get; set; }
  public bool SetupCalled { get; private set; }
  public bool ExecuteCalled { get; private set; }

  public override void Setup(RenderGraphBuilder _builder)
  {
    SetupCalled = true;
    Console.WriteLine($"[DEBUG] {Name}.Setup() called");
  }

  public override void Execute(RenderPassContext _context)
  {
    ExecuteCalled = true;
    Console.WriteLine($"[DEBUG] {Name}.Execute() called");
    OnExecute?.Invoke();
  }

  public override bool CanExecute()
  {
    var canExecute = base.CanExecute();
    Console.WriteLine($"[DEBUG] {Name}.CanExecute() = {canExecute} (Enabled: {Enabled}, Dependencies: {Dependencies.Count})");

    foreach(var dep in Dependencies)
    {
      Console.WriteLine($"[DEBUG]   Dependency {dep.Name}: Enabled={dep.Enabled}, WasExecuted={dep.Statistics.WasExecutedThisFrame}");
    }

    return canExecute;
  }
}
