using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

public record BeginEventCommand(string Name): ICommand
{
  public CommandType Type => CommandType.BeginEvent;
  public int SizeInBytes => sizeof(long);
}
