using Directx12Impl.Parts;

using Resources.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

/// <summary>
/// Расширения для DX12 текстурных представлений
/// </summary>
public static class DX12TextureViewExtensions
{
  /// <summary>
  /// Получить handle дескриптора для Render Target View
  /// </summary>
  public static CpuDescriptorHandle GetRenderTargetView(this DX12TextureView _view)
  {
    if(_view.ViewType != TextureViewType.RenderTarget)
      throw new InvalidOperationException("View is not a Render Target View");

    return _view.GetDescriptorHandle().CpuHandle;
  }

  /// <summary>
  /// Получить handle дескриптора для Depth Stencil View
  /// </summary>
  public static CpuDescriptorHandle GetDepthStencilView(this DX12TextureView _view)
  {
    if(_view.ViewType != TextureViewType.DepthStencil)
      throw new InvalidOperationException("View is not a Depth Stencil View");

    return _view.GetDescriptorHandle().CpuHandle;
  }

  /// <summary>
  /// Получить handle дескриптора для Shader Resource View
  /// </summary>
  public static DX12DescriptorHandle GetShaderResourceView(this DX12TextureView _view)
  {
    if(_view.ViewType != TextureViewType.ShaderResource)
      throw new InvalidOperationException("View is not a Shader Resource View");

    return _view.GetDescriptorHandle();
  }

  /// <summary>
  /// Получить handle дескриптора для Unordered Access View
  /// </summary>
  public static DX12DescriptorHandle GetUnorderedAccessView(this DX12TextureView _view)
  {
    if(_view.ViewType != TextureViewType.UnorderedAccess)
      throw new InvalidOperationException("View is not an Unordered Access View");

    return _view.GetDescriptorHandle();
  }
}
