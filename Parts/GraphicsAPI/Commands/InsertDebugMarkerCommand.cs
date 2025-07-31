using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

public record InsertDebugMarkerCommand(string Name): ICommand
{
  public CommandType Type => CommandType.InsertDebugMarker;
  public int SizeInBytes => sizeof(int) + (Name?.Length ?? 0) * sizeof(char);
}
