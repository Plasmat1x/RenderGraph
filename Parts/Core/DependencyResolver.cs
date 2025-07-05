using Utility;

namespace Core;

public class DependencyResolver
{
  private readonly DirectedGraph<RenderPass> p_passGraph = new DirectedGraph<RenderPass>();
  private readonly Dictionary<ResourceHandle, List<RenderPass>> p_resourceDependencies = [];
  
  public void BuildDependencyGraph(List<RenderPass> _passes)
  {
    throw new NotImplementedException();
  } 

  public List<RenderPass> TopologicalSort()
  {
    throw new NotImplementedException();
  }

  public List<RenderPass> DetectCycles()
  {
    throw new NotImplementedException();
  }

  public List<RenderPass> CullUnusedPasses()
  {
    throw new NotImplementedException();
  }

  public bool ValidateDependencies()
  {
    throw new NotImplementedException();
  }

  public List<RenderPass> GetCrtiticalPath()
  {
    throw new NotImplementedException();
  }
}


