using Directx12Impl.Extensions;
using Directx12Impl.Parts;
using Directx12Impl.Parts.Managers;
using Directx12Impl.Parts.Utils;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Directx12Impl;

/// <summary>
/// Класс для работы с DX12 представлениями текстур
/// </summary>
public unsafe class DX12TextureView: ITextureView
{
  private readonly DX12Texture p_texture;
  private readonly TextureViewDescription p_description;
  private readonly TextureViewType p_viewType;
  private readonly DX12DescriptorHandle p_descriptorHandle;
  private bool p_disposed;

  public DX12TextureView(DX12Texture _texture, TextureViewDescription _description, DX12DescriptorHeapManager _descriptorManager)
  {
    p_texture = _texture ?? throw new ArgumentNullException(nameof(_texture));
    p_description = _description;
    p_viewType = _description.ViewType;

    p_descriptorHandle = CreateDescriptor(_descriptorManager);
  }

  public DX12TextureView(DX12Texture _texture, TextureViewType _viewType, TextureViewDescription _description, DX12DescriptorAllocation _allocation)
  {
    p_texture = _texture ?? throw new ArgumentNullException(nameof(_texture));
    p_description = _description;
    p_viewType = _viewType;

    p_descriptorHandle = new(_allocation.CpuHandle, _allocation.GpuHandle);
  }

  public ITexture Texture => p_texture;
  public TextureViewType ViewType => p_viewType;
  public TextureViewDescription Description => p_description;
  public bool IsDisposed => p_disposed;

  /// <summary>
  /// Получить handle дескриптора
  /// </summary>
  public DX12DescriptorHandle GetDescriptorHandle() => p_descriptorHandle;

  /// <summary>
  /// Получить связанный DX12 ресурс
  /// </summary>
  public ID3D12Resource* GetResource() => p_texture.GetResource();
  public nint GetNativeHandle() => (nint)p_descriptorHandle.CpuHandle.Ptr;
  
  private DX12DescriptorHandle CreateDescriptor(DX12DescriptorHeapManager _descriptorManager)
  {
    switch(p_viewType)
    {
      case TextureViewType.ShaderResource:
        return CreateShaderResourceView(_descriptorManager);
      case TextureViewType.RenderTarget:
        return CreateRenderTargetView(_descriptorManager);
      case TextureViewType.DepthStencil:
        return CreateDepthStencilView(_descriptorManager);
      case TextureViewType.UnorderedAccess:
        return CreateUnorderedAccessView(_descriptorManager);
      default:
        throw new ArgumentException($"Unsupported view type: {p_viewType}");
    }
  }

  private DX12DescriptorHandle CreateShaderResourceView(DX12DescriptorHeapManager _descriptorManager)
  {
    var allocation = _descriptorManager.AllocateCBVSRVUAV();

    var srvDesc = new ShaderResourceViewDesc
    {
      Format = p_description.Format.Convert(),
      ViewDimension = GetSRVDimension(),
      Shader4ComponentMapping = DX12Helpers.D3D12DefaultShader4ComponentMapping
    };

    FillSRVDescription(ref srvDesc);

    p_texture.GetResource()->GetDevice(out ComPtr<ID3D12Device> device);
    device.CreateShaderResourceView(p_texture.GetResource(), &srvDesc, allocation.CpuHandle);

    return new DX12DescriptorHandle(allocation.CpuHandle, allocation.GpuHandle);
  }

  private DX12DescriptorHandle CreateRenderTargetView(DX12DescriptorHeapManager _descriptorManager)
  {
    var allocation = _descriptorManager.AllocateRTV();

    var rtvDesc = new RenderTargetViewDesc
    {
      Format = p_description.Format.Convert(),
      ViewDimension = GetRTVDimension()
    };

    FillRTVDescription(ref rtvDesc);

    p_texture.GetResource()->GetDevice(out ComPtr<ID3D12Device> device);
    device.CreateRenderTargetView(p_texture.GetResource(), &rtvDesc, allocation.CpuHandle);

    return new DX12DescriptorHandle(allocation.CpuHandle);
  }

  private DX12DescriptorHandle CreateDepthStencilView(DX12DescriptorHeapManager _descriptorManager)
  {
    var allocation = _descriptorManager.AllocateDSV();

    var dsvDesc = new DepthStencilViewDesc
    {
      Format = p_description.Format.Convert().GetDepthSRVFormat(),
      ViewDimension = GetDSVDimension(),
      Flags = DsvFlags.None
    };

    FillDSVDescription(ref dsvDesc);

    p_texture.GetResource()->GetDevice(out ComPtr<ID3D12Device> device);
    device.CreateDepthStencilView(p_texture.GetResource(), &dsvDesc, allocation.CpuHandle);

    return new DX12DescriptorHandle(allocation.CpuHandle);
  }

  private DX12DescriptorHandle CreateUnorderedAccessView(DX12DescriptorHeapManager _descriptorManager)
  {
    var allocation = _descriptorManager.AllocateCBVSRVUAV();

    var uavDesc = new UnorderedAccessViewDesc
    {
      Format = p_description.Format.Convert(),
      ViewDimension = GetUAVDimension()
    };

    FillUAVDescription(ref uavDesc);

    p_texture.GetResource()->GetDevice(out ComPtr<ID3D12Device> device);
    device.CreateUnorderedAccessView(p_texture.GetResource(), (ID3D12Resource*)null, &uavDesc, allocation.CpuHandle);

    return new DX12DescriptorHandle(allocation.CpuHandle, allocation.GpuHandle);
  }

  private SrvDimension GetSRVDimension()
  {
    if(p_texture.ArraySize > 1)
    {
      if(p_texture.Depth > 1)
        throw new NotSupportedException("3D texture arrays are not supported");

      return (p_texture.Description.MiscFlags & ResourceMiscFlags.TextureCube) != 0
        ? SrvDimension.Texturecubearray
        : SrvDimension.Texture2Darray;
    }

    if(p_texture.Depth > 1)
      return SrvDimension.Texture3D;
    else if(p_texture.Height > 1)
      return SrvDimension.Texture2D;
    else
      return SrvDimension.Texture1D;
  }

  private RtvDimension GetRTVDimension()
  {
    if(p_texture.ArraySize > 1)
      return RtvDimension.Texture2Darray;
    else if(p_texture.Height > 1)
      return RtvDimension.Texture2D;
    else
      return RtvDimension.Texture1D;
  }

  private DsvDimension GetDSVDimension()
  {
    if(p_texture.ArraySize > 1)
      return DsvDimension.Texture2Darray;
    else
      return DsvDimension.Texture2D;
  }

  private UavDimension GetUAVDimension()
  {
    if(p_texture.ArraySize > 1)
      return UavDimension.Texture2Darray;
    else if(p_texture.Depth > 1)
      return UavDimension.Texture3D;
    else if(p_texture.Height > 1)
      return UavDimension.Texture2D;
    else
      return UavDimension.Texture1D;
  }

  private void FillSRVDescription(ref ShaderResourceViewDesc _desc)
  {
    switch(_desc.ViewDimension)
    {
      case SrvDimension.Texture1D:
        _desc.Anonymous.Texture1D.MostDetailedMip = p_description.MostDetailedMip;
        _desc.Anonymous.Texture1D.MipLevels = p_description.MipLevels;
        _desc.Anonymous.Texture1D.ResourceMinLODClamp = 0.0f;
        break;

      case SrvDimension.Texture2D:
        _desc.Anonymous.Texture2D.MostDetailedMip = p_description.MostDetailedMip;
        _desc.Anonymous.Texture2D.MipLevels = p_description.MipLevels;
        _desc.Anonymous.Texture2D.PlaneSlice = 0;
        _desc.Anonymous.Texture2D.ResourceMinLODClamp = 0.0f;
        break;

      case SrvDimension.Texture3D:
        _desc.Anonymous.Texture3D.MostDetailedMip = p_description.MostDetailedMip;
        _desc.Anonymous.Texture3D.MipLevels = p_description.MipLevels;
        _desc.Anonymous.Texture3D.ResourceMinLODClamp = 0.0f;
        break;

      case SrvDimension.Texture2Darray:
        _desc.Anonymous.Texture2DArray.MostDetailedMip = p_description.MostDetailedMip;
        _desc.Anonymous.Texture2DArray.MipLevels = p_description.MipLevels;
        _desc.Anonymous.Texture2DArray.FirstArraySlice = p_description.FirstArraySlice;
        _desc.Anonymous.Texture2DArray.ArraySize = p_description.ArraySize;
        _desc.Anonymous.Texture2DArray.PlaneSlice = 0;
        _desc.Anonymous.Texture2DArray.ResourceMinLODClamp = 0.0f;
        break;

      case SrvDimension.Texturecube:
        _desc.Anonymous.TextureCube.MostDetailedMip = p_description.MostDetailedMip;
        _desc.Anonymous.TextureCube.MipLevels = p_description.MipLevels;
        _desc.Anonymous.TextureCube.ResourceMinLODClamp = 0.0f;
        break;

      case SrvDimension.Texturecubearray:
        _desc.Anonymous.TextureCubeArray.MostDetailedMip = p_description.MostDetailedMip;
        _desc.Anonymous.TextureCubeArray.MipLevels = p_description.MipLevels;
        _desc.Anonymous.TextureCubeArray.First2DArrayFace = p_description.FirstArraySlice;
        _desc.Anonymous.TextureCubeArray.NumCubes = p_description.ArraySize / 6;
        _desc.Anonymous.TextureCubeArray.ResourceMinLODClamp = 0.0f;
        break;
    }
  }

  private void FillRTVDescription(ref RenderTargetViewDesc _desc)
  {
    switch(_desc.ViewDimension)
    {
      case RtvDimension.Texture1D:
        _desc.Anonymous.Texture1D.MipSlice = p_description.MostDetailedMip;
        break;

      case RtvDimension.Texture2D:
        _desc.Anonymous.Texture2D.MipSlice = p_description.MostDetailedMip;
        _desc.Anonymous.Texture2D.PlaneSlice = 0;
        break;

      case RtvDimension.Texture3D:
        _desc.Anonymous.Texture3D.MipSlice = p_description.MostDetailedMip;
        _desc.Anonymous.Texture3D.FirstWSlice = 0;
        _desc.Anonymous.Texture3D.WSize = p_texture.Depth;
        break;

      case RtvDimension.Texture2Darray:
        _desc.Anonymous.Texture2DArray.MipSlice = p_description.MostDetailedMip;
        _desc.Anonymous.Texture2DArray.FirstArraySlice = p_description.FirstArraySlice;
        _desc.Anonymous.Texture2DArray.ArraySize = p_description.ArraySize;
        _desc.Anonymous.Texture2DArray.PlaneSlice = 0;
        break;
    }
  }

  private void FillDSVDescription(ref DepthStencilViewDesc _desc)
  {
    switch(_desc.ViewDimension)
    {
      case DsvDimension.Texture1D:
        _desc.Anonymous.Texture1D.MipSlice = p_description.MostDetailedMip;
        break;

      case DsvDimension.Texture2D:
        _desc.Anonymous.Texture2D.MipSlice = p_description.MostDetailedMip;
        break;

      case DsvDimension.Texture2Darray:
        _desc.Anonymous.Texture2DArray.MipSlice = p_description.MostDetailedMip;
        _desc.Anonymous.Texture2DArray.FirstArraySlice = p_description.FirstArraySlice;
        _desc.Anonymous.Texture2DArray.ArraySize = p_description.ArraySize;
        break;
    }
  }

  private void FillUAVDescription(ref UnorderedAccessViewDesc _desc)
  {
    switch(_desc.ViewDimension)
    {
      case UavDimension.Texture1D:
        _desc.Anonymous.Texture1D.MipSlice = p_description.MostDetailedMip;
        break;

      case UavDimension.Texture2D:
        _desc.Anonymous.Texture2D.MipSlice = p_description.MostDetailedMip;
        _desc.Anonymous.Texture2D.PlaneSlice = 0;
        break;

      case UavDimension.Texture3D:
        _desc.Anonymous.Texture3D.MipSlice = p_description.MostDetailedMip;
        _desc.Anonymous.Texture3D.FirstWSlice = 0;
        _desc.Anonymous.Texture3D.WSize = p_texture.Depth;
        break;

      case UavDimension.Texture2Darray:
        _desc.Anonymous.Texture2DArray.MipSlice = p_description.MostDetailedMip;
        _desc.Anonymous.Texture2DArray.FirstArraySlice = p_description.FirstArraySlice;
        _desc.Anonymous.Texture2DArray.ArraySize = p_description.ArraySize;
        _desc.Anonymous.Texture2DArray.PlaneSlice = 0;
        break;
    }
  }

  public void Dispose()
  {
    if(!p_disposed)
    {
      // Дескрипторы освобождаются автоматически через DescriptorHeapManager
      p_disposed = true;
    }
  }


}