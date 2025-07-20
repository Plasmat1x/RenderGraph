using GraphicsAPI.Descriptions;

namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс семплера
/// </summary>
public interface ISampler: IResource
{
  SamplerDescription Description { get; }
}