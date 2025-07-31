using GraphicsAPI.Commands.enums;

using Resources;

namespace GraphicsAPI.Commands;

// === Viewport Commands ===

public record SetViewportCommand(Viewport Viewport): ICommand
{
  public CommandType Type => CommandType.SetViewport;
  public int SizeInBytes => sizeof(float) * 6; // x, y, width, height, minDepth, maxDepth
}
