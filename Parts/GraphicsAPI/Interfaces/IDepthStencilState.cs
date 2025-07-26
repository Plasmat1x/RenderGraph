using GraphicsAPI.Descriptions;

namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс depth stencil state
/// </summary>
public interface IDepthStencilState: IResource
{
  DepthStencilStateDescription Description { get; }
}