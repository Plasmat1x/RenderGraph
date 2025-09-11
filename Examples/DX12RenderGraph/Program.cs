#region Program Entry Point

using System.Text;

/// <summary>
/// Точка входа в программу
/// </summary>
public class Program
{
  public static void Main(string[] args)
  {
    Console.OutputEncoding = Encoding.UTF8;

    Console.WriteLine("=== RenderGraph + DirectX12 Example ===\n");

    try
    {
      using var example = new RenderGraphDX12Example();
      example.Run();
    }
    catch(Exception ex)
    {
      Console.WriteLine($"❌ Fatal error: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }

    Console.WriteLine("\n=== Application Finished ===");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
  }
}

#endregion