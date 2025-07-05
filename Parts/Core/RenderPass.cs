namespace Core;
public abstract class RenderPass
{
  public string Name { get; }

  protected readonly List<ResourceHandle> p_reads = new();
  protected readonly List<ResourceHandle> p_writes = new();
  protected readonly List<RenderPass> p_deps = new();

  public abstract void Setup(RenderPassContext _ctx);
  public abstract void Execute(RenderPassContext _ctx);
}
