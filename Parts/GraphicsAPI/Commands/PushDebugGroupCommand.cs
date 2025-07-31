using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

// === Debug Commands ===

public record PushDebugGroupCommand(string Name): ICommand
{
  public CommandType Type => CommandType.PushDebugGroup;
  public int SizeInBytes => sizeof(int) + (Name?.Length ?? 0) * sizeof(char);
}
