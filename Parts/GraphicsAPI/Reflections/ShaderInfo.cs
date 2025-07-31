using GraphicsAPI.Enums;

namespace GraphicsAPI.Reflections;

public class ShaderInfo
{
  public string Creator { get; set; }
  public uint Version { get; set; }
  public uint Flags { get; set; }
  public uint InstructionCount { get; set; }
  public string ShaderModel { get; set; }
  public ShaderStage Stage { get; set; }
}
