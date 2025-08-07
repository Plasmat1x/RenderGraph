using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetRasterizerStateCommand(IRasterizerState RasterizerState): ICommand
{
  public CommandType Type => CommandType.SetRasterizerState;
  public int SizeInBytes => sizeof(long);
}
