namespace GraphicsAPI;

public struct Box
{
  public uint Left { get; set; }
  public uint Top { get; set; }
  public uint Front { get; set; }
  public uint Right { get; set; }
  public uint Bottom { get; set; }
  public uint Back { get; set; }

  public uint Width => Right - Left;
  public uint Height => Bottom - Top;
  public uint Depth => Back - Front;
}