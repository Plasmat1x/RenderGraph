namespace Utility;

public class ResourcePool<T>
{
  private readonly Queue<T> p_availableResources;
  private readonly HashSet<T> p_usedResources;
  private readonly Func<T> p_createFunction;

  public T Rent()
  {
    throw new NotImplementedException();
  }

  public void Return(T _item)
  {
    throw new NotImplementedException();
  }
  public void Clear()
  {
    throw new NotImplementedException();
  }
  public PoolUsageStats GetUsageStats()
  {
    throw new NotImplementedException();
  }
}
