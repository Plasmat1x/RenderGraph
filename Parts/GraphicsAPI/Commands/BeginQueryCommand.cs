using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

// === Query Commands ===

public record BeginQueryCommand(IQuery Query): ICommand
{
  public CommandType Type => CommandType.BeginQuery;
  public int SizeInBytes => sizeof(long);
}
