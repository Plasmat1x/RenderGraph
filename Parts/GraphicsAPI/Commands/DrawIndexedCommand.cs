using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

public record DrawIndexedCommand(uint IndexCount, uint InstanceCount, uint StartIndex, int BaseVertex, uint StartInstance): ICommand
{
  public CommandType Type => CommandType.DrawIndexed;
  public int SizeInBytes => sizeof(uint) * 4 + sizeof(int);
}
