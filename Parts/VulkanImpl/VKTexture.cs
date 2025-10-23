using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;
using Resources;
using Resources.Enums;

namespace VulkanImpl;

public class VKTexture : ITexture
{
  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public string Name { get; }
  public ResourceType ResourceType { get; }
  public bool IsDisposed { get; }

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public IntPtr GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public TextureDescription Description { get; }
  public uint Width { get; }
  public uint Height { get; }
  public uint Depth { get; }
  public uint MipLevels { get; }
  public uint ArraySize { get; }
  public TextureFormat Format { get; }
  public uint SampleCount { get; }

  public ITextureView CreateView(TextureViewDescription _description)
  {
    throw new NotImplementedException();
  }

  public ITextureView GetDefaultShaderResourceView()
  {
    throw new NotImplementedException();
  }

  public ITextureView GetDefaultRenderTargetView()
  {
    throw new NotImplementedException();
  }

  public ITextureView GetDefaultDepthStencilView()
  {
    throw new NotImplementedException();
  }

  public ITextureView GetDefaultUnorderedAccessView()
  {
    throw new NotImplementedException();
  }

  public void SetData<T>(T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    throw new NotImplementedException();
  }

  public T[] GetData<T>(uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    throw new NotImplementedException();
  }

  public uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice)
  {
    throw new NotImplementedException();
  }

  public void GenerateMips()
  {
    throw new NotImplementedException();
  }
}
