using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Interfaces;

namespace GraphicsAPI.Commands;

// === Render Target Commands ===

public record SetRenderTargetsCommand(ITextureView[] ColorTargets, ITextureView DepthTarget): ICommand
{
  public CommandType Type => CommandType.SetRenderTargets;
  public int SizeInBytes => sizeof(long) * (ColorTargets?.Length ?? 0) + sizeof(long);
}
