namespace Utility;

public struct MemoryUsageInfo
{
  public ulong TotalAllocated;
  public ulong TotalUsed;
  public ulong TextureMemory;
  public ulong BufferMemory;
  public ulong PeakUsage;

  public float GetFragmentation()
  {
    if(TotalAllocated == 0)
      return 0f;

    ulong unused = TotalAllocated - TotalUsed;
    return (float)unused / TotalAllocated;
  }

  public float GetUtilization()
  {
    if(TotalAllocated == 0)
      return 0f;

    return (float)TotalUsed / TotalAllocated;
  }

  public float GetTextureMemoryRatio()
  {
    if(TotalUsed == 0)
      return 0f;

    return (float)TextureMemory / TotalUsed;
  }

  public float GetBufferMemoryRatio()
  {
    if(TotalUsed == 0)
      return 0f;

    return (float)BufferMemory / TotalUsed;
  }

  public string GetFormattedSize(ulong _bytes)
  {
    const ulong KB = 1024;
    const ulong MB = KB * 1024;
    const ulong GB = MB * 1024;

    if(_bytes >= GB)
      return $"{_bytes / (double)GB:F2} GB";
    else if(_bytes >= MB)
      return $"{_bytes / (double)MB:F2} MB";
    else if(_bytes >= KB)
      return $"{_bytes / (double)KB:F2} KB";
    else
      return $"{_bytes} B";
  }

  public override string ToString()
  {
    return $"MemoryUsage(Total: {GetFormattedSize(TotalAllocated)}, " +
           $"Used: {GetFormattedSize(TotalUsed)}, " +
           $"Textures: {GetFormattedSize(TextureMemory)}, " +
           $"Buffers: {GetFormattedSize(BufferMemory)}, " +
           $"Utilization: {GetUtilization():P1})";
  }
}
