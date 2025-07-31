using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record ClearUnorderedAccessBufferCommand(IBufferView Target, uint Value): ICommand
{
  public CommandType Type => CommandType.ClearUnorderedAccess;
  public int SizeInBytes => sizeof(long) + sizeof(uint);
}
