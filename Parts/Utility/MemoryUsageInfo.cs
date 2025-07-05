namespace Utility;

public class MemoryUsageInfo
{
  public ulong TotalAllocated {  get; private set; }
  public ulong TotalUsed { get; private set; }
  public ulong TextureMemory { get; private set; }
  public ulong BufferMemory { get; private set; }
  public ulong PeakUsage { get; private set; }

  public float GetFragmentization()
  {
    throw new NotImplementedException();
  }

  public float GetUtilization()
  {
    throw new NotImplementedException();
  }
}
