using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

public record SetComputeShaderCommand(ShaderStage Stage, IShader Shader): ICommand
{
  public CommandType Type => CommandType.SetShader;
  public int SizeInBytes => sizeof(int) + sizeof(long);
}