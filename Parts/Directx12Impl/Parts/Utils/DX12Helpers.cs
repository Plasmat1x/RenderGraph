using Directx12Impl.Extensions;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System;

namespace Directx12Impl.Parts.Utils;
public static class DX12Helpers
{
  /// <summary>
  /// Константа для стандартного маппинга компонентов шейдера
  /// </summary>
  public const uint D3D12DefaultShader4ComponentMapping = 0x1688; // D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING

  public static Filter ConvertFilter(FilterMode _minFilter, FilterMode _magFilter, FilterMode _mipFilter, bool _isComparison = false)
  {
    var filter = Filter.MinMagMipPoint;

    var isMinLinear = _minFilter == FilterMode.Linear;
    var isMagLinear = _magFilter == FilterMode.Linear;
    var isMipLinear = _mipFilter == FilterMode.Linear;

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

  public static ulong AlignUp(ulong _size, ulong _alignment) => _size + _alignment - 1 & ~(_alignment - 1);
  
  public static ulong CalculateTextureSize(uint _width, uint _height, uint _depth, uint _mipLevels, uint _arraySize, Format _format)
  {
    ulong totalSize = 0;
    var bytesPerPixel = _format.GetFormatSize();
    var isCompressed = _format.IsCompressedFormat();

    for(uint arraySlice = 0; arraySlice < _arraySize; arraySlice++)
    {
      var w = _width;
      var h = _height;
      var d = _depth;

      for(uint mip = 0; mip < _mipLevels; mip++)
      {
        uint rowPitch;
        uint slicePitch;

        if(isCompressed)
        {
          var blockWidth = Math.Max(1, (w + 3) / 4);
          var blockHeight = Math.Max(1, (h + 3) / 4);
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

  public static void ThrowIfFailed(HResult _hr, string _message)
  {
    if(_hr.IsFailure)
    {
      throw new Exception($"{_message} (HRESULT: 0x{_hr.Value:X8})");
    }
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
