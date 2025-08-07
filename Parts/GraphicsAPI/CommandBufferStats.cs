using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI;

public struct CommandBufferStats
{
  public int TotalCommands { get; set; }
  public int DrawCalls { get; set; }
  public int DispatchCalls { get; set; }
  public int ResourceBindings { get; set; }
  public int ResourceTransitions { get; set; }

  public override readonly string ToString()
  {
    return $"Commands: {TotalCommands}, Draws: {DrawCalls}, Dispatches: {DispatchCalls}, " +
           $"Resource Bindings: {ResourceBindings}, Transitions: {ResourceTransitions}";
  }
}