using Resources.Enums;

namespace Resources.Interfaces;

public interface IResourceDescription
{
  public abstract string Name { get; }
  public abstract ResourceUsage Usage { get; }
  public abstract BindFlags BindFlags { get; }
  public abstract CPUAccessFlags CPUAcessFalgs{get;}
  public abstract ResourceMiscFlags ResourceMiscFlags { get; }

  public abstract ulong GetMemorySize();

  public abstract bool IsComaptible(IResourceDescription _other);

}
