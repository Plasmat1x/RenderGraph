using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class BlendStateDescriptionExtensions
{
  public static BlendDesc Convert(this BlendStateDescription _desc)
  {
    if(_desc == null)
      _desc = new BlendStateDescription();

    var blendDesc = new BlendDesc
    {
      AlphaToCoverageEnable = _desc.AlphaToCoverageEnable,
      IndependentBlendEnable = _desc.IndependentBlendEnable
    };

    for(var i = 0; i < 8; i++)
    {
      if(i < _desc.RenderTargets.Length)
      {
        var rt = _desc.RenderTargets[i];
        blendDesc.RenderTarget[i] = new RenderTargetBlendDesc
        {
          BlendEnable = rt.BlendEnable,
          LogicOpEnable = false,
          SrcBlend = rt.SrcBlend.Convert(),
          DestBlend = rt.DstBlend.Convert(),
          BlendOp = rt.BlendOp.Convert(),
          SrcBlendAlpha = rt.SrcBlendAlpha.Convert(),
          DestBlendAlpha = rt.DstBlendAlpha.Convert(),
          BlendOpAlpha = rt.BlendOpAlpha.Convert(),
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
}
