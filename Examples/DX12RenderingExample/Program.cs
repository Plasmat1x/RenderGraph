using Silk.NET.Maths;
using Silk.NET.Windowing;

using System.Text;

namespace DX12RenderingExample;

public static class Program
{
  public static void Main(string[] args)
  {
    Console.OutputEncoding = Encoding.UTF8;

    Console.WriteLine("🚀 Starting DX12 Rendering Example...");

    var example = new RenderingExample();

    var window = Window.Create(new WindowOptions
    {
      API = Silk.NET.Windowing.GraphicsAPI.Default,
      Size = new Vector2D<int>(800, 600),
      Position = new Vector2D<int>(200, 200),
      IsVisible = true,
      TopMost = false,
      WindowBorder = WindowBorder.Resizable,
      WindowState = WindowState.Normal,
      Title = "Hello"
    });


    window.Load += () => {
      var mockWindowHandle = window.Native.DXHandle.GetValueOrDefault();
      example.Initialize(mockWindowHandle, 1920, 1080);
      example.DemonstrateBatchUpload();
      example.DemonstrateReadback();
    };

    window.Closing += example.Cleanup;
    window.Render += _ => example.Render();

    window.Run();

    //try
    //{
    //  var window = Window.Create(new WindowOptions
    //  {
    //    API = Silk.NET.Windowing.GraphicsAPI.Default,
    //    Size = new Vector2D<int>(800, 600),
    //    Position = new Vector2D<int>(200, 200),
    //    IsVisible = true,
    //    TopMost = false,
    //    WindowBorder = WindowBorder.Resizable,
    //    WindowState = WindowState.Normal,
    //    Title = "Hello"
    //  });


    //  window.Load += () => {
    //    var mockWindowHandle = window.Native.DXHandle.GetValueOrDefault();
    //    example.Initialize(mockWindowHandle, 1920, 1080);
    //    example.DemonstrateBatchUpload();
    //    example.DemonstrateReadback();
    //  };

    //  window.Run();


    //  Console.WriteLine("\n🎮 Render loop would start here...");
    //  Console.WriteLine("Press any key to cleanup and exit...");
    //  Console.ReadKey();

    //}
    //catch(Exception ex)
    //{
    //  Console.WriteLine($"❌ Error: {ex.Message}");
    //  Console.WriteLine($"Stack trace: {ex.StackTrace}");
    //}
    //finally
    //{
    //  example.Cleanup();
    //}

    Console.WriteLine("👋 Example completed!");
  }
}
