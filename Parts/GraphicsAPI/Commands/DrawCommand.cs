using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands;

// === Draw Commands ===

public record DrawCommand(uint VertexCount, uint InstanceCount, uint StartVertex, uint StartInstance): ICommand
{
  public CommandType Type => CommandType.Draw;
  public int SizeInBytes => sizeof(uint) * 4;
}
