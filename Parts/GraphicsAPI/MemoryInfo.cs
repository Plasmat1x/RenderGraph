namespace GraphicsAPI;

public struct MemoryInfo
{
  public ulong TotalMemory { get; set; }
  public ulong AvailableMemory { get; set; }
  public ulong UsedMemory { get; set; }
  public ulong Budget { get; set; }
  public ulong CurrentUsage { get; set; }
  public ulong CurrentReservation { get; set; }
}