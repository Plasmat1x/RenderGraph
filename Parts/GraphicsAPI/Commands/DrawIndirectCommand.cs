using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record DrawIndirectCommand(IBufferView ArgsBuffer, ulong Offset): ICommand
{
  public CommandType Type => CommandType.DrawIndirect;
  public int SizeInBytes => sizeof(long) + sizeof(ulong);
}
