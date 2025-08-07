using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetDepthStencilStateCommand(IDepthStencilState DepthStencilState, uint StencilRef): ICommand
{
  public CommandType Type => CommandType.SetDepthStencilState;
  public int SizeInBytes => sizeof(long) + sizeof(uint);
}
