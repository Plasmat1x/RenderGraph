using GraphicsAPI.Commands.enums;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetSamplerCommand(ShaderStage Stage, uint Slot, ISampler Sampler): ICommand
{
  public CommandType Type => CommandType.SetSampler;
  public int SizeInBytes => sizeof(int) + sizeof(uint) + sizeof(long);
}
