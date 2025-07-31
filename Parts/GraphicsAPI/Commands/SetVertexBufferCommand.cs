using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

// === Vertex/Index Buffer Commands ===

public record SetVertexBufferCommand(IBufferView Buffer, uint Slot): ICommand
{
  public CommandType Type => CommandType.SetVertexBuffer;
  public int SizeInBytes => sizeof(long) + sizeof(uint);
}
