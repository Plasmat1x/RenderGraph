using System.Linq.Expressions;

using Utility;

namespace Core;

public class DependencyResolver
{
  private readonly DirectedGraph<RenderPass> p_passGraph = new DirectedGraph<RenderPass>();
  private readonly Dictionary<ResourceHandle, List<RenderPass>> p_resourceDependencies = [];

  public void AddNode(RenderPass _pass)
  {
    if(_pass == null)
      throw new ArgumentNullException(nameof(_pass));
    p_passGraph.AddNode(_pass);
  }

  public void AddEdge(RenderPass _from, RenderPass _to)
  {
    if(_from == null)
      throw new ArgumentNullException(nameof(_from));
    if(_to == null)
      throw new ArgumentNullException(nameof(_to));

    p_passGraph.AddEdge(_from, _to);
  }

  public void BuildDependencyGraph(List<RenderPass> _passes)
  {
    if(_passes == null)
      throw new ArgumentNullException(nameof(_passes));

    Clear();

    foreach(var pass in _passes)
    {
      AddNode(pass);
    }

    p_resourceDependencies.Clear();
    var resourceProducers = new Dictionary<ResourceHandle, RenderPass>();

    foreach(var pass in _passes)
    {
      foreach(var output in pass.Outputs)
      {
        if(resourceProducers.ContainsKey(output))
        {
          throw new InvalidOperationException($"Resource {output.Name} is produced by multiple passes: " +
           $"'{resourceProducers[output].Name}' and '{pass.Name}'");
        }
        resourceProducers[output] = pass;
      }
    }

    foreach(var pass in _passes)
    {
      foreach(var input in pass.Inputs)
      {
        if(resourceProducers.TryGetValue(input, out var producer))
        {
          if(producer != pass)
          {
            AddEdge(producer, pass);

            if(!p_resourceDependencies.ContainsKey(input))
              p_resourceDependencies[input] = [];

            if(!p_resourceDependencies[input].Contains(pass))
              p_resourceDependencies[input].Add(pass);
          }
        }
      }

      foreach (var dependency in pass.Dependencies)
        AddEdge(dependency, pass);
    }
  }

  public List<RenderPass> TopologicalSort()
  {
    try
    {
      return p_passGraph.GetTopologicalSort();
    }
    catch(InvalidOperationException ex)
    {
      throw new InvalidOperationException("Cannot create execution order to circular dependencies", ex);
    }
  }

  public List<RenderPass> DetectCycles()
  {
    if(!p_passGraph.HasCycle())
      return [];

    var visited = new HashSet<RenderPass>();
    var visiting = new HashSet<RenderPass>();
    var cycle = new List<RenderPass>();

    foreach(var node in p_passGraph.Nodes)
    {
      if(!visited.Contains(node))
      {
        if(DetectCycleDFS(node, visited, visiting, cycle))
          return cycle;
      }
    }

    return [];
  }

  public List<RenderPass> CullUnusedPasses()
  {
    var unusedPasses = new List<RenderPass>();
    
    foreach(var pass in p_passGraph.Nodes)
    {
      var dependents = p_passGraph.GetDependicies(pass);
      var hasExternalOutputs = HasExternalOutputs(pass);
      if(dependents.Count == 0 && !hasExternalOutputs && !pass.AlwaysExecute)
      {
        unusedPasses.Add(pass);
      }
    }

    return unusedPasses;
  }

  public bool ValidateDependencies()
  {
    try
    {
      if(p_passGraph.HasCycle())
        return false;

      foreach(var pass in p_passGraph.Nodes)
      {
        foreach(var dependency in pass.Dependencies)
        {
          if(!p_passGraph.ContainsNode(dependency))
            return false;
        }
      }
      return true;
    }
    catch
    {
      return false;
    }
  }

  public List<RenderPass> GetCrtiticalPath()
  {
    var criticalPath = new List<RenderPass>();
    var longestPaths = new Dictionary<RenderPass, List<RenderPass>>();

    foreach (var node in p_passGraph.Nodes)
    {
      var path = FindLongestPath(node, new HashSet<RenderPass>());
      longestPaths[node] = path;
    }

    var maxLength = 0;
    foreach (var kvp in longestPaths)
    {
      if(kvp.Value.Count > maxLength)
      {
        maxLength = kvp.Value.Count;
        criticalPath = kvp.Value;
      }
    }

    return criticalPath;
  }

  public void Clear()
  {
    p_passGraph.Clear();
    p_resourceDependencies.Clear();
  }

  private List<RenderPass> FindLongestPath(RenderPass _node, HashSet<RenderPass> _visited)
  {
    if(_visited.Contains(_node))
      return [];

    var longestPath = new List<RenderPass> { _node };
    var maxLength = 0;

    foreach(var dependent in p_passGraph.GetDependicies(_node))
    {
      var path = FindLongestPath(dependent, [.. _visited]);
      if(path.Count > maxLength)
      {
        maxLength = path.Count;
        longestPath = [_node, .. path];
      }
    }
    return longestPath;
  }

  private bool DetectCycleDFS(RenderPass _node, HashSet<RenderPass> _visited, HashSet<RenderPass> _visiting, List<RenderPass> _cycle)
  {
    if(_visiting.Contains(_node))
    {
      _cycle.Add(_node);
      return true;
    }

    if(_visited.Contains(_node))
      return false;

    _visiting.Add(_node); 

    foreach(var neighbor in p_passGraph.GetDependicies(_node))
    {
      if(DetectCycleDFS(neighbor, _visited, _visiting, _cycle))
      {
        _cycle.Add(_node);
        return true;
      }
    }

    _visiting.Remove(_node);
    _visited.Add(_node);
    return false;
  }
  private bool HasExternalOutputs(RenderPass _pass)
  {
    foreach(var output in _pass.Outputs)
    {
      if(!p_resourceDependencies.ContainsKey(output) || p_resourceDependencies[output].Count == 0)
        return true;
    }

    return false;
  }
}


