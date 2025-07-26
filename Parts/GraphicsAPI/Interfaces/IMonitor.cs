using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI.Interfaces;
public interface IMonitor
{
  string Name { get; }
  int Width { get; }
  int Height { get; }
  int RefreshRate { get; }
  IntPtr Handle { get; }
}
