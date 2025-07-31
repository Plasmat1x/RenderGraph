using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record CopyTextureRegionCommand(ITexture Src, uint SrcMip, uint SrcArray, Box SrcBox,
                                     ITexture Dst, uint DstMip, uint DstArray, Point3D DstOffset): ICommand
{
  public CommandType Type => CommandType.CopyTextureRegion;
  public int SizeInBytes => sizeof(long) * 2 + sizeof(uint) * 4 + sizeof(int) * 6 + sizeof(int) * 3;
}
