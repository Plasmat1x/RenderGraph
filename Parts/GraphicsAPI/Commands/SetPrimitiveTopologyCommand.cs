using GraphicsAPI.Commands.enums;
using GraphicsAPI.Enums;

namespace GraphicsAPI.Commands;

public record SetPrimitiveTopologyCommand(PrimitiveTopology Topology): ICommand
{
  public CommandType Type => CommandType.SetPrimitiveTopology;
  public int SizeInBytes => sizeof(int);
}
