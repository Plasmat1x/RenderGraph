using GraphicsAPI;
using GraphicsAPI.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;

/// <summary>
/// DX12-специфичная статистика
/// </summary>
public struct DX12CommandBufferStats
{
  public CommandBufferStats BaseStats;
  public int StateTransitions;
  public int BarrierFlushes;
  public CommandBufferExecutionMode ExecutionMode;
  public IntPtr NativeCommandListPtr;

  public override readonly string ToString()
  {
    return $"{BaseStats}, DX12: Transitions={StateTransitions}, Flushes={BarrierFlushes}, Mode={ExecutionMode}";
  }
}

