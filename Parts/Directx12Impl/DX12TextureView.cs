using GraphicsAPI.Descriptions;
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
  private DescriptorAllocation p_allcation;
  private bool p_disposed;

  public DX12TextureView(
    DX12Texture _texture,
    TextureViewType _viewType,
    TextureViewDescription _desc,
    DescriptorAllocation _allocation)
  {
    p_texture = _texture ?? throw new ArgumentNullException(nameof(_texture));
    p_viewType = _viewType;
    p_description = _desc ?? throw new ArgumentNullException(nameof(_desc));
    p_allcation = _allocation ?? throw new ArgumentNullException(nameof(_allocation));
  }

  public ITexture Texture => p_texture;

  public TextureViewType ViewType => p_viewType;

  public TextureViewDescription Description => p_description;

  public IntPtr GetNativeHandle()
  {
    ThrowIfDisposed();
    return (IntPtr)p_allcation.CpuHandle.Ptr;
  }

  public CpuDescriptorHandle GetDescriptorHandle()
  {
    ThrowIfDisposed();
    return p_allcation.CpuHandle;
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    p_allcation?.Dispose();

    p_disposed = true;
    GC.SuppressFinalize(this);
  }

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(DX12TextureView));
  }
}
