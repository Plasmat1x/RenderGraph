//using GraphicsAPI.Descriptions;
//using GraphicsAPI.Interfaces;

//using Resources.Enums;

//using Silk.NET.Direct3D12;

//namespace Directx12Impl;

//public class _DX12TextureView: ITextureView
//{
//  private _DX12Texture p_texture;
//  private TextureViewType p_viewType;
//  private _TextureViewDescription p_description;
//  private DescriptorAllocation p_allcation;
//  private bool p_disposed;

//  public _DX12TextureView(
//    _DX12Texture _texture,
//    TextureViewType _viewType,
//    _TextureViewDescription _desc,
//    DescriptorAllocation _allocation)
//  {
//    p_texture = _texture ?? throw new ArgumentNullException(nameof(_texture));
//    p_viewType = _viewType;
//    p_description = _desc ?? throw new ArgumentNullException(nameof(_desc));
//    p_allcation = _allocation ?? throw new ArgumentNullException(nameof(_allocation));
//  }

//  public ITexture Texture => p_texture;

//  public TextureViewType ViewType => p_viewType;

//  public _TextureViewDescription Description => p_description;

//  public IntPtr GetNativeHandle()
//  {
//    ThrowIfDisposed();
//    return (IntPtr)p_allcation.CpuHandle.Ptr;
//  }

//  public CpuDescriptorHandle GetDescriptorHandle()
//  {
//    ThrowIfDisposed();
//    return p_allcation.CpuHandle;
//  }

//  public void Dispose()
//  {
//    if(p_disposed)
//      return;

//    p_allcation?.Dispose();

//    p_disposed = true;
//    GC.SuppressFinalize(this);
//  }

//  private void ThrowIfDisposed()
//  {
//    if(p_disposed)
//      throw new ObjectDisposedException(nameof(_DX12TextureView));
//  }

//  internal CpuDescriptorHandle GetRenderTargetView()
//  {
//    throw new NotImplementedException();
//  }

//  internal CpuDescriptorHandle? GetDepthStencilView()
//  {
//    throw new NotImplementedException();
//  }
//}
