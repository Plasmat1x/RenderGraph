namespace GraphicsAPI.Enums;

[Flags]
public enum ColorWriteMask
{
  Red = 1,
  Green = 2,
  Blue = 4,
  Alpha = 8,
  All = Red | Green | Blue | Alpha
}