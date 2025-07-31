using GraphicsAPI.Commands.enums;

using Resources;

namespace GraphicsAPI.Commands;

public record SetScissorRectCommand(Rectangle Rect): ICommand
{
  public CommandType Type => CommandType.SetScissorRect;
  public int SizeInBytes => sizeof(int) * 4; // x, y, width, height
}
