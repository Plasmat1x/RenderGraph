using Resources.Enums;

namespace GraphicsAPI;

public class BufferViewDescription
{
  public BufferViewType ViewType { get; set; }
  public TextureFormat Format { get; set; } = TextureFormat.Unknown;
  public ulong Offset { get; set; } = 0;
  public ulong Size { get; set; } = 0;
  public uint Stride { get; set; } = 0;
}