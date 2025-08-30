namespace GraphicsAPI.Enums;

/// <summary>
/// Режим выполнения команд в DX12CommandBuffer
/// </summary>
public enum CommandBufferExecutionMode
{
  /// <summary>
  /// Немедленное выполнение с минимальными проверками (максимальная производительность)
  /// </summary>
  Immediate,

  /// <summary>
  /// Отложенное выполнение с возможностью оптимизации и группировки команд
  /// </summary>
  Deferred,

  /// <summary>
  /// Общее выполнение через базовые методы (максимальная совместимость)
  /// </summary>
  Generic
}