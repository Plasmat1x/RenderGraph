namespace Utility;

public struct PoolUsageStats
{
  public int AvailableCount;
  public int UsedCount;
  public int TotalCount;

  public float UtilizationRate => TotalCount > 0 ? (float)UsedCount / TotalCount : 0f;
}
