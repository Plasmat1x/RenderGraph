using GraphicsAPI.Commands.enums;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Enums;

namespace GraphicsAPI.Commands;

public record TransitionResourcesCommand(IResource[] Resources, ResourceState[] NewStates): ICommand
{
  public CommandType Type => CommandType.TransitionResources;
  public int SizeInBytes => sizeof(long) * (Resources?.Length ?? 0) + sizeof(int) * (NewStates?.Length ?? 0);
}
