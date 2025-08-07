namespace GraphicsAPI.Reflections.Enums;

public enum ResourceBindingFlags
{
  None = 0,
  UsedByVertexShader = 1 << 0,
  UsedByHullShader = 1 << 1,
  UsedByDomainShader = 1 << 2,
  UsedByGeometryShader = 1 << 3,
  UsedByPixelShader = 1 << 4,
  UsedByComputeShader = 1 << 5,
  ComparisonSampler = 1 << 6
}
