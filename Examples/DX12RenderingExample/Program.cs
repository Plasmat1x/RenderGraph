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
    Console.WriteLine("🚀 Starting DX12 Rendering Example...");

    var example = new RenderingExample();

    try
    {
      // Для демонстрации используем заглушку окна
      var mockWindowHandle = IntPtr.Zero; // В реальной реализации здесь был бы HWND

      example.Initialize(mockWindowHandle, 1920, 1080);

      // Демонстрируем возможности
      example.DemonstrateBatchUpload();
      example.DemonstrateReadback();

      // В реальном приложении здесь был бы render loop
      Console.WriteLine("\n🎮 Render loop would start here...");
      Console.WriteLine("Press any key to cleanup and exit...");
      Console.ReadKey();

    }
    catch(Exception ex)
    {
      Console.WriteLine($"❌ Error: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
    finally
    {
      example.Cleanup();
    }

    Console.WriteLine("👋 Example completed!");
  }
}
