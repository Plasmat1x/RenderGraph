using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record EndQueryCommand(IQuery Query): ICommand
{
  public CommandType Type => CommandType.EndQuery;
  public int SizeInBytes => sizeof(long);
}
