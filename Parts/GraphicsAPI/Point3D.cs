namespace GraphicsAPI;

public struct Point3D
{
  public uint X { get; init; }
  public uint Y { get; init; }
  public uint Z { get; init; }

  public Point3D(uint _x, uint _y, uint _z)
  {
    X = _x;
    Y = _y;
    Z = _z;
  }
}