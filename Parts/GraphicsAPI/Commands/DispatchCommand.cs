using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;

namespace GraphicsAPI.Commands;

// === Compute Commands ===

public record DispatchCommand(uint GroupCountX, uint GroupCountY, uint GroupCountZ): ICommand
{
  public CommandType Type => CommandType.Dispatch;
  public int SizeInBytes => sizeof(uint) * 3;
}
