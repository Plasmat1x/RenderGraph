using GraphicsAPI.Enums;

using Resources.Enums;

namespace GraphicsAPI.Descriptions;

public class SwapChainDescription
{
  public uint Width { get; set; }
  public uint Height { get; set; }
  public TextureFormat Format { get; set; } = TextureFormat.R8G8B8A8_UNORM;
  public uint BufferCount { get; set; } = 2;
  //public uint SampleCount { get; set; } = 1;
  //public uint SampleQuality { get; set; } = 0;
  public SwapEffect SwapEffect { get; set; } = SwapEffect.FlipDiscard;
  public SwapChainFlags Flags { get; set; } = 0;
  public uint RefreshRate { get; set; } = 0;
  public ScalingMode Scaling { get; set; } = ScalingMode.None;
  public AlphaMode AlphaMode { get; set; } = AlphaMode.Ignore;

}