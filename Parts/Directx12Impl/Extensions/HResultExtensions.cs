using GraphicsAPI.Descriptions;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl.Extensions;
public static class HResultExtensions
{
  public static Exception GetException(this HResult _hr)
  {
    return Marshal.GetExceptionForHR(_hr);
  }
}


public static class InputLayoutDecsriptionExtensions
{
  public static unsafe InputElementDesc[] Convert(this InputLayoutDescription _layoutDesc)
  {
    if(_layoutDesc?.Elements == null || _layoutDesc.Elements.Count == 0)
      return Array.Empty<InputElementDesc>();

    var elements = new InputElementDesc[_layoutDesc.Elements.Count];

    for(var i = 0; i < _layoutDesc.Elements.Count; i++)
    {
      var element = _layoutDesc.Elements[i];
      elements[i] = new InputElementDesc
      {
        SemanticName = (byte*)SilkMarshal.StringToPtr(element.SemanticName),
        SemanticIndex = element.SemanticIndex,
        Format = element.Format.Convert(),
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
}