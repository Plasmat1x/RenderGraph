using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetVertexBuffersCommand(IBufferView[] Buffers, uint StartSlot): ICommand
{
  public CommandType Type => CommandType.SetVertexBuffers;
  public int SizeInBytes => sizeof(long) * (Buffers?.Length ?? 0) + sizeof(uint);
}
