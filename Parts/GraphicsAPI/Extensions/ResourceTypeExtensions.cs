using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace GraphicsAPI.Extensions;


/// <summary>
/// Исправления для интерфейсов ресурсов - добавляем правильный ResourceType
/// </summary>
public static class ResourceTypeExtensions
{
  /// <summary>
  /// Получить правильный ResourceType для текстуры
  /// </summary>
  public static ResourceType GetResourceType(this TextureDescription _description)
  {
    if((_description.MiscFlags & ResourceMiscFlags.TextureCube) != 0)
    {
      return _description.ArraySize > 6 ? ResourceType.TextureCubeArray : ResourceType.TextureCube;
    }

    if(_description.Depth > 1)
      return ResourceType.Texture3D;

    if(_description.Height > 1)
    {
      if(_description.SampleCount > 1)
        return _description.ArraySize > 1 ? ResourceType.Texture2DMSArray : ResourceType.Texture2DMS;
      else
        return _description.ArraySize > 1 ? ResourceType.Texture2DArray : ResourceType.Texture2D;
    }

    return _description.ArraySize > 1 ? ResourceType.Texture1DArray : ResourceType.Texture1D;
  }

  /// <summary>
  /// Получить правильный ResourceType для буфера
  /// </summary>
  public static ResourceType GetResourceType(this BufferDescription _description)
  {
    return _description.BufferUsage switch
    {
      BufferUsage.Vertex => ResourceType.VertexBuffer,
      BufferUsage.Index => ResourceType.IndexBuffer,
      BufferUsage.Constant => ResourceType.ConstantBuffer,
      BufferUsage.Structured => ResourceType.StructuredBuffer,
      BufferUsage.Raw => ResourceType.RawBuffer,
      BufferUsage.IndirectArgs => ResourceType.IndirectArgsBuffer,
      _ => ResourceType.Buffer
    };
  }

  /// <summary>
  /// Получить ResourceType для шейдера
  /// </summary>
  public static ResourceType GetResourceType(this IShader _shader)
  {
    return ResourceType.Shader;
  }

  /// <summary>
  /// Получить ResourceType для render state
  /// </summary>
  public static ResourceType GetResourceType(this IRenderState _renderState)
  {
    return ResourceType.RenderState;
  }

  /// <summary>
  /// Получить ResourceType для sampler
  /// </summary>
  public static ResourceType GetResourceType(this ISampler _sampler)
  {
    return ResourceType.Sampler;
  }
}