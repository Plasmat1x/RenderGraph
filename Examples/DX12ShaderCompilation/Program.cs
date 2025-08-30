using System.Text;

namespace DX12ShaderCompilation;

internal class Program
{
  static void Main(string[] _args)
  {
    Console.OutputEncoding = Encoding.UTF8;
    DX12ShaderCompilation.RunTests();
  }
}
