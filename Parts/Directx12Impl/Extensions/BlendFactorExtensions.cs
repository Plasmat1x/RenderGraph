using GraphicsAPI.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Extensions;

public static class BlendFactorExtensions
{
  public static Blend Convert(this BlendFactor _option) => _option switch
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
}
