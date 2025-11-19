using GraphicsAPI;
using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using Resources;
using Resources.Enums;
using Silk.NET.Vulkan;
using Vulkan = Silk.NET.Vulkan;
using CommandBuffer = GraphicsAPI.CommandBuffer;
using SampleCountFlags = GraphicsAPI.Enums.SampleCountFlags;

namespace VulkanImpl;

public class VKGraphicsDevice: IGraphicsDevice
{
  //TODO: individual command pool per queue type by family index
  // Command pool is implementation detail 
  
  private readonly Queue<CommandBuffer> p_submissionQueue = [];
  private readonly object p_submissionLock = new object();

  private Vulkan.Vk p_vk;
  private Vulkan.Instance p_instance; 
  private Vulkan.PhysicalDevice p_physicalDevice;
  private Vulkan.Device p_device;

  private Vulkan.CommandPool p_copyCommandPool;
  private Vulkan.Queue p_copyQueue;
  
  private Vulkan.CommandPool p_directCommandPool;
  private Vulkan.Queue p_directQueue;
  
  private Vulkan.CommandPool p_computeCommandPool;
  private Vulkan.Queue p_computeQueue;
  
  public string Name { get; }
  public API API { get; }
  public DeviceCapabilities Capabilities { get; }

  public VKGraphicsDevice()
  {

  }
  
  public ITexture CreateTexture(TextureDescription _description)
  {
    throw new NotImplementedException();
  }

  public IBuffer CreateBuffer(BufferDescription _description)
  {
    throw new NotImplementedException();
  }

  public IShader CreateShader(ShaderDescription _description)
  {
    throw new NotImplementedException();
  }

  public IRenderState CreateRenderState(RenderStateDescription _description)
  {
    throw new NotImplementedException();
  }

  public IRenderState CreateRenderState(RenderStateDescription _renderStateDesc, PipelineStateDescription _pipelineStateDesc)
  {
    throw new NotImplementedException();
  }

  public ISampler CreateSampler(SamplerDescription _description)
  {
    throw new NotImplementedException();
  }

  public IFence CreateFence(ulong _initialValue = 0)
  {
    throw new NotImplementedException();
  }

  public ISwapChain CreateSwapChain(SwapChainDescription _description, IntPtr _windowHandle)
  {
    throw new NotImplementedException();
  }

  public CommandBuffer CreateCommandBuffer()
  {
    throw new NotImplementedException();
  }

  public CommandBuffer CreateCommandBuffer(CommandBufferType _type, CommandBufferExecutionMode _mode)
  {
    throw new NotImplementedException();
  }

  public void Submit(CommandBuffer _commandBuffer)
  {
    throw new NotImplementedException();
  }

  public void Submit(CommandBuffer[] _commandBuffers)
  {
    throw new NotImplementedException();
  }

  public void Submit(CommandBuffer _commandBuffer, IFence _fence, ulong _fenceValue)
  {
    throw new NotImplementedException();
  }

  public Task SubmitAsync(CommandBuffer _commandBuffer)
  {
    throw new NotImplementedException();
  }

  public void WaitForGPU()
  {
    throw new NotImplementedException();
  }

  public void WaitForFence(IFence _fence)
  {
    throw new NotImplementedException();
  }

  public void WaitForFenceValue(IFence _fence, ulong _value)
  {
    throw new NotImplementedException();
  }

  public void Present()
  {
    throw new NotImplementedException();
  }

  public void Present(ISwapChain _swapChain)
  {
    throw new NotImplementedException();
  }

  public MemoryInfo GetMemoryInfo()
  {
    throw new NotImplementedException();
  }

  public ulong GetTotalMemory()
  {
    throw new NotImplementedException();
  }

  public ulong GetAvailableMemory()
  {
    throw new NotImplementedException();
  }

  public bool SupportsFormat(TextureFormat _format, FormatUsage _usage)
  {
    throw new NotImplementedException();
  }

  public uint GetFormatBytesPerPixel(TextureFormat _format)
  {
    throw new NotImplementedException();
  }

  public SampleCountFlags GetSupportedSampleCounts(TextureFormat _format)
  {
    throw new NotImplementedException();
  }

  public void SetDebugName(IResource _resource, string _name)
  {
    throw new NotImplementedException();
  }

  public void BeginEvent(string _name)
  {
    throw new NotImplementedException();
  }

  public void EndEvent()
  {
    throw new NotImplementedException();
  }

  public void SetMarker(string _name)
  {
    throw new NotImplementedException();
  }
  
  public void Dispose()
  {
    throw new NotImplementedException();
  }
}
