using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;

namespace GraphicsAPI.Commands;

public record SetMarkerCommand(string Name): ICommand
{
  public CommandType Type => CommandType.SetRenderState;
  public int SizeInBytes => sizeof(long);
}