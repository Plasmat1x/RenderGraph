using GraphicsAPI;
using GraphicsAPI.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl.Parts;

/// <summary>
/// DX12-специфичная статистика
/// </summary>
public struct DX12CommandBufferStats
{
  /// <summary>
  /// Базовая статистика из GenericCommandBuffer
  /// </summary>
  public CommandBufferStats BaseStats { get; set; }

  /// <summary>
  /// Количество переходов состояний ресурсов
  /// </summary>
  public int StateTransitions { get; set; }

  /// <summary>
  /// Количество сбросов барьеров
  /// </summary>
  public int BarrierFlushes { get; set; }

  /// <summary>
  /// Текущий режим выполнения
  /// </summary>
  public CommandBufferExecutionMode ExecutionMode { get; set; }

  /// <summary>
  /// Указатель на нативный command list (для отладки)
  /// </summary>
  public nint NativeCommandListPtr { get; set; }

  /// <summary>
  /// Количество установленных дескрипторов
  /// </summary>
  public int DescriptorsSet { get; set; }

  /// <summary>
  /// Количество смен pipeline state
  /// </summary>
  public int PipelineStateChanges { get; set; }

  public override string ToString()
  {
    return $"DX12 Stats: Mode={ExecutionMode}, Commands={BaseStats.TotalCommands}, " +
           $"StateTransitions={StateTransitions}, BarrierFlushes={BarrierFlushes}, " +
           $"PSO Changes={PipelineStateChanges}";
  }
}

