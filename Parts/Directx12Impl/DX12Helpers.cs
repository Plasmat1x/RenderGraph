using GraphicsAPI.Enums;

using Resources.Enums;

using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System;

namespace Directx12Impl;
public static class DX12Helpers
{
  public static Format ConvertFormat(TextureFormat _format) => _format switch
  {
    TextureFormat.Unknown => Format.FormatUnknown,
    TextureFormat.R32G32B32A32_TYPELESS => Format.FormatR32G32B32A32Typeless,
    TextureFormat.R32G32B32A32_FLOAT => Format.FormatR32G32B32A32Float,
    TextureFormat.R32G32B32A32_UINT => Format.FormatR32G32B32A32Uint,
    TextureFormat.R32G32B32A32_SINT => Format.FormatR32G32B32A32Sint,
    TextureFormat.R8G8B8A8_TYPELESS => Format.FormatR8G8B8A8Typeless,
    TextureFormat.R8G8B8A8_UNORM => Format.FormatR8G8B8A8Unorm,
    TextureFormat.R8G8B8A8_UNORM_SRGB => Format.FormatR8G8B8A8UnormSrgb,
    TextureFormat.R8G8B8A8_UINT => Format.FormatR8G8B8A8Uint,
    TextureFormat.R8G8B8A8_SNORM => Format.FormatR8G8B8A8SNorm,
    TextureFormat.R8G8B8A8_SINT => Format.FormatR8G8B8A8Sint,
    TextureFormat.D32_FLOAT => Format.FormatD32Float,
    TextureFormat.D24_UNORM_S8_UINT => Format.FormatD24UnormS8Uint,
    TextureFormat.BC1_TYPELESS => Format.FormatBC1Typeless,
    TextureFormat.BC1_UNORM => Format.FormatBC1Unorm,
    TextureFormat.BC1_UNORM_SRGB => Format.FormatBC1UnormSrgb,
    TextureFormat.BC3_TYPELESS => Format.FormatBC3Typeless,
    TextureFormat.BC3_UNORM => Format.FormatBC3Unorm,
    TextureFormat.BC3_UNORM_SRGB => Format.FormatBC3UnormSrgb,
    TextureFormat.BC7_TYPELESS => Format.FormatBC7Typeless,
    TextureFormat.BC7_UNORM => Format.FormatBC7Unorm,
    TextureFormat.BC7_UNORM_SRGB => Format.FormatBC7UnormSrgb,
    _ => throw new ArgumentException($"Unsupported texture format: {_format}")
  };

  public static TextureFormat ConvertFormat(Format _format) => _format switch
  {
    Format.FormatUnknown => TextureFormat.Unknown,
    Format.FormatR32G32B32A32Typeless => TextureFormat.R32G32B32A32_TYPELESS,
    Format.FormatR32G32B32A32Float => TextureFormat.R32G32B32A32_FLOAT,
    Format.FormatR32G32B32A32Uint => TextureFormat.R32G32B32A32_UINT,
    Format.FormatR32G32B32A32Sint => TextureFormat.R32G32B32A32_SINT,
    Format.FormatR8G8B8A8Typeless => TextureFormat.R8G8B8A8_TYPELESS,
    Format.FormatR8G8B8A8Unorm => TextureFormat.R8G8B8A8_UNORM,
    Format.FormatR8G8B8A8UnormSrgb => TextureFormat.R8G8B8A8_UNORM_SRGB,
    Format.FormatR8G8B8A8Uint => TextureFormat.R8G8B8A8_UINT,
    Format.FormatR8G8B8A8SNorm => TextureFormat.R8G8B8A8_SNORM,
    Format.FormatR8G8B8A8Sint => TextureFormat.R8G8B8A8_SINT,
    Format.FormatD32Float => TextureFormat.D32_FLOAT,
    Format.FormatD24UnormS8Uint => TextureFormat.D24_UNORM_S8_UINT,
    Format.FormatBC1Typeless => TextureFormat.BC1_TYPELESS,
    Format.FormatBC1Unorm => TextureFormat.BC1_UNORM,
    Format.FormatBC1UnormSrgb => TextureFormat.BC1_UNORM_SRGB,
    Format.FormatBC3Typeless => TextureFormat.BC3_TYPELESS,
    Format.FormatBC3Unorm => TextureFormat.BC3_UNORM,
    Format.FormatBC3UnormSrgb => TextureFormat.BC3_UNORM_SRGB,
    Format.FormatBC7Typeless => TextureFormat.BC7_TYPELESS,
    Format.FormatBC7Unorm => TextureFormat.BC7_UNORM,
    Format.FormatBC7UnormSrgb => TextureFormat.BC7_UNORM_SRGB,
    _ => throw new ArgumentException($"Unsupported texture format: {_format}")
  };

  public static ResourceStates ConvertResourceState(ResourceState _state) => _state switch
  {
    ResourceState.Undefined => ResourceStates.Common,
    ResourceState.Common => ResourceStates.Common,
    ResourceState.RenderTarget => ResourceStates.RenderTarget,
    ResourceState.UnorderedAccess => ResourceStates.UnorderedAccess,
    ResourceState.DepthWrite => ResourceStates.DepthWrite,
    ResourceState.DepthRead => ResourceStates.DepthRead,
    ResourceState.ShaderResource => ResourceStates.PixelShaderResource | ResourceStates.NonPixelShaderResource,
    ResourceState.StreamOut => ResourceStates.StreamOut,
    ResourceState.IndirectArgument => ResourceStates.IndirectArgument,
    ResourceState.CopyDestination => ResourceStates.CopyDest,
    ResourceState.CopySource => ResourceStates.CopySource,
    ResourceState.ResolveDestination => ResourceStates.ResolveDest,
    ResourceState.ResolveSource => ResourceStates.ResolveSource,
    ResourceState.Present => ResourceStates.Present,
    _ => throw new ArgumentException($"Unsupported resource state: {_state}")
  };

  public static CommandListType ConvertCommandListType(CommandBufferType _type) => _type switch
  {
    CommandBufferType.Direct => CommandListType.Direct,
    CommandBufferType.Bundle => CommandListType.Bundle,
    CommandBufferType.Compute => CommandListType.Compute,
    CommandBufferType.Copy => CommandListType.Copy,
    _ => throw new ArgumentException($"Unsupported command buffer type: {_type}")
  };

  public static Filter ConvertFilter(FilterMode _minFilter, FilterMode _magFilter, FilterMode _mipFilter, bool _isComparison = false)
  {
    var filter = Filter.MinMagMipPoint;

    bool isMinLinear = _minFilter == FilterMode.Linear;
    bool isMagLinear = _magFilter == FilterMode.Linear;
    bool isMipLinear = _mipFilter == FilterMode.Linear;

    if(!isMinLinear && !isMagLinear && !isMipLinear)
      filter = Filter.MinMagMipPoint;
    else if(isMinLinear && !isMagLinear && !isMipLinear)
      filter = Filter.MinLinearMagMipPoint;
    else if(!isMinLinear && isMagLinear && !isMipLinear)
      filter = Filter.MinPointMagLinearMipPoint;
    else if(isMinLinear && isMagLinear && !isMipLinear)
      filter = Filter.MinMagLinearMipPoint;
    else if(!isMinLinear && !isMagLinear && isMipLinear)
      filter = Filter.MinMagPointMipLinear;
    else if(isMinLinear && !isMagLinear && isMipLinear)
      filter = Filter.MinLinearMagPointMipLinear;
    else if(!isMinLinear && isMagLinear && isMipLinear)
      filter = Filter.MinPointMagMipLinear;
    else if(isMinLinear && isMagLinear && isMipLinear)
      filter = Filter.MinMagMipLinear;

    if(_isComparison)
    {
      filter = (Filter)((int)filter | 0x80);
    }

    return filter;
  }

  public static TextureAddressMode ConvertAddressMode(AddressMode _mode) => _mode switch
  {
    AddressMode.Wrap => TextureAddressMode.Wrap,
    AddressMode.Mirror => TextureAddressMode.Mirror,
    AddressMode.Clamp => TextureAddressMode.Clamp,
    AddressMode.Border => TextureAddressMode.Border,
    AddressMode.MirrorOnce => TextureAddressMode.MirrorOnce,
    _ => throw new ArgumentException($"Unsupported texture address mode: {_mode}")
  };

  public static ComparisonFunc ConvertCompariosonFunc(ComparisonFunction _func) => _func switch
  {
    ComparisonFunction.Never => ComparisonFunc.Never,
    ComparisonFunction.Less => ComparisonFunc.Less,
    ComparisonFunction.Equal => ComparisonFunc.Equal,
    ComparisonFunction.LessEqual => ComparisonFunc.LessEqual,
    ComparisonFunction.Greater => ComparisonFunc.Greater,
    ComparisonFunction.NotEqual => ComparisonFunc.NotEqual,
    ComparisonFunction.GreaterEqual => ComparisonFunc.GreaterEqual,
    ComparisonFunction.Always => ComparisonFunc.Always,
    _ => throw new ArgumentException($"Unsupported comparison function: {_func}")
  };

  public static HeapType ConvertHeapType(ResourceUsage _usage) => _usage switch
  {
    ResourceUsage.Default   => HeapType.Default,
    ResourceUsage.Immutable => HeapType.Default,
    ResourceUsage.Dynamic   => HeapType.Upload,
    ResourceUsage.Staging   => HeapType.Readback,
    _ => HeapType.Default,
  };

  public static ResourceFlags ConvertBindFlags(BindFlags _flags)
  {
    var result = ResourceFlags.None;

    if((_flags & BindFlags.RenderTarget) != 0)
      result |= ResourceFlags.AllowRenderTarget;
    if((_flags & BindFlags.DepthStencil) != 0)
      result |= ResourceFlags.AllowDepthStencil;
    if((_flags & BindFlags.UnorderedAccess) != 0)
      result |= ResourceFlags.AllowUnorderedAccess;

    if((_flags & BindFlags.DepthStencil) != 0)
      result |= ResourceFlags.DenyShaderResource;

    return result;
  }

  public static uint GetFormatSize(Format _format) => _format switch
  {
    Format.FormatR32G32B32A32Typeless or
                Format.FormatR32G32B32A32Float or
                Format.FormatR32G32B32A32Uint or
                Format.FormatR32G32B32A32Sint => 16,

    Format.FormatR32G32B32Typeless or
    Format.FormatR32G32B32Float or
    Format.FormatR32G32B32Uint or
    Format.FormatR32G32B32Sint => 12,

    Format.FormatR16G16B16A16Typeless or
    Format.FormatR16G16B16A16Float or
    Format.FormatR16G16B16A16Unorm or
    Format.FormatR16G16B16A16Uint or
    Format.FormatR16G16B16A16SNorm or
    Format.FormatR16G16B16A16Sint or
    Format.FormatR32G32Typeless or
    Format.FormatR32G32Float or
    Format.FormatR32G32Uint or
    Format.FormatR32G32Sint => 8,

    Format.FormatR8G8B8A8Typeless or
    Format.FormatR8G8B8A8Unorm or
    Format.FormatR8G8B8A8UnormSrgb or
    Format.FormatR8G8B8A8Uint or
    Format.FormatR8G8B8A8SNorm or
    Format.FormatR8G8B8A8Sint or
    Format.FormatB8G8R8A8Unorm or
    Format.FormatB8G8R8X8Unorm or
    Format.FormatB8G8R8A8Typeless or
    Format.FormatB8G8R8A8UnormSrgb or
    Format.FormatB8G8R8X8Typeless or
    Format.FormatB8G8R8X8UnormSrgb or
    Format.FormatR10G10B10A2Typeless or
    Format.FormatR10G10B10A2Unorm or
    Format.FormatR10G10B10A2Uint or
    Format.FormatR11G11B10Float or
    Format.FormatR16G16Typeless or
    Format.FormatR16G16Float or
    Format.FormatR16G16Unorm or
    Format.FormatR16G16Uint or
    Format.FormatR16G16SNorm or
    Format.FormatR16G16Sint or
    Format.FormatR32Typeless or
    Format.FormatD32Float or
    Format.FormatR32Float or
    Format.FormatR32Uint or
    Format.FormatR32Sint or
    Format.FormatR24G8Typeless or
    Format.FormatD24UnormS8Uint or
    Format.FormatR24UnormX8Typeless or
    Format.FormatX24TypelessG8Uint => 4,

    Format.FormatR8G8Typeless or
    Format.FormatR8G8Unorm or
    Format.FormatR8G8Uint or
    Format.FormatR8G8SNorm or
    Format.FormatR8G8Sint or
    Format.FormatR16Typeless or
    Format.FormatR16Float or
    Format.FormatD16Unorm or
    Format.FormatR16Unorm or
    Format.FormatR16Uint or
    Format.FormatR16SNorm or
    Format.FormatR16Sint => 2,

    Format.FormatR8Typeless or
    Format.FormatR8Unorm or
    Format.FormatR8Uint or
    Format.FormatR8SNorm or
    Format.FormatR8Sint or
    Format.FormatA8Unorm => 1,

    // Сжатые форматы (размер за блок 4x4)
    Format.FormatBC1Typeless or
    Format.FormatBC1Unorm or
    Format.FormatBC1UnormSrgb or
    Format.FormatBC4Typeless or
    Format.FormatBC4Unorm or
    Format.FormatBC4SNorm => 8,

    Format.FormatBC2Typeless or
    Format.FormatBC2Unorm or
    Format.FormatBC2UnormSrgb or
    Format.FormatBC3Typeless or
    Format.FormatBC3Unorm or
    Format.FormatBC3UnormSrgb or
    Format.FormatBC5Typeless or
    Format.FormatBC5Unorm or
    Format.FormatBC5SNorm or
    Format.FormatBC6HTypeless or
    Format.FormatBC6HUF16 or
    Format.FormatBC6HSF16 or
    Format.FormatBC7Typeless or
    Format.FormatBC7Unorm or
    Format.FormatBC7UnormSrgb => 16,

    _ => 0
  };

  public static bool IsCompressedFormat(Format _format) => _format >= Format.FormatBC1Typeless && _format <= Format.FormatBC7UnormSrgb;

  public static bool IsDepthStencilFormat(Format _format) => _format switch 
  {
    Format.FormatD32FloatS8X24Uint or
    Format.FormatD32Float or
    Format.FormatD24UnormS8Uint or
    Format.FormatD16Unorm => true,
    _ => false
  };

  public static Format GetDepthSRVFormat(Format _format) => _format switch
  {
    Format.FormatD32FloatS8X24Uint or
    Format.FormatR32G8X24Typeless => Format.FormatR32FloatX8X24Typeless,
    Format.FormatD32Float or
    Format.FormatR32Typeless => Format.FormatR32Float,
    Format.FormatD24UnormS8Uint or
    Format.FormatR24G8Typeless => Format.FormatR24UnormX8Typeless,
    Format.FormatD16Unorm or
    Format.FormatR16Typeless => Format.FormatR16Unorm,
    _ => _format
  };

  public static ulong AlignUp(ulong _size, ulong _alignment) => (_size + _alignment - 1) & ~(_alignment - 1);
  
  public static ulong CalculateTextureSize(uint _width, uint _height, uint _depth, uint _mipLevels, uint _arraySize, Format _format)
  {
    ulong totalSize = 0;
    uint bytesPerPixel = GetFormatSize(_format);
    bool isCompressed = IsCompressedFormat(_format);

    for(uint arraySlice = 0; arraySlice < _arraySize; arraySlice++)
    {
      uint w = _width;
      uint h = _height;
      uint d = _depth;

      for(uint mip = 0; mip < _mipLevels; mip++)
      {
        uint rowPitch;
        uint slicePitch;

        if(isCompressed)
        {
          uint blockWidth = Math.Max(1, (w + 3) / 4);
          uint blockHeight = Math.Max(1, (h + 3) / 4);
          rowPitch = blockWidth * bytesPerPixel;
          slicePitch = rowPitch * blockHeight;
        }
        else
        {
          rowPitch = w * bytesPerPixel;
          rowPitch = (uint)AlignUp(rowPitch, D3D12.TextureDataPitchAlignment);
          slicePitch = rowPitch * h;
        }

        totalSize += slicePitch * d;

        w = Math.Max(1, w / 2);
        h = Math.Max(1, h / 2);
        d = Math.Max(1, d / 2);
      }
    }

    return totalSize;
  }
}
