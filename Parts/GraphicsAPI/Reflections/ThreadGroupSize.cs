namespace GraphicsAPI.Reflections;

public class ThreadGroupSize
{
  public uint X { get; set; } = 1;
  public uint Y { get; set; } = 1;
  public uint Z { get; set; } = 1;

  public uint TotalThreads => X * Y * Z;
}
