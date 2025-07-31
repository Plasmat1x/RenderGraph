using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI.Commands.enums;

/// <summary>
/// Категории команд
/// </summary>
public enum CommandCategory
{
  State,
  Clear,
  Draw,
  Compute,
  Copy,
  Synchronization,
  Query,
  Debug,
  Other
}