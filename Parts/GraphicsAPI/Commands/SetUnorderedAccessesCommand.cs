using GraphicsAPI.Commands.enums;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetUnorderedAccessesCommand(ShaderStage Stage, uint StartSlot, ITextureView[] Resources): ICommand
{
  public CommandType Type => CommandType.SetUnorderedAccesses;
  public int SizeInBytes => sizeof(int) + sizeof(uint) + sizeof(long) * (Resources?.Length ?? 0);
}
