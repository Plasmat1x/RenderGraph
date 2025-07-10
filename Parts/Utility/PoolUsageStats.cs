namespace Utility;

public struct PoolUsageStats
{
  public int AvailableCount { get; set; };
  public int UsedCount { get; set; }
  public int TotalCount { get; set; }

  public float UtilizationRate => TotalCount > 0 ? (float)UsedCount / TotalCount : 0f;
}
