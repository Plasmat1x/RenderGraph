using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Resources;
using Resources.Enums;

using System.Collections.Generic;

namespace GraphicsAPI.Interfaces;

public interface IGraphicsDevice: IDisposable
{
  string Name { get; }
  API API { get; }
  DeviceCapabilities Capabilities { get; }
  
  /// <summary>
  /// Создание экземпляра текстуры по декларативному описанию
  /// </summary>
  /// <param name="_description">Декларативное описание</param>
  /// <returns>Экземпляр Экземпляр интерфейсаса текстуры</returns>
  ITexture CreateTexture(TextureDescription _description);
  
  /// <summary>
  /// Создание экземляра буффера по декларативному описанию
  /// </summary>
  /// <param name="_description">Декларативнон описание</param>
  /// <returns>Экземпляр Экземпляр интерфейсаса буффера</returns>
  IBuffer CreateBuffer(BufferDescription _description);
  
  /// <summary>
  /// Создание экземпяра шейдера по декларативному описанию
  /// </summary>
  /// <param name="_description">Декларативное описание</param>
  /// <returns>Экземпляр интерфейса шейдера</returns>
  IShader CreateShader(ShaderDescription _description);
  
  /// <summary>
  /// Создание экземпяра состояния рендеринга по декларативному описанию
  /// </summary>
  /// <param name="_description">декларативное описание</param>
  /// <returns>Экземпляр интерфейса состояние рендеринга</returns>
  IRenderState CreateRenderState(RenderStateDescription _description);
  
  /// <summary>
  /// Создание экземпяра состояния рендеринга по декларативному описанию
  /// </summary>
  /// <param name="_renderStateDesc">Декларативное описание</param>
  /// <param name="_pipelineStateDesc">Декларативное описание</param>
  /// <returns>Экземпляр интерфейса состояние рендеринга</returns>
  IRenderState CreateRenderState(RenderStateDescription _renderStateDesc, PipelineStateDescription _pipelineStateDesc);
  
  /// <summary>
  /// Создание экземпляра сэмплера по декларативному описанию
  /// </summary>
  /// <param name="_description">Декларативное описание</param>
  /// <returns>Экземпляр интерфейсас сэмплера</returns>
  ISampler CreateSampler(SamplerDescription _description);
  
  /// <summary>
  /// Создание фенса
  /// </summary>
  /// <param name="_initialValue">начальное значение фенса</param>
  /// <returns>Экземпляр интерфейса фенса</returns>
  IFence CreateFence(ulong _initialValue = 0);
  
  /// <summary>
  /// Создание свопчейна по декларативному описанию и хендлу окна
  /// </summary>
  /// <param name="_description">Декларативное описание</param>
  /// <param name="_windowHandle">хендл окна</param>
  /// <returns>Экземпляр интерфейсас свопчейна</returns>
  ISwapChain CreateSwapChain(SwapChainDescription _description, IntPtr _windowHandle);
  
  /// <summary>
  /// Создание коммандного буффера по умолчнию Direct и Immediate
  /// </summary>
  /// <returns>Экземпляр абстрактного класса коммандного буффера</returns>
  CommandBuffer CreateCommandBuffer();
  
  /// <summary>
  /// Создание коммандного буффера
  /// </summary>
  /// <param name="_type">Тип коммандного буффера</param>
  /// <param name="_mode">Режим исполнения коммандного буффера</param>
  /// <returns></returns>
  CommandBuffer CreateCommandBuffer(CommandBufferType _type, CommandBufferExecutionMode _mode);
  
  /// <summary>
  /// Выполнить командный буфер
  /// </summary>
  /// <param name="_commandBuffer">Коммандный буффера</param>
  void Submit(CommandBuffer _commandBuffer);

  /// <summary>
  /// Выполнить командные буферы батчем
  /// </summary>
  /// <param name="_commandBuffer">Коммандные буфферы</param>
  void Submit(CommandBuffer[] _commandBuffers);

  /// <summary>
  /// Выполнить командный буфер с fence для синхронизации
  /// </summary>
  void Submit(CommandBuffer _commandBuffer, IFence _fence, ulong _fenceValue);

  /// <summary>
  /// Асинхронное выполнение командного буфера
  /// </summary>
  Task SubmitAsync(CommandBuffer _commandBuffer);
  
  void WaitForGPU();
  void WaitForFence(IFence _fence);
  void WaitForFenceValue(IFence _fence, ulong _value);
  
  void Present();
  void Present(ISwapChain _swapChain);
  
  MemoryInfo GetMemoryInfo();
  ulong GetTotalMemory();
  ulong GetAvailableMemory();
  
  bool SupportsFormat(TextureFormat _format, FormatUsage _usage);
  uint GetFormatBytesPerPixel(TextureFormat _format);
  SampleCountFlags GetSupportedSampleCounts(TextureFormat _format);
  
  void SetDebugName(IResource _resource, string _name);
  void BeginEvent(string _name);
  void EndEvent();
  void SetMarker(string _name);

  /// <summary>
  /// Создать состояние blending по декларативному описанию
  /// </summary>
  IBlendState CreateBlendState(BlendStateDescription _description);

  /// <summary>
  /// Создать состояние depth-stencil по декларативному описанию
  /// </summary>
  IDepthStencilState CreateDepthStencilState(DepthStencilStateDescription _description);

  /// <summary>
  /// Создать состояние rasterizer по декларативному описанию
  /// </summary>
  IRasterizerState CreateRasterizerState(RasterizerStateDescription _description);

  /// <summary>
  /// Создать query по декларативному описанию
  /// </summary>
  IQuery CreateQuery(QueryDescription _description);

  /// <summary>
  /// Выполнить командный буфер немедленно
  /// </summary>
  void ExecuteCommandBuffer(CommandBuffer _commandBuffer);

  /// <summary>
  /// Выполнить несколько командных буферов немедленно
  /// </summary>
  void ExecuteCommandBuffers(CommandBuffer[] _commandBuffers);

  /// <summary>
  /// Создать пакетный загрузчик ресурсов
  /// </summary>
  IBatchUploader CreateBatchUploader();

  /// <summary>
  /// Получить список всех доступных мониторов
  /// </summary>
  IReadOnlyList<IMonitor> GetMonitors();

  /// <summary>
  /// Получить primary монитор
  /// </summary>
  /// <returns>Primary монитор или null, если не определён</returns>
  IMonitor GetPrimaryMonitor();
}