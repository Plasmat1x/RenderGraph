namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс состояния рендеринга
/// </summary>
public interface IRenderState: IResource
{
  RenderStateDescription Description { get; }
}