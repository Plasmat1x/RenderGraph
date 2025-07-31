using System;
using System.Text;

namespace Examples.ShaderReflectionDemo;

public static class Programm
{
  public static void Main(string[] args)
  {
    Console.OutputEncoding = Encoding.UTF8;
    Console.WriteLine("=== Shader Reflection Demo BEGIN ===\n");

    ShaderReflectionDemo.Run();

    Console.WriteLine("=== Shader Reflection Demo END ===\n");
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
  }
}
