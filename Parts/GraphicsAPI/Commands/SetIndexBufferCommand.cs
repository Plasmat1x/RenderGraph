using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace GraphicsAPI.Commands;

public record SetIndexBufferCommand(IBufferView Buffer, IndexFormat Format): ICommand
{
  public CommandType Type => CommandType.SetIndexBuffer;
  public int SizeInBytes => sizeof(long) + sizeof(int);
}
