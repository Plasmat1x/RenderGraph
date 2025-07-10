using Resources;
using Resources.Enums;

namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс текстуры
/// </summary>
public interface ITexture: IResource
{
  TextureDescription Description { get; }
  uint Width { get; }
  uint Height { get; }
  uint Depth { get; }
  uint MipLevels { get; }
  uint ArraySize { get; }
  TextureFormat Format { get; }
  uint SampleCount { get; }

  // Views
  ITextureView CreateView(TextureViewDescription description);
  ITextureView GetDefaultShaderResourceView();
  ITextureView GetDefaultRenderTargetView();
  ITextureView GetDefaultDepthStencilView();
  ITextureView GetDefaultUnorderedAccessView();

  // Данные
  void SetData<T>(T[] data, uint mipLevel = 0, uint arraySlice = 0) where T : struct;
  T[] GetData<T>(uint mipLevel = 0, uint arraySlice = 0) where T : struct;

  // Субресурсы
  uint GetSubresourceIndex(uint mipLevel, uint arraySlice);
  void GenerateMips();
}
