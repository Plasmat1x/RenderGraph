using Resources.Enums;

namespace Resources;

public struct BufferDescription
{
  public ulong Size { get; set; }
  public TextureUsage Usage { get; set; }
  public BindFlags BindFlags { get; set; }
  public CPUAccessFlags CPUAccessFlags { get; set; }
  public uint Stride { get; set; }
}
