using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record CopyBufferRegionCommand(IBuffer Src, ulong SrcOffset, IBuffer Dst, ulong DstOffset, ulong Size): ICommand
{
  public CommandType Type => CommandType.CopyBufferRegion;
  public int SizeInBytes => sizeof(long) * 2 + sizeof(ulong) * 3;
}
