namespace Resources.Enums;

/// <summary>
/// Использование ресурсов (как часто обновляются)
/// </summary>
public enum ResourceUsage
{
  Default,    // GPU read/write, редкие обновления с CPU
  Immutable,  // GPU read only, никогда не обновляется
  Dynamic,    // GPU read, CPU write, частые обновления
  Staging     // CPU read/write, для копирования данных
}