using GraphicsAPI.Enums;

using Silk.NET.Maths;
using Silk.NET.Windowing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DX12RenderingExample;

public static class Program
{
  public static void Main(string[] args)
  {
    var opts = new WindowOptions
    {
      API = Silk.NET.Windowing.GraphicsAPI.Default,
      Size = new Vector2D<int>(800, 600),
      Position = new Vector2D<int>(200, 200),
      IsVisible = true,
      TopMost = false,
      WindowBorder = WindowBorder.Resizable,
      WindowState = WindowState.Normal,
      Title = "Hello"
    };

    var window = Window.Create(opts);

    var example = new DX12RenderingExample.RenderingExample();

    window.Load += () => example.Initialize(window.Native.DXHandle.GetValueOrDefault(), 800, 600);
    window.Render += (dt) => example.Render();
    window.Closing += () => example.Cleanup();
    window.Resize += (sz) => { };
    window.Update += (dt) => { };

    window.Run();
  }
}
