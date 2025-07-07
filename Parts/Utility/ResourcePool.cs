namespace Utility;

public class ResourcePool<T> : IDisposable where T : class, IDisposable
{
  private readonly Queue<T> p_availableResources = [];
  private readonly HashSet<T> p_usedResources = [];
  private readonly Func<T> p_createFunction;

  public ResourcePool(Func<T> _createFunction)
  {
    p_createFunction = _createFunction ?? throw new ArgumentNullException(nameof(_createFunction));
  }

  public T Rent()
  {
    T resource;

    if(p_availableResources.Count > 0)
      resource = p_availableResources.Dequeue();
    else
      resource = p_createFunction();

    p_usedResources.Add(resource);
    return resource;
  }

  public void Return(T _resource)
  {
    if(_resource == null)
      return;

    if(p_usedResources.Remove(_resource))
      p_availableResources.Enqueue(_resource);
  }

  public void Clear()
  {
    foreach(var resource in p_availableResources)
      resource?.Dispose();
    p_availableResources.Clear();

    foreach(var resource in p_usedResources)
      resource?.Dispose();
    p_usedResources.Clear();
  }

  public void Defragment()
  {
    const int maxPoolSize = 32;

    while(p_availableResources.Count > maxPoolSize)
    {
      var resource = p_availableResources.Dequeue();
      resource?.Dispose();
    }
  }

  public IEnumerable<T> GetAvailableResources() => p_availableResources;

  public PoolUsageStats GetUsageStats()
  {
    return new PoolUsageStats
    {
      AvailableCount = p_availableResources.Count,
      UsedCount = p_usedResources.Count,
      TotalCount = p_availableResources.Count + p_usedResources.Count
    };
  }

  public void Dispose()
  {
    Clear();
  }
}
