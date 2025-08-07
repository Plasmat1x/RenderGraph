using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;

namespace GraphicsAPI.Commands;

// === Draw Commands ===

public record DrawCommand(uint VertexCount, uint InstanceCount, uint StartVertex, uint StartInstance): ICommand
{
  public CommandType Type => CommandType.Draw;
  public int SizeInBytes => sizeof(uint) * 4;
}
