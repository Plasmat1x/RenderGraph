using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI;

public struct CommandBufferStats
{
  public int TotalCommands;
  public int DrawCalls;
  public int DispatchCalls;
  public int ResourceBindings;
  public int ResourceTransitions;

  public override readonly string ToString()
  {
    return $"Commands: {TotalCommands}, Draws: {DrawCalls}, Dispatches: {DispatchCalls}, " +
           $"Resource Bindings: {ResourceBindings}, Transitions: {ResourceTransitions}";
  }
}