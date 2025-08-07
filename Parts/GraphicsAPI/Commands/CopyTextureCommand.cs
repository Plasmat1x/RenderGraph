using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

// === Copy Commands ===

public record CopyTextureCommand(ITexture Src, ITexture Dst): ICommand
{
  public CommandType Type => CommandType.CopyTexture;
  public int SizeInBytes => sizeof(long) * 2;
}
