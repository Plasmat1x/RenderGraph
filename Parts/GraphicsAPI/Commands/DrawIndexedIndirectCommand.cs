using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record DrawIndexedIndirectCommand(IBufferView ArgsBuffer, ulong Offset): ICommand
{
  public CommandType Type => CommandType.DrawIndexedIndirect;
  public int SizeInBytes => sizeof(long) + sizeof(ulong);
}
