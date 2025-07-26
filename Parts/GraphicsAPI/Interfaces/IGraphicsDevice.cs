using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Resources;
using Resources.Enums;

namespace GraphicsAPI.Interfaces;

public interface IGraphicsDevice: IDisposable
{
  string Name { get; }
  API API { get; }
  DeviceCapabilities Capabilities { get; }

  ITexture CreateTexture(TextureDescription _description);
  IBuffer CreateBuffer(BufferDescription _description);
  IShader CreateShader(ShaderDescription _description);
  IRenderState CreateRenderState(RenderStateDescription _description);
  ISampler CreateSampler(SamplerDescription _description);
  CommandBuffer CreateCommandBuffer();
  CommandBuffer CreateCommandBuffer(CommandBufferType _type);
  void ExecuteCommandBuffer(CommandBuffer _commandBuffer);
  void ExecuteCommandBuffers(CommandBuffer[] _commandBuffers);
  void WaitForGPU();
  void WaitForFence(IFence _fence);
  IFence CreateFence(ulong _initialValue = 0);
  MemoryInfo GetMemoryInfo();
  ulong GetTotalMemory();
  ulong GetAvailableMemory();
  ISwapChain CreateSwapChain(SwapChainDescription _description, IntPtr _windowHandle);
  void Present();
  bool SupportsFormat(TextureFormat _format, FormatUsage _usage);
  uint GetFormatBytesPerPixel(TextureFormat _format);
  SampleCountFlags GetSupportedSampleCounts(TextureFormat _format);
}