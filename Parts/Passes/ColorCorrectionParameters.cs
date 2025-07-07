using System.Numerics;

namespace Passes;

public struct ColorCorrectionParameters
{
  public float Gamma;
  public float Contrast;
  public float Brightness;
  public float Saturation;
  public Vector3 ColorBalance;
  public float Exposure;
  public int EnableToneMapping;
  public int ToneMappingType;
}
