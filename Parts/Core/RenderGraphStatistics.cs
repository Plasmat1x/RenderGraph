using Utility;

namespace Core;

public class RenderGraphStatistics
{
  public int TotalPasses;
  public int EnabledPasses;
  public int DisabledPasses;
  public int TotalResources;
  public MemoryUsageInfo MemoryUsage;
  public bool IsCompiled;
  public ulong LastFrameIndex;

  public float PassUtilization => TotalPasses > 0 ? (float)EnabledPasses / TotalPasses : 0f;
}
