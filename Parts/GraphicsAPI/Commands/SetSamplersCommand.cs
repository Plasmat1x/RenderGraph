using GraphicsAPI.Commands.enums;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetSamplersCommand(ShaderStage Stage, uint StartSlot, ISampler[] Samplers): ICommand
{
  public CommandType Type => CommandType.SetSamplers;
  public int SizeInBytes => sizeof(int) + sizeof(uint) + sizeof(long) * (Samplers?.Length ?? 0);
}
