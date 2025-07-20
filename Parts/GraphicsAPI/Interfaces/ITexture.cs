using GraphicsAPI.Descriptions;

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
  ITextureView CreateView(TextureViewDescription _description);
  ITextureView GetDefaultShaderResourceView();
  ITextureView GetDefaultRenderTargetView();
  ITextureView GetDefaultDepthStencilView();
  ITextureView GetDefaultUnorderedAccessView();
  void SetData<T>(T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : struct;
  T[] GetData<T>(uint _mipLevel = 0, uint _arraySlice = 0) where T : struct;
  uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice);
  void GenerateMips();
}
