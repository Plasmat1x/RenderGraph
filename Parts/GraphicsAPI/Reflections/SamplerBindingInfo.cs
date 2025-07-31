namespace GraphicsAPI.Reflections;

public class SamplerBindingInfo
{
  public string Name { get; set; }
  public uint BindPoint { get; set; }
  public uint BindCount { get; set; } = 1;
  public uint Space { get; set; } = 0;
  public SamplerBindingFlags Flags { get; set; }
}
