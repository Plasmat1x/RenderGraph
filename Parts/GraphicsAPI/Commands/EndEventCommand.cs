using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;

namespace GraphicsAPI.Commands;

public record EndEventCommand(): ICommand
{
  public CommandType Type => CommandType.SetRenderState;
  public int SizeInBytes => sizeof(long);
}