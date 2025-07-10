using System.Numerics;

namespace Passes;

public struct BlurParameters
{
  public float BlurRadius { get; set; }
  public float BlurSigma { get; set; }
  public Vector2 TexelSize { get; set; }
  public int KernelSize { get; set; }
}
