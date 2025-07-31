using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

public record UAVBarriersCommand(IResource[] Resources): ICommand
{
  public CommandType Type => CommandType.UAVBarriers;
  public int SizeInBytes => sizeof(long) * (Resources?.Length ?? 0);
}
