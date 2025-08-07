using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetShaderResourcesCommand(ShaderStage Stage, uint StartSlot, ITextureView[] Resources): ICommand
{
  public CommandType Type => CommandType.SetShaderResources;
  public int SizeInBytes => sizeof(int) + sizeof(uint) + sizeof(long) * (Resources?.Length ?? 0);
}
