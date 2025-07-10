namespace GraphicsAPI.Enums;

public struct ComponentMapping
{
  public ComponentSwizzle R;
  public ComponentSwizzle G;
  public ComponentSwizzle B;
  public ComponentSwizzle A;

  public static readonly ComponentMapping Identity = new ComponentMapping
  {
    R = ComponentSwizzle.R,
    G = ComponentSwizzle.G,
    B = ComponentSwizzle.B,
    A = ComponentSwizzle.A
  };
}