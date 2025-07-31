using GraphicsAPI.Commands.enums;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record ClearDepthStencilCommand(ITextureView Target, ClearFlags Flags, float Depth, byte Stencil): ICommand
{
  public CommandType Type => CommandType.ClearDepthStencil;
  public int SizeInBytes => sizeof(long) + sizeof(int) + sizeof(float) + sizeof(byte);
}
