namespace GraphicsAPI;

public struct Box
{
  public uint Left;
  public uint Top;
  public uint Front;
  public uint Right;
  public uint Bottom;
  public uint Back;

  public uint Width => Right - Left;
  public uint Height => Bottom - Top;
  public uint Depth => Back - Front;
}