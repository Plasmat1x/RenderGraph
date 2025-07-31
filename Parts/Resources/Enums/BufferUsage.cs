namespace Resources.Enums;

/// <summary>
/// Расширенное использование буферов
/// </summary>
public enum BufferUsage
{
  Vertex,
  Index,
  Constant,
  Structured,
  Raw,
  IndirectArgs,
  Counter,
  Append,
  Consume,
  Staging,
  Upload,
  Readback
}