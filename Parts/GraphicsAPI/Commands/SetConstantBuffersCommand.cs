using GraphicsAPI.Commands.enums;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetConstantBuffersCommand(ShaderStage Stage, uint StartSlot, IBufferView[] Buffers): ICommand
{
  public CommandType Type => CommandType.SetConstantBuffers;
  public int SizeInBytes => sizeof(int) + sizeof(uint) + sizeof(long) * (Buffers?.Length ?? 0);
}
