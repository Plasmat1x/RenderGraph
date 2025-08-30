using System.Text;

namespace GenericCommandBuffer;
public static class Program
{
  public static void Main(string[] _args)
  {
    Console.OutputEncoding = Encoding.UTF8;

    CommandBufferExample.RunExample();

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
  }
}
