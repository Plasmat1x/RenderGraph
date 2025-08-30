using System.Text;

namespace Examples.ShaderReflectionDemo;

public static class Programm
{
  public static void Main(string[] _args)
  {
    Console.OutputEncoding = Encoding.UTF8;

    ShaderReflectionDemo.Run();

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
  }
}
