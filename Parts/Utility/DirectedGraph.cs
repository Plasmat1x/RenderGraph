namespace Utility;

public class DirectedGraph<T>
{
  private readonly HashSet<T> p_nodes = [];
  private readonly Dictionary<T, List<T>> p_edges = [];
  private readonly Dictionary<T, List<T>> p_reverseEdges = [];

  public IReadOnlyCollection<T> Nodes => p_nodes;
  public int NodeCount => p_nodes.Count;
  public int EdgeCount => p_edges.Values.Sum(list => list.Count);

  public void AddNode(T _node)
  {
    if(_node == null)
      throw new ArgumentNullException(nameof(_node));

    p_nodes.Add(_node);

    if(!p_edges.ContainsKey(_node))
      p_edges[_node] = [];

    if(!p_reverseEdges.ContainsKey(_node))
      p_reverseEdges[_node] = [];
  }

  public void AddEdge(T _from, T _to)
  {
    if(_from == null)
      throw new ArgumentNullException(nameof(_from));
    if(_to == null)
      throw new ArgumentNullException(nameof(_to));

    AddNode(_from);
    AddNode(_to);

    if(!p_edges[_from].Contains(_to))
    {
      p_edges[_from].Add(_to);
      p_reverseEdges[_to].Add(_from);
    }
  }

  public void RemoveNode(T _node)
  {
    if(p_nodes == null || !p_nodes.Contains(_node))
      return;

    var incoming = p_reverseEdges[_node].ToList();
    foreach(var from in incoming)
    {
      RemoveEdge(from, _node);
    }

    var outgoing = p_edges[_node].ToList();
    foreach(var to in outgoing)
    {
      RemoveEdge(_node, to);
    }

    p_nodes.Remove(_node);
    p_edges.Remove(_node);
    p_reverseEdges.Remove(_node);
  }

  public void RemoveEdge(T _from, T _to)
  {
    if(_from == null ||  _to == null) 
      return;
    
    if(p_edges.ContainsKey(_from))
      p_edges[_from].Remove(_to);

    if(p_reverseEdges.ContainsKey(_to))
      p_reverseEdges[_to].Remove(_from);
  }

  public List<T> GetTopologicalSort()
  {
    var result = new List<T>();
    var visited = new HashSet<T>();
    var visiting = new HashSet<T>();

    foreach(var node in p_nodes)
    {
      if(!visited.Contains(node))
      {
        if(!TopologicalSortDFS(node, visited, visiting, result))
        {
          throw new InvalidOperationException("Graph contains cycles sort is not possible");
        }
      }
    }
    result.Reverse();
    return result;
  }

  public bool HasCycle()
  {
    var visited = new HashSet<T>();
    var visiting = new HashSet<T>();

    foreach(var node in p_nodes)
    {
      if(!visited.Contains(node))
      {
        if(HasCycleDFS(node, visited, visiting))
          return true;
      }
    }

    return false;
  }

  public List<T> GetDependicies(T _node)
  {
    if(_node == null || !p_nodes.Contains(_node))
      return new List<T>();

    return p_reverseEdges[_node].ToList();
  }

  public void Clear()
  {
    p_nodes.Clear();
    p_edges.Clear();
    p_reverseEdges.Clear();
  }

  public bool ContainsNode(T _node)
  {
    return _node != null && p_nodes.Contains(_node);
  }

  public bool ContainsEdge(T _from, T _to)
  {
    return _from != null && _to != null &&
      p_edges.ContainsKey(_from) &&
      p_edges[_from].Contains(_to);
  }

  private bool TopologicalSortDFS(T? node, HashSet<T> visited, HashSet<T> visiting, List<T> result)
  {
    if(visiting.Contains(node))
      return false;

    if(visited.Contains(node))
     return true;

    visiting.Add(node);

    foreach(var neighbor in p_edges[node])
    {
      if(!TopologicalSortDFS(neighbor, visited, visiting, result))
        return false;
    }

    visiting.Remove(node);
    visited.Add(node);
    result.Add(node);

    return true;
  }

  private bool HasCycleDFS(T? node, HashSet<T> visited, HashSet<T> visiting)
  {
    if(visiting.Contains(node))
      return true;

    if(visited.Contains(node))
      return false;

    visiting.Add(node);

    foreach(var neighbor in p_edges[node])
    {
      if(HasCycleDFS(neighbor, visited, visiting))
        return true;
    }

    visiting.Remove(node);
    visited.Add(node);
    return true;
  }
}