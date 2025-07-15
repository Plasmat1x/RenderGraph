using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Directx12Impl;

public class DX12TextureView: ITextureView
{
  private DX12Texture p_texture;
  private TextureViewType p_viewType;
  private TextureViewDescription p_description;
  private CpuDescriptorHandle p_dsecriptor;
  private Format p_format;

  public DX12TextureView()
  {

  }

  public ITexture Texture => throw new NotImplementedException();

  public TextureViewType ViewType => throw new NotImplementedException();

  public TextureViewDescription Description => throw new NotImplementedException();

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  private void CreateView()
  {
    throw new NotImplementedException();
  }
}
