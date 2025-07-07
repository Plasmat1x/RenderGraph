using GraphicsAPI.Enums;

namespace GraphicsAPI;

public struct DeviceCapabilities
{
  public uint MaxTexture1DSize;
  public uint MaxTexture2DSize;
  public uint MaxTexture3DSize;
  public uint MaxTextureCubeSize;
  public uint MaxTextureArrayLayers;
  public uint MaxColorAttachments;
  public uint MaxVertexAttributes;
  public uint MaxVertexBuffers;
  public uint MaxUniformBufferBindings;
  public uint MaxStorageBufferBindings;
  public uint MaxSampledImageBindings;
  public uint MaxStorageImageBindings;
  public uint MaxSamplerBindings;
  public uint MaxComputeWorkGroupSize;
  public uint MaxComputeWorkGroupInvocations;
  public SampleCountFlags SupportedSampleCounts;
  public bool SupportsGeometryShader;
  public bool SupportsTessellation;
  public bool SupportsComputeShader;
  public bool SupportsMultiDrawIndirect;
  public bool SupportsDrawIndirect;
  public bool SupportsDepthClamp;
  public bool SupportsAnisotropicFiltering;
  public bool SupportsTextureCompressionBC;
  public bool SupportsTextureCompressionETC;
  public bool SupportsTextureCompressionASTC;
}