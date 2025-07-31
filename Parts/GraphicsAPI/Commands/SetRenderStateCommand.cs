using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

// === Render State Commands ===

public record SetRenderStateCommand(IRenderState RenderState): ICommand
{
  public CommandType Type => CommandType.SetRenderState;
  public int SizeInBytes => sizeof(long);
}
