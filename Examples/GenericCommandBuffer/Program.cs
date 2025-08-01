using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCommandBuffer;
public static class Program
{
  public static void Main(string[] args)
  {
    Console.OutputEncoding = Encoding.UTF8;

    CommandBufferExample.RunExample();

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
  }
}
