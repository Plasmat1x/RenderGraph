using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

using System.Numerics;

namespace GraphicsAPI.Commands;

// === Clear Commands ===

public record ClearRenderTargetCommand(ITextureView Target, Vector4 Color): ICommand
{
  public CommandType Type => CommandType.ClearRenderTarget;
  public int SizeInBytes => sizeof(long) + sizeof(float) * 4;
}
