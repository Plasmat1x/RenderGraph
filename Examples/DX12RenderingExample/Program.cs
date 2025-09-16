using Silk.NET.Maths;
using Silk.NET.Windowing;

using System.Text;

namespace DX12RenderingExample;

public static class Program
{
  //TODO: треугольник рисуется но у него цвета нет, поэтому не отображается.
  //необходимо проверить шейдеры и все с этм связаное.
  //Есть вероятность что матрицы используются в Col-major, что так же может влиять на отображение
  public static void Main(string[] args)
  {
    Console.OutputEncoding = Encoding.UTF8;

    Console.WriteLine("🚀 Starting DX12 Rendering Example...");

    var example = new RenderingExample();
    int windowWidth = 1920;
    int windowHeight = 1080;

    var window = Window.Create(new WindowOptions
    {
      API = Silk.NET.Windowing.GraphicsAPI.Default,
      Size = new Vector2D<int>(windowWidth, windowHeight),
      Position = new Vector2D<int>(200, 200),
      IsVisible = true,
      TopMost = false,
      WindowBorder = WindowBorder.Resizable,
      WindowState = WindowState.Normal,
      Title = "Hello"
    });


    window.Load += () => {
      var mockWindowHandle = window.Native.DXHandle.GetValueOrDefault();
      example.Initialize(mockWindowHandle, (uint)windowWidth, (uint)windowHeight);
      example.DemonstrateBatchUpload();
      example.DemonstrateReadback();
    };

    window.Closing += example.Cleanup;
    window.Render += _ => example.Render();
    window.Update += _ => {
      if(example.framesCount >= 10)
      {
        //window.Close();
      }
    };

    window.Run();

    Console.WriteLine("👋 Example completed!");
  }
}
