using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

// === Compute Commands ===

public record DispatchCommand(uint GroupCountX, uint GroupCountY, uint GroupCountZ): ICommand
{
  public CommandType Type => CommandType.Dispatch;
  public int SizeInBytes => sizeof(uint) * 3;
}
