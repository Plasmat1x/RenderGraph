using GraphicsAPI.Commands.enums;

namespace GraphicsAPI.Commands.Interfaces;

/// <summary>
/// Базовый интерфейс для всех команд командного буфера
/// </summary>
public interface ICommand
{
  /// <summary>
  /// Тип команды для оптимизации и отладки
  /// </summary>
  CommandType Type { get; }

  /// <summary>
  /// Размер команды в байтах (для статистики)
  /// </summary>
  int SizeInBytes { get; }
}
