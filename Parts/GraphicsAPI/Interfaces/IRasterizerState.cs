namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс rasterizer state
/// </summary>
public interface IRasterizerState: IResource
{
  RasterizerStateDescription Description { get; }
}