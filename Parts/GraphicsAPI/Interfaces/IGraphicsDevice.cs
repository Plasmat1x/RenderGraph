using GraphicsAPI.Enums;

using Resources;
using Resources.Enums;

namespace GraphicsAPI.Interfaces;

public interface IGraphicsDevice: IDisposable
{
  string Name { get; }
  GraphicsAPI.Enums.GraphicsAPI API { get; }
  DeviceCapabilities Capabilities { get; }

  ITexture CreateTexture(TextureDescription description);
  IBuffer CreateBuffer(BufferDescription description);
  IShader CreateShader(ShaderDescription description);
  IRenderState CreateRenderState(RenderStateDescription description);
  ISampler CreateSampler(SamplerDescription description);

  // Command buffers
  CommandBuffer CreateCommandBuffer();
  CommandBuffer CreateCommandBuffer(CommandBufferType type);
  void ExecuteCommandBuffer(CommandBuffer commandBuffer);
  void ExecuteCommandBuffers(CommandBuffer[] commandBuffers);

  // Синхронизация
  void WaitForGPU();
  void WaitForFence(IFence fence);
  IFence CreateFence(ulong initialValue = 0);

  // Информация о памяти
  MemoryInfo GetMemoryInfo();
  ulong GetTotalMemory();
  ulong GetAvailableMemory();

  // Swapchain (для презентации)
  ISwapChain CreateSwapChain(SwapChainDescription description);
  void Present();

  // Утилиты
  bool SupportsFormat(TextureFormat format, FormatUsage usage);
  uint GetFormatBytesPerPixel(TextureFormat format);
  SampleCountFlags GetSupportedSampleCounts(TextureFormat format);
}