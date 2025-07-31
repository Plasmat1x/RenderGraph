using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl.Tools;
public static class DX12ShaderComponentMapping
{
  private static int p_mask = unchecked(0x7);
  private static int p_shift = 3;

  public static int AlwaysSetBitAvoidingZeromemMistakes()
  {
    return 1 << p_shift * 4;
  }

  /// <summary>
  /// устновить кастомный мапинг
  /// </summary>
  /// <param name="_src0"></param>
  /// <param name="_src1"></param>
  /// <param name="_src2"></param>
  /// <param name="_src3"></param>
  /// <returns></returns>
  public static int EncodeShader4ComponentMapping(int _src0, int _src1, int _src2, int _src3)
  {
    return

     _src0 & p_mask |
     (_src1 & p_mask) << p_shift |
     (_src2 & p_mask) << p_shift * 2 |
     (_src3 & p_mask) << p_shift * 3 |
     AlwaysSetBitAvoidingZeromemMistakes()
    ;
  }

  public static ShaderComponentMapping DecodeShader4ComponentMapping(int _toExtract, int _mapping)
  {
    return (ShaderComponentMapping)(_mapping >> p_shift * _toExtract & p_mask);
  }

  /// <summary>
  /// стандартный маппин байт в шейдере (1-2-3-4)
  /// </summary>
  /// <returns></returns>
  public static ShaderComponentMapping Default4ComponentMapping()
  {
    return (ShaderComponentMapping)EncodeShader4ComponentMapping(0, 1, 2, 3);
  }
}