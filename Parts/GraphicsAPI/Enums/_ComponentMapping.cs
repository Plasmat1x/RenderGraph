namespace GraphicsAPI.Enums;

public struct _ComponentMapping
{
  public ComponentSwizzle R;
  public ComponentSwizzle G;
  public ComponentSwizzle B;
  public ComponentSwizzle A;

  public static readonly _ComponentMapping Identity = new _ComponentMapping
  {
    R = ComponentSwizzle.R,
    G = ComponentSwizzle.G,
    B = ComponentSwizzle.B,
    A = ComponentSwizzle.A
  };
}
