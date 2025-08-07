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

  // === Создание ресурсов ===
  ITexture CreateTexture(TextureDescription _description);
  IBuffer CreateBuffer(BufferDescription _description);
  IShader CreateShader(ShaderDescription _description);
  IRenderState CreateRenderState(RenderStateDescription _description);
  IRenderState CreateRenderState(RenderStateDescription _renderStateDesc, PipelineStateDescription _pipelineStateDesc);
  ISampler CreateSampler(SamplerDescription _description);
  IFence CreateFence(ulong _initialValue = 0);
  ISwapChain CreateSwapChain(SwapChainDescription _description, IntPtr _windowHandle);

  // === Создание командных буферов ===
  CommandBuffer CreateCommandBuffer();
  CommandBuffer CreateCommandBuffer(CommandBufferType _type, CommandBufferExecutionMode _mode);

  // === ОБНОВЛЕННЫЕ методы выполнения для Generic архитектуры ===

  /// <summary>
  /// Выполнить командный буфер (заменяет ExecuteCommandBuffer)
  /// </summary>
  void Submit(CommandBuffer _commandBuffer);

  /// <summary>
  /// Выполнить несколько командных буферов (заменяет ExecuteCommandBuffers)
  /// </summary>
  void Submit(CommandBuffer[] _commandBuffers);

  /// <summary>
  /// Выполнить командный буфер с fence для синхронизации
  /// </summary>
  void Submit(CommandBuffer _commandBuffer, IFence _fence, ulong _fenceValue);

  /// <summary>
  /// Асинхронное выполнение командного буфера
  /// </summary>
  Task SubmitAsync(CommandBuffer _commandBuffer);
  

  //void ExecuteCommandBuffer(CommandBuffer _commandBuffer);
  //void ExecuteCommandBuffers(CommandBuffer[] _commandBuffers);

  // === Синхронизация ===
  void WaitForGPU();
  void WaitForFence(IFence _fence);
  void WaitForFenceValue(IFence _fence, ulong _value);

  // === Презентация ===
  void Present();
  void Present(ISwapChain _swapChain);

  // === Информация о памяти ===
  MemoryInfo GetMemoryInfo();
  ulong GetTotalMemory();
  ulong GetAvailableMemory();

  // === Поддержка форматов ===
  bool SupportsFormat(TextureFormat _format, FormatUsage _usage);
  uint GetFormatBytesPerPixel(TextureFormat _format);
  SampleCountFlags GetSupportedSampleCounts(TextureFormat _format);

  // === Отладка и профилирование ===
  void SetDebugName(IResource _resource, string _name);
  void BeginEvent(string _name);
  void EndEvent();
  void SetMarker(string _name);
}