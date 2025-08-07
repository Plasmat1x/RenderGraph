using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;

namespace GraphicsAPI.Commands;

public record UAVBarrierCommand(IResource Resource): ICommand
{
  public CommandType Type => CommandType.UAVBarrier;
  public int SizeInBytes => sizeof(long);
}
