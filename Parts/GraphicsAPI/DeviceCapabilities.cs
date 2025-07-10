using GraphicsAPI.Enums;

namespace GraphicsAPI;

public struct DeviceCapabilities
{
  public uint MaxTexture1DSize { get; set; }
  public uint MaxTexture2DSize { get; set; }
  public uint MaxTexture3DSize { get; set; }
  public uint MaxTextureCubeSize { get; set; }
  public uint MaxTextureArrayLayers { get; set; }
  public uint MaxColorAttachments { get; set; }
  public uint MaxVertexAttributes { get; set; }
  public uint MaxVertexBuffers { get; set; }
  public uint MaxUniformBufferBindings { get; set; }
  public uint MaxStorageBufferBindings { get; set; }
  public uint MaxSampledImageBindings { get; set; }
  public uint MaxStorageImageBindings { get; set; }
  public uint MaxSamplerBindings { get; set; }
  public uint MaxComputeWorkGroupSize { get; set; }
  public uint MaxComputeWorkGroupInvocations { get; set; }
  public SampleCountFlags SupportedSampleCounts { get; set; }
  public bool SupportsGeometryShader { get; set; }
  public bool SupportsTessellation { get; set; }
  public bool SupportsComputeShader { get; set; }
  public bool SupportsMultiDrawIndirect { get; set; }
  public bool SupportsDrawIndirect { get; set; }
  public bool SupportsDepthClamp { get; set; }
  public bool SupportsAnisotropicFiltering { get; set; }
  public bool SupportsTextureCompressionBC { get; set; }
  public bool SupportsTextureCompressionETC { get; set; }
  public bool SupportsTextureCompressionASTC { get; set; }
}