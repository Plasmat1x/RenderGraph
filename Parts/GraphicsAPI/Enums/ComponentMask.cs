namespace GraphicsAPI.Enums;

[Flags]
public enum ComponentMask
{
  X = 1,
  Y = 2,
  Z = 4,
  W = 8,
  All = X | Y | Z | W
}