using System.Data;

namespace Utility;

public class DirectedGraph<T>
{
  private readonly HashSet<T> p_nodes = [];
  private readonly Dictionary<T, List<T>> p_edges = [];

  public void AddNode(T _node)
  {
    throw new NotImplementedException();
  }
  
  public void AddEdge(T _from, T _to)
  {
    throw new NotImplementedException();
  }

  public void RemoveNode(T _node)
  {
    throw new NotImplementedException();
  }

  public void RemoveEdge(T _from, T _to)
  {
    throw new NotImplementedException();
  }

  public List<T> GetTopologicalSort()
  {
    throw new NotImplementedException();
  }

  public bool HasCycle()
  {
    throw new NotImplementedException();
  }

  public List<T> GetDependicies(T _node)
  {
    throw new NotImplementedException();
  }
}