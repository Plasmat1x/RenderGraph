using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record CopyBufferCommand(IBuffer Src, IBuffer Dst): ICommand
{
  public CommandType Type => CommandType.CopyBuffer;
  public int SizeInBytes => sizeof(long) * 2;
}
