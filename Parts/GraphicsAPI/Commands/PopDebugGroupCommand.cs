using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

public record PopDebugGroupCommand(): ICommand
{
  public CommandType Type => CommandType.PopDebugGroup;
  public int SizeInBytes => 0;
}
