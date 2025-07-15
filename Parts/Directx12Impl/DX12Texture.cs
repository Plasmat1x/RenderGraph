using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Directx12Impl;

public class DX12Texture: ITexture
{
  private ComPtr<ID3D12Resource> p_resource;
  private TextureDescription p_description;
  private Format p_dxgiFormat;
  private ResourceDesc p_resourceDesc;
  private ResourceStates p_currentState;
  private ComPtr<ID3D12Device> p_device;
  private CpuDescriptorHandle p_srvDescriptor;
  private CpuDescriptorHandle p_rtvDescriptor;
  private CpuDescriptorHandle p_dsvDescriptor;
  private CpuDescriptorHandle p_uavDescriptor;
  private Dictionary<TextureViewDescription, DX12TextureView> P_views;

  public DX12Texture()
  {
    
  }

  public TextureDescription Description => p_description;

  public uint Width => throw new NotImplementedException();

  public uint Height => throw new NotImplementedException();

  public uint Depth => throw new NotImplementedException();

  public uint MipLevels => throw new NotImplementedException();

  public uint ArraySize => throw new NotImplementedException();

  public TextureFormat Format => throw new NotImplementedException();

  public uint SampleCount => throw new NotImplementedException();

  public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

  public ResourceType ResourceType => throw new NotImplementedException();

  public bool IsDisposed => throw new NotImplementedException();

  public ITextureView CreateView(TextureViewDescription _description)
  {
    throw new NotImplementedException();
  }

  public void GenerateMips()
  {
    throw new NotImplementedException();
  }

  public T[] GetData<T>(uint _mipLevel = 0, uint _arraySlice = 0) where T : struct
  {
    throw new NotImplementedException();
  }

  public ITextureView GetDefaultDepthStencilView()
  {
    throw new NotImplementedException();
  }

  public ITextureView GetDefaultRenderTargetView()
  {
    throw new NotImplementedException();
  }

  public ITextureView GetDefaultShaderResourceView()
  {
    throw new NotImplementedException();
  }

  public ITextureView GetDefaultUnorderedAccessView()
  {
    throw new NotImplementedException();
  }

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice)
  {
    throw new NotImplementedException();
  }

  public void SetData<T>(T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : struct
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  private Format ConvertFormat(TextureFormat _format)
  {
    throw new NotImplementedException();
  }

  private void CreateResource()
  {
    throw new NotImplementedException();
  }

  private void CreateDefaultViews()
  {
    throw new NotImplementedException();
  }
}