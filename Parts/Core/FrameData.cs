using System.Numerics;

namespace Core;

public class FrameData
{
  private readonly Dictionary<string, object> p_globalConstants = [];

  public ulong FrameIndex { get; set; }
  public float DeltaTime { get; set; }
  public Matrix4x4 ViewMatrix { get; set; }
  public Matrix4x4 ProjectionMatrix { get; set; }
  public Matrix4x4 ViewProjectionMatrix { get; private set; }
  public Vector3 CameraPosition  { get; set; }
  public uint ScreenWidth {  get; set; }
  public uint ScreenHeight { get; set; }


  public T GetConstantBuffer<T>()where T : struct
  {
    var key = typeof(T).Name;
    if(p_globalConstants.TryGetValue(key, out var value) && value is T typedValue)
      return typedValue;

    return default;
  } 

  public void SetConstantBuffer<T>(T _data) where T : struct
  {
    var key = typeof(T).Name;
    p_globalConstants[key] = _data;
  }

  public void SetGlobalConstants(string _name, object _value)
  {
    p_globalConstants[_name] = _value;
  }

  public T GetGlobalConstant<T>(string _name)
  {
    if(p_globalConstants.TryGetValue(_name, out var value) && value is T typedValue)
      return typedValue;

    return default;
  }

  public void UpdateMatrices()
  {
    ViewProjectionMatrix = Matrix4x4.Multiply(ProjectionMatrix, ProjectionMatrix);
  }

  public void Reset()
  {
    FrameIndex = 0;
    DeltaTime = 0f;
    ViewMatrix = Matrix4x4.Identity;
    ProjectionMatrix = Matrix4x4.Identity;
    CameraPosition = Vector3.Zero;
    ScreenHeight = 0;
    ScreenWidth = 0;
    p_globalConstants.Clear();
  }
}


