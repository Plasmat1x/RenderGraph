namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс blend state
/// </summary>
public interface IBlendState: IResource
{
  BlendStateDescription Description { get; }
}