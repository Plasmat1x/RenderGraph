namespace Core;
public abstract class RenderPass
{
  public string Name { get; set; }
  public bool Enabled { get; set; }
  public int ExecutionOrder { get; set; }

  public readonly List<ResourceHandle> Inputs = new();
  public readonly List<ResourceHandle> Outputs = new();
  public readonly List<RenderPass> Dependicies = new();

  public abstract void Setup(RenderGraphBuilder _builder);
  public abstract void Execute(RenderPassContext _ctx);
  public abstract void AddDependency(RenderPass _pass);
  public abstract void RemoveDependency(RenderPass _pass);
  public abstract bool CanExecute();
  public abstract ResourceUsageInfo GetResourceUsage();
}
