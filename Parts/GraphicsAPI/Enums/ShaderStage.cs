namespace GraphicsAPI.Enums;

public enum ShaderStage
{
  Unknown,
  Vertex,
  Hull,
  Domain,
  Geometry,
  Pixel,
  Compute,
  Amplification = 7,  // DX12 Mesh Shaders
  Mesh = 8,           // DX12 Mesh Shaders
  RayGeneration = 9,  // DXR
  Intersection = 10,  // DXR
  AnyHit = 11,       // DXR
  ClosestHit = 12,   // DXR
  Miss = 13,         // DXR
  Callable = 14
}
