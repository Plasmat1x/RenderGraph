using System.Text;
using System.Xml.Linq;

/// <summary>
/// Точка входа в приложение
/// </summary>
public static class Program
{
  public static void Main(string[] args)
  {
    Console.OutputEncoding = Encoding.UTF8;
    Console.WriteLine("🎨 Starting Simple RenderGraph + DirectX12 Example...");

    try
    {
      using var example = new SimpleRenderGraphExample();
      example.Run();
    }
    catch(Exception ex)
    {
      Console.WriteLine($"❌ Error: {ex.Message}");
      Console.WriteLine(ex.StackTrace);
    }

    Console.WriteLine("✅ Example completed!");
  }
}