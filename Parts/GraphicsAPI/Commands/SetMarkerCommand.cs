using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

public record SetMarkerCommand(string Name): ICommand
{
  public CommandType Type => CommandType.SetRenderState;
  public int SizeInBytes => sizeof(long);
}