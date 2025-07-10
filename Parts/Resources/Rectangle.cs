namespace Resources;
public struct Rectangle
{
  public static bool operator ==(Rectangle _left, Rectangle _right) => _left.Equals(_right);
  public static bool operator !=(Rectangle _left, Rectangle _right) => !_left.Equals(_right);
  public static readonly Rectangle Empty = new Rectangle(0, 0, 0, 0);

  public Rectangle(int _x, int _y, int _width, int _height)
  {
    X = _x;
    Y = _y;
    Width = _width;
    Height = _height;
  }

  public int X { get; set; }
  public int Y { get; set; }
  public int Width { get; set; }
  public int Height { get; set; }

  public int Left => X;
  public int Top => Y;
  public int Right => X + Width;
  public int Bottom => Y + Height;

  public bool IsEmpty => Width <= 0 || Height <= 0;
  public int Area => Width * Height;

  public bool Contains(int _x, int _y)
  {
    return _x >= X && _x < X + Width && _y >= Y && _y < Y + Height;
  }

  public bool Contains(Rectangle _rect)
  {
    return X <= _rect.X &&
           Y <= _rect.Y &&
           X + Width >= _rect.X + _rect.Width &&
           Y + Height >= _rect.Y + _rect.Height;
  }

  public bool Intersects(Rectangle _rect)
  {
    return _rect.X < X + Width &&
           X < _rect.X + _rect.Width &&
           _rect.Y < Y + Height &&
           Y < _rect.Y + _rect.Height;
  }

  public Rectangle Intersect(Rectangle _rect)
  {
    int x1 = Math.Max(X, _rect.X);
    int y1 = Math.Max(Y, _rect.Y);
    int x2 = Math.Min(X + Width, _rect.X + _rect.Width);
    int y2 = Math.Min(Y + Height, _rect.Y + _rect.Height);

    if(x2 >= x1 && y2 >= y1)
      return new Rectangle(x1, y1, x2 - x1, y2 - y1);
    else
      return new Rectangle(0, 0, 0, 0);
  }

  public Rectangle Union(Rectangle _rect)
  {
    int x1 = Math.Min(X, _rect.X);
    int y1 = Math.Min(Y, _rect.Y);
    int x2 = Math.Max(X + Width, _rect.X + _rect.Width);
    int y2 = Math.Max(Y + Height, _rect.Y + _rect.Height);

    return new Rectangle(x1, y1, x2 - x1, y2 - y1);
  }

  public override bool Equals(object? _obj)
  {
    return _obj is Rectangle rectangle &&
           X == rectangle.X &&
           Y == rectangle.Y &&
           Width == rectangle.Width &&
           Height == rectangle.Height;
  }

  public override int GetHashCode()
  {
    unchecked
    {
      int hash = 17;
      hash = hash * 23 + X;
      hash = hash * 23 + Y;
      hash = hash * 23 + Width;
      hash = hash * 23 + Height;
      return hash;
    }
  }

  public override string ToString() => $"Rectangle(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";
}
