using GraphicsAPI;

namespace Core;

public class RenderGraph : IDisposable
{
  private readonly List<RenderPass> p_passes = new();
  private readonly ResourceManager p_resourceManager;
  private readonly DependencyResolver p_dependencyResolver;
  private readonly IGraphicsDevice p_device;
  private bool p_compiled;
  private FrameData p_frameData;

  public void AddPass<T>(T _pass) where T : RenderPass
  {
    throw new NotImplementedException();
  }
  public void RemovePass(RenderPass _pass)
  {
    throw new NotImplementedException();
  }
  public void Execute(ICommandBuffer _commandBuffer)
  {
    throw new NotImplementedException();
  }
  public void Compile()
  {
    throw new NotImplementedException();
  }
  public void Reset()
  {
    throw new NotImplementedException();
  }
  public bool ValidateGraph()
  {
    throw new NotImplementedException();
  }
  public List<RenderPass> GetExecutionOrder()
  {
    throw new NotImplementedException();
  }
  public void Dispose()
  {

  }
}
