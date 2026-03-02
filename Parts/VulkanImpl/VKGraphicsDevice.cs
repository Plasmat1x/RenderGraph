using GraphicsAPI;
using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Vulkan;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VKCommandBuffer = GraphicsAPI.CommandBuffer;
using VKSampleCountFlags = GraphicsAPI.Enums.SampleCountFlags;

namespace VulkanImpl;

public unsafe class VKGraphicsDevice : IGraphicsDevice
{
  private Vk p_vk;
  private Instance p_instance;
  private PhysicalDevice p_physicalDevice;
  private Device p_device;

  private uint p_graphicsQueueFamily;
  private uint p_computeQueueFamily;
  private uint p_transferQueueFamily;
  private Queue p_graphicsQueue;
  private Queue p_computeQueue;
  private Queue p_transferQueue;

  private CommandPool p_graphicsCommandPool;
  private CommandPool p_computeCommandPool;
  private CommandPool p_transferCommandPool;

  private bool p_disposed;

  public string Name => "Vulkan Device";
  public API API => API.Vulkan;
  public DeviceCapabilities Capabilities { get; private set; }

  public VKGraphicsDevice(bool _enableDebug = false)
  {
    p_vk = Vk.GetApi();
    Capabilities = new DeviceCapabilities
    {
      MaxTexture2DSize = 4096,
      MaxTexture3DSize = 256,
      MaxTextureArrayLayers = 256,
      MaxColorAttachments = 8,
      MaxVertexAttributes = 16,
      MaxVertexBuffers = 16,
      SupportedSampleCounts = VKSampleCountFlags.Count1 | VKSampleCountFlags.Count2 | VKSampleCountFlags.Count4 | VKSampleCountFlags.Count8,
      SupportsGeometryShader = true,
      SupportsTessellation = true,
      SupportsComputeShader = true,
      SupportsMultiDrawIndirect = false,
      SupportsAnisotropicFiltering = true,
      SupportsTextureCompressionBC = true
    };
  }

  public ITexture CreateTexture(TextureDescription _description)
  {
    return new VKTexture(_description);
  }

  public IBuffer CreateBuffer(BufferDescription _description)
  {
    return new VKBuffer(_description);
  }

  public IShader CreateShader(ShaderDescription _description)
  {
    throw new NotImplementedException("VKShader needs implementation");
  }

  public IRenderState CreateRenderState(RenderStateDescription _description)
  {
    throw new NotImplementedException("VKRenderState needs implementation");
  }

  public IRenderState CreateRenderState(RenderStateDescription _renderStateDesc, PipelineStateDescription _pipelineStateDesc)
  {
    return CreateRenderState(_renderStateDesc);
  }

  public ISampler CreateSampler(SamplerDescription _description)
  {
    throw new NotImplementedException("VKSampler needs implementation");
  }

  public IFence CreateFence(ulong _initialValue = 0)
  {
    throw new NotImplementedException("VKFence needs implementation");
  }

  public ISwapChain CreateSwapChain(SwapChainDescription _description, IntPtr _windowHandle)
  {
    throw new NotImplementedException("SwapChain requires window integration");
  }

  public GraphicsAPI.CommandBuffer CreateCommandBuffer()
  {
    throw new NotImplementedException("VKCommandBuffer needs implementation");
  }

  public GraphicsAPI.CommandBuffer CreateCommandBuffer(CommandBufferType _type, CommandBufferExecutionMode _mode)
  {
    throw new NotImplementedException("VKCommandBuffer needs implementation");
  }

  public void Submit(GraphicsAPI.CommandBuffer _commandBuffer) { }
  public void Submit(GraphicsAPI.CommandBuffer[] _commandBuffers) { }
  public void Submit(GraphicsAPI.CommandBuffer _commandBuffer, IFence _fence, ulong _fenceValue) { }
  public Task SubmitAsync(GraphicsAPI.CommandBuffer _commandBuffer) => Task.CompletedTask;

  public IBlendState CreateBlendState(BlendStateDescription _description)
  {
    throw new NotImplementedException("VKBlendState needs implementation");
  }

  public IDepthStencilState CreateDepthStencilState(DepthStencilStateDescription _description)
  {
    throw new NotImplementedException("VKDepthStencilState needs implementation");
  }

  public IRasterizerState CreateRasterizerState(RasterizerStateDescription _description)
  {
    throw new NotImplementedException("VKRasterizerState needs implementation");
  }

  public IQuery CreateQuery(QueryDescription _description)
  {
    throw new NotImplementedException("VKQuery needs implementation");
  }

  public IBatchUploader CreateBatchUploader()
  {
    throw new NotImplementedException("VKBatchUploader needs implementation");
  }

  public IReadOnlyList<IMonitor> GetMonitors()
  {
    throw new NotImplementedException();
  }

  public IMonitor GetPrimaryMonitor()
  {
    throw new NotImplementedException();
  }

  public void ExecuteCommandBuffer(GraphicsAPI.CommandBuffer _commandBuffer)
  {
    throw new NotImplementedException("VKCommandBuffer execution needs implementation");
  }

  public void ExecuteCommandBuffers(GraphicsAPI.CommandBuffer[] _commandBuffers)
  {
    throw new NotImplementedException("VKCommandBuffer execution needs implementation");
  }

  public void WaitForGPU()
  {
    // Would need real Vulkan device
  }

  public void WaitForFence(IFence _fence)
  {
    throw new NotImplementedException("VKFence.WaitForFence needs implementation");
  }

  public void WaitForFenceValue(IFence _fence, ulong _value)
  {
    WaitForFence(_fence);
  }

  public void Present() { }
  public void Present(ISwapChain _swapChain) { }

  public MemoryInfo GetMemoryInfo()
  {
    return new MemoryInfo
    {
      TotalMemory = 8UL * 1024 * 1024 * 1024,
      AvailableMemory = 6UL * 1024 * 1024 * 1024,
      UsedMemory = 2UL * 1024 * 1024 * 1024,
      Budget = 7UL * 1024 * 1024 * 1024,
      CurrentUsage = 1UL * 1024 * 1024 * 1024,
      CurrentReservation = 500UL * 1024 * 1024
    };
  }

  public ulong GetTotalMemory() => GetMemoryInfo().TotalMemory;
  public ulong GetAvailableMemory() => GetMemoryInfo().AvailableMemory;

  public bool SupportsFormat(TextureFormat _format, FormatUsage _usage) => true;

  public uint GetFormatBytesPerPixel(TextureFormat _format)
  {
    return _format switch
    {
      TextureFormat.R8G8B8A8_UNORM => 4,
      TextureFormat.R16G16B16A16_FLOAT => 8,
      TextureFormat.R32G32B32A32_FLOAT => 16,
      TextureFormat.D32_FLOAT => 4,
      TextureFormat.D24_UNORM_S8_UINT => 4,
      _ => 4
    };
  }

  public VKSampleCountFlags GetSupportedSampleCounts(TextureFormat _format)
  {
    return VKSampleCountFlags.Count1 | VKSampleCountFlags.Count2 | VKSampleCountFlags.Count4 | VKSampleCountFlags.Count8;
  }

  public void SetDebugName(IResource _resource, string _name) { }
  public void BeginEvent(string _name) { }
  public void EndEvent() { }
  public void SetMarker(string _name) { }

  public void Dispose()
  {
    if(p_disposed)
      return;

    if(p_graphicsCommandPool.Handle != 0)
      p_vk.DestroyCommandPool(p_device, p_graphicsCommandPool, null);
    if(p_computeCommandPool.Handle != 0)
      p_vk.DestroyCommandPool(p_device, p_computeCommandPool, null);
    if(p_transferCommandPool.Handle != 0)
      p_vk.DestroyCommandPool(p_device, p_transferCommandPool, null);

    if(p_device.Handle != 0)
      p_vk.DestroyDevice(p_device, null);

    if(p_instance.Handle != 0)
      p_vk.DestroyInstance(p_instance, null);

    p_disposed = true;
  }
}
