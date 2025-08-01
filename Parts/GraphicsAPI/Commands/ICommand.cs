using GraphicsAPI.Commands.enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI.Commands;

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
