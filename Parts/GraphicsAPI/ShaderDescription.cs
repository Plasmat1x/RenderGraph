using GraphicsAPI.Enums;

namespace GraphicsAPI;

public class ShaderDescription
{
  public string Name { get; set; } = string.Empty;
  public ShaderStage Stage { get; set; }
  public string EntryPoint { get; set; } = "main";
  public string ShaderModel { get; set; } = "5_1";
  public byte[] ByteCode { get; set; }
  public string FilePath {  get; set; }
  public string SourceCode { get; set; }

  public List<ShaderMacro> Defines { get; set; } = [];
  public string[] IncludePaths { get; set; }
}
