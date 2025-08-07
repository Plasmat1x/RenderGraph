using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetUnorderedAccessCommand(ShaderStage Stage, uint Slot, ITextureView Resource): ICommand
{
  public CommandType Type => CommandType.SetUnorderedAccess;
  public int SizeInBytes => sizeof(int) + sizeof(uint) + sizeof(long);
}
