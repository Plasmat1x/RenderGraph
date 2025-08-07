using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;

using Resources;

namespace GraphicsAPI.Commands;

public record SetScissorRectsCommand(Rectangle[] Rects): ICommand
{
  public CommandType Type => CommandType.SetScissorRects;
  public int SizeInBytes => sizeof(int) * 4 * (Rects?.Length ?? 0);
}
