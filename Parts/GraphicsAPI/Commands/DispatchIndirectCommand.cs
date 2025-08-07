using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record DispatchIndirectCommand(IBufferView ArgsBuffer, ulong Offset): ICommand
{
  public CommandType Type => CommandType.DispatchIndirect;
  public int SizeInBytes => sizeof(long) + sizeof(ulong);
}
