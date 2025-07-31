using GraphicsAPI.Commands.enums;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetConstantBufferCommand(ShaderStage Stage, uint Slot, IBufferView Buffer): ICommand
{
  public CommandType Type => CommandType.SetConstantBuffer;
  public int SizeInBytes => sizeof(int) + sizeof(uint) + sizeof(long);
}
