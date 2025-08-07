using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;

namespace GraphicsAPI.Commands;

public record UAVBarriersCommand(IResource[] Resources): ICommand
{
  public CommandType Type => CommandType.UAVBarriers;
  public int SizeInBytes => sizeof(long) * (Resources?.Length ?? 0);
}
