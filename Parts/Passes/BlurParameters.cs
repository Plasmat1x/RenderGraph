using System.Numerics;

namespace Passes;

public struct BlurParameters
{
  public float BlurRadius;
  public float BlurSigma;
  public Vector2 TexelSize;
  public int KernelSize;
}
