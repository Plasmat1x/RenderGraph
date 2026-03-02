using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using Resources;
using Resources.Enums;

namespace VulkanImpl;

public class VKTexture : ITexture
{
  private readonly TextureDescription p_description;
  private bool p_disposed;

  public VKTexture(TextureDescription _description)
  {
    p_description = _description;
  }

  public TextureDescription Description => p_description;
  public string Name => p_description.Name;
  public ResourceType ResourceType => ResourceType.Texture2D;
  public bool IsDisposed => p_disposed;
  public uint Width => p_description.Width;
  public uint Height => p_description.Height;
  public uint Depth => p_description.Depth;
  public uint MipLevels => p_description.MipLevels;
  public uint ArraySize => p_description.ArraySize;
  public uint ArrayLayers => p_description.ArraySize;
  public TextureFormat Format => p_description.Format;
  public uint SampleCount => 1;
  public bool IsMapped => false;

  public uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice) => _mipLevel * ArraySize + _arraySlice;

  public ITextureView CreateView(TextureViewDescription _description)
  {
    throw new NotImplementedException("VKTexture.CreateView not implemented");
  }

  public ITextureView GetDefaultShaderResourceView()
  {
    throw new NotImplementedException("VKTexture.GetDefaultShaderResourceView not implemented");
  }

  public ITextureView GetDefaultRenderTargetView()
  {
    throw new NotImplementedException("VKTexture.GetDefaultRenderTargetView not implemented");
  }

  public ITextureView GetDefaultDepthStencilView()
  {
    throw new NotImplementedException("VKTexture.GetDefaultDepthStencilView not implemented");
  }

  public ITextureView GetDefaultUnorderedAccessView()
  {
    throw new NotImplementedException("VKTexture.GetDefaultUnorderedAccessView not implemented");
  }

  public void SetData<T>(T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    throw new NotImplementedException("VKTexture.SetData not implemented");
  }

  public T[] GetData<T>(uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    throw new NotImplementedException("VKTexture.GetData not implemented");
  }

  public void GenerateMips()
  {
    throw new NotImplementedException("VKTexture.GenerateMips not implemented");
  }

  public IntPtr GetNativeHandle() => IntPtr.Zero;
  public ulong GetMemorySize() => Width * Height * Depth * 4u;

  public void Dispose()
  {
    if(p_disposed)
      return;
    p_disposed = true;
  }
}
