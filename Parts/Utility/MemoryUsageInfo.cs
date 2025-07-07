namespace Utility;

public struct MemoryUsageInfo
{
  public ulong TotalAllocated;
  public ulong TotalUsed;
  public ulong TextureMemory;
  public ulong BufferMemory;
  public ulong PeakUsage;

  public float GetFragmentization()
  {
    throw new NotImplementedException();
  }

  public float GetUtilization()
  {
    throw new NotImplementedException();
  }
}
