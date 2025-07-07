using GraphicsAPI.Enums;

namespace GraphicsAPI;

public class ShaderDescription
{
  public string Name { get; set; } = string.Empty;
  public ShaderStage Stage { get; set; }
  public byte[] Bytecode { get; set; }
  public string EntryPoint { get; set; } = "main";
  public string SourceCode { get; set; } = string.Empty;
  public Dictionary<string, object> Defines { get; set; } = new();
}