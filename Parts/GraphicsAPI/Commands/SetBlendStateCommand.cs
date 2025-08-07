using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Interfaces;

using System.Numerics;

namespace GraphicsAPI.Commands;

public record SetBlendStateCommand(IBlendState BlendState, Vector4 BlendFactor, uint SampleMask): ICommand
{
  public CommandType Type => CommandType.SetBlendState;
  public int SizeInBytes => sizeof(long) + sizeof(float) * 4 + sizeof(uint);
}
