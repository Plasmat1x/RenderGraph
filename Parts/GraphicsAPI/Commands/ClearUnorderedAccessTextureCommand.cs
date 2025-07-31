using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

using System.Numerics;

namespace GraphicsAPI.Commands;

public record ClearUnorderedAccessTextureCommand(ITextureView Target, Vector4 Value): ICommand
{
  public CommandType Type => CommandType.ClearUnorderedAccess;
  public int SizeInBytes => sizeof(long) + sizeof(float) * 4;
}
