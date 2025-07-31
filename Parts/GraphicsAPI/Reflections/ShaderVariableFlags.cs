namespace GraphicsAPI.Reflections;

public enum ShaderVariableFlags
{
  None = 0,
  UsedByShader = 1 << 0,
  IsRowMajor = 1 << 1,
  IsColumnMajor = 1 << 2,
  IsArray = 1 << 3,
  IsMatrix = 1 << 4,
  ForceUsed = 1 << 5
}
