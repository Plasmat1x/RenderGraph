using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;

namespace GraphicsAPI.Commands;

public record PopDebugGroupCommand(): ICommand
{
  public CommandType Type => CommandType.PopDebugGroup;
  public int SizeInBytes => 0;
}
