using System.Numerics;

namespace Core;

public class FrameData
{
  public ulong FrameIndex;
  public float DeltaTime;
  public Matrix4x4 ViewMatrix;
  public Matrix4x4 ProjectionMatrix;
  public Matrix4x4 ViewProjectionMatrix;
  public Vector3 CameraPosition;
  public uint ScreenWidth;
  public uint ScreenHeight;
  public Dictionary<string, object> GlobalConstants = [];

  public T GetConstantBuffer<T>()
  {
    throw new NotImplementedException();
  } 

  public void SetConstantBuffer<T>(T _data)
  {
    throw new NotImplementedException();
  }

  public void UpdateMatrices()
  {
    throw new NotImplementedException();
  }
}


