using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

// === Shader Resource Commands ===

public record SetShaderResourceCommand(ShaderStage Stage, uint Slot, ITextureView Resource): ICommand
{
  public CommandType Type => CommandType.SetShaderResource;
  public int SizeInBytes => sizeof(int) + sizeof(uint) + sizeof(long);
}
