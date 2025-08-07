using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Enums;

namespace GraphicsAPI.Commands;

// === Resource Transition Commands ===

public record TransitionResourceCommand(IResource Resource, ResourceState NewState): ICommand
{
  public CommandType Type => CommandType.TransitionResource;
  public int SizeInBytes => sizeof(long) + sizeof(int);
}
