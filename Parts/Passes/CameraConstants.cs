using System.Numerics;

namespace Passes;
internal class CameraConstants
{
  public Matrix4x4 ViewMatrix { get; set; }
  public Matrix4x4 ProjectionMatrix { get; set; }
  public Matrix4x4 ViewProjectionMatrix { get; set; }
  public Vector4 CameraPosition { get; set; }
  public Vector4 ScreenResolution { get; set; }
}