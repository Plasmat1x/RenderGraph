using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System;

namespace Directx12Impl;
public static class DX12Helpers
{
  /// <summary>
  /// Константа для стандартного маппинга компонентов шейдера
  /// </summary>
  public const uint D3D12DefaultShader4ComponentMapping = 0x1688; // D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING

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
    ResourceState.Common => ResourceStates.Common,
    ResourceState.RenderTarget => ResourceStates.RenderTarget,
    ResourceState.UnorderedAccess => ResourceStates.UnorderedAccess,
    ResourceState.DepthWrite => ResourceStates.DepthWrite,
    ResourceState.DepthRead => ResourceStates.DepthRead,
    ResourceState.ShaderResource => ResourceStates.PixelShaderResource | ResourceStates.NonPixelShaderResource,
    ResourceState.StreamOut => ResourceStates.StreamOut,
    ResourceState.IndirectArgument => ResourceStates.IndirectArgument,
    ResourceState.CopyDest => ResourceStates.CopyDest,
    ResourceState.CopySource => ResourceStates.CopySource,
    ResourceState.ResolveDest => ResourceStates.ResolveDest,
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

  public static ComparisonFunc ConvertComparisonFunc(ComparisonFunction _func) => _func switch
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

  public static BlendOp ConvertBlendOp(BlendOperation _operation) => _operation switch
  {
    BlendOperation.Add => BlendOp.Add,
    BlendOperation.Subtract => BlendOp.Subtract,
    BlendOperation.ReverseSubtract => BlendOp.RevSubtract,
    BlendOperation.Min => BlendOp.Min,
    BlendOperation.Max => BlendOp.Max,
    _ => BlendOp.Add
  };

  public static Silk.NET.Direct3D12.CullMode ConvertCullMode(GraphicsAPI.Enums.CullMode _mode) => _mode switch
  {
    GraphicsAPI.Enums.CullMode.None => Silk.NET.Direct3D12.CullMode.None,
    GraphicsAPI.Enums.CullMode.Front => Silk.NET.Direct3D12.CullMode.Front,
    GraphicsAPI.Enums.CullMode.Back => Silk.NET.Direct3D12.CullMode.Back,
    _ => Silk.NET.Direct3D12.CullMode.Back
  };

  public static StencilOp ConvertStencilOperation(StencilOperation _operation) => _operation switch
  {
    StencilOperation.Keep => StencilOp.Keep,
    StencilOperation.Zero => StencilOp.Zero,
    StencilOperation.Replace => StencilOp.Replace,
    StencilOperation.IncrementSaturated => StencilOp.IncrSat,
    StencilOperation.DecrementSaturated => StencilOp.DecrSat,
    StencilOperation.Invert => StencilOp.Invert,
    StencilOperation.Increment => StencilOp.Incr,
    StencilOperation.Decrement => StencilOp.Decr,
    _ => StencilOp.Keep
  };

  public static Blend ConvertBlend(BlendFactor _option) => _option switch
  {
    BlendFactor.Zero => Blend.Zero,
    BlendFactor.One => Blend.One,
    BlendFactor.SrcColor => Blend.SrcColor,
    BlendFactor.InvSrcColor => Blend.InvSrcColor,
    BlendFactor.SrcAlpha => Blend.SrcAlpha,
    BlendFactor.InvSrcAlpha => Blend.InvSrcAlpha,
    BlendFactor.DstAlpha => Blend.DestAlpha,
    BlendFactor.InvDstAlpha => Blend.InvDestAlpha,
    BlendFactor.DstColor => Blend.DestColor,
    BlendFactor.InvDstColor => Blend.InvDestColor,
    BlendFactor.SrcAlphaSat => Blend.SrcAlphaSat,
    BlendFactor.BlendFactor => Blend.BlendFactor,
    BlendFactor.InvBlendFactor => Blend.InvBlendFactor,
    _ => Blend.One
  };

  public static PrimitiveTopologyType ConvertPrimitiveTopologyType(PrimitiveTopology _topology) => _topology switch
  {
    PrimitiveTopology.PointList => PrimitiveTopologyType.Point,
    PrimitiveTopology.LineList or PrimitiveTopology.LineStrip => PrimitiveTopologyType.Line,
    PrimitiveTopology.TriangleList or PrimitiveTopology.TriangleStrip => PrimitiveTopologyType.Triangle,
    _ => PrimitiveTopologyType.Undefined
  };

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

  public static unsafe InputElementDesc[] CreateInputLayout(InputLayoutDescription _layoutDesc)
  {
    if(_layoutDesc?.Elements == null || _layoutDesc.Elements.Count == 0)
      return Array.Empty<InputElementDesc>();

    var elements = new InputElementDesc[_layoutDesc.Elements.Count];

    for(int i = 0; i < _layoutDesc.Elements.Count; i++)
    {
      var element = _layoutDesc.Elements[i];
      elements[i] = new InputElementDesc
      {
        SemanticName = (byte*)SilkMarshal.StringToPtr(element.SemanticName),
        SemanticIndex = element.SemanticIndex,
        Format = DX12Helpers.ConvertFormat(element.Format),
        InputSlot = element.InputSlot,
        AlignedByteOffset = element.AlignedByteOffset == uint.MaxValue
              ? D3D12.AppendAlignedElement
              : element.AlignedByteOffset,
        InputSlotClass = element.InputSlotClass == GraphicsAPI.Enums.InputClassification.PerVertexData
              ? Silk.NET.Direct3D12.InputClassification.PerVertexData
              : Silk.NET.Direct3D12.InputClassification.PerInstanceData,
        InstanceDataStepRate = element.InstanceDataStepRate
      };
    }

    return elements;
  }

  public static RasterizerDesc ConvertRasterizerState(RasterizerStateDescription _desc)
  {
    if(_desc == null)
      _desc = new RasterizerStateDescription();

    return new RasterizerDesc
    {
      FillMode = _desc.FillMode == GraphicsAPI.Enums.FillMode.Solid ? Silk.NET.Direct3D12.FillMode.Solid : Silk.NET.Direct3D12.FillMode.Wireframe,
      CullMode = DX12Helpers.ConvertCullMode(_desc.CullMode),
      FrontCounterClockwise = _desc.FrontCounterClockwise,
      DepthBias = _desc.DepthBias,
      DepthBiasClamp = _desc.DepthBiasClamp,
      SlopeScaledDepthBias = _desc.SlopeScaledDepthBias,
      DepthClipEnable = _desc.DepthClipEnable,
      MultisampleEnable = _desc.MultisampleEnable,
      AntialiasedLineEnable = _desc.AntialiasedLineEnable,
      ForcedSampleCount = 0,
      ConservativeRaster = ConservativeRasterizationMode.Off
    };
  }

  public static DepthStencilDesc ConvertDepthStencilState(DepthStencilStateDescription _desc)
  {
    if(_desc == null)
      _desc = new DepthStencilStateDescription();

    return new DepthStencilDesc
    {
      DepthEnable = _desc.DepthEnable,
      DepthWriteMask = _desc.DepthWriteEnable
            ? DepthWriteMask.All
            : DepthWriteMask.Zero,
      DepthFunc = ConvertComparisonFunc(_desc.DepthFunction),
      StencilEnable = _desc.StencilEnable,
      StencilReadMask = _desc.StencilReadMask,
      StencilWriteMask = _desc.StencilWriteMask,
      FrontFace = ConvertStencilOp(_desc.FrontFace),
      BackFace = ConvertStencilOp(_desc.BackFace)
    };
  }

  public static DepthStencilopDesc ConvertStencilOp(StencilOpDescription _desc)
  {
    return new DepthStencilopDesc
    {
      StencilFailOp = ConvertStencilOperation(_desc.StencilFailOp),
      StencilDepthFailOp = ConvertStencilOperation(_desc.StencilDepthFailOp),
      StencilPassOp = ConvertStencilOperation(_desc.StencilPassOp),
      StencilFunc = ConvertComparisonFunc(_desc.StencilFunction)
    };
  }

  public static BlendDesc ConvertBlendState(BlendStateDescription _desc)
  {
    if(_desc == null)
      _desc = new BlendStateDescription();

    var blendDesc = new BlendDesc
    {
      AlphaToCoverageEnable = _desc.AlphaToCoverageEnable,
      IndependentBlendEnable = _desc.IndependentBlendEnable
    };

    for(int i = 0; i < 8; i++)
    {
      if(i < _desc.RenderTargets.Length)
      {
        var rt = _desc.RenderTargets[i];
        blendDesc.RenderTarget[i] = new RenderTargetBlendDesc
        {
          BlendEnable = rt.BlendEnable,
          LogicOpEnable = false,
          SrcBlend = ConvertBlend(rt.SrcBlend),
          DestBlend = ConvertBlend(rt.DstBlend),
          BlendOp = ConvertBlendOp(rt.BlendOp),
          SrcBlendAlpha = ConvertBlend(rt.SrcBlendAlpha),
          DestBlendAlpha = ConvertBlend(rt.DstBlendAlpha),
          BlendOpAlpha = ConvertBlendOp(rt.BlendOpAlpha),
          LogicOp = LogicOp.Noop,
          RenderTargetWriteMask = (byte)rt.WriteMask
        };
      }
      else
      {
        blendDesc.RenderTarget[i] = new RenderTargetBlendDesc
        {
          BlendEnable = false,
          LogicOpEnable = false,
          SrcBlend = Blend.One,
          DestBlend = Blend.Zero,
          BlendOp = BlendOp.Add,
          SrcBlendAlpha = Blend.One,
          DestBlendAlpha = Blend.Zero,
          BlendOpAlpha = BlendOp.Add,
          LogicOp = LogicOp.Noop,
          RenderTargetWriteMask = (byte)ColorWriteMask.All
        };
      }
    }

    return blendDesc;
  }

  public static void ThrowIfFailed(HResult _hr, string _message)
  {
    if(_hr.IsFailure)
    {
      throw new Exception($"{_message} (HRESULT: 0x{_hr.Value:X8})");
    }
  }

  internal static Format ConvertIndexFormat(IndexFormat _format)
  {
    throw new NotImplementedException();
  }

  internal static unsafe void SetResourceName(ID3D12Resource* _resource, string _name)
  {
    if(_resource != null && !string.IsNullOrEmpty(_name))
    {
      var namePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalUni(_name);
      try
      {
        _resource->SetName((char*)namePtr);
      }
      finally
      {
        System.Runtime.InteropServices.Marshal.FreeHGlobal(namePtr);
      }
    }
  }
}
