using GraphicsAPI.Commands.enums;

using Resources;

namespace GraphicsAPI.Commands;

public record SetViewportsCommand(Viewport[] Viewports): ICommand
{
  public CommandType Type => CommandType.SetViewports;
  public int SizeInBytes => sizeof(float) * 6 * (Viewports?.Length ?? 0);
}
