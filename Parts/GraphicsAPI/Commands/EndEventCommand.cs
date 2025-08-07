using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

public record EndEventCommand(): ICommand
{
  public CommandType Type => CommandType.SetRenderState;
  public int SizeInBytes => sizeof(long);
}