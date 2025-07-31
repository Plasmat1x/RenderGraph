using GraphicsAPI;
using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace MockImpl;
public class MockGraphicsDevice: IGraphicsDevice
{
  private uint p_textureIdCounter = 1;
  private uint p_bufferIdCounter = 1;
  private uint p_shaderIdCounter = 1;
  private bool p_disposed = false;

  public MockGraphicsDevice()
  {
    Capabilities = new DeviceCapabilities
    {
      MaxTexture2DSize = 16384,
      MaxTexture3DSize = 2048,
      MaxTextureArrayLayers = 2048,
      MaxColorAttachments = 8,
      MaxVertexAttributes = 16,
      MaxVertexBuffers = 16,
      SupportedSampleCounts = SampleCountFlags.Count1 | SampleCountFlags.Count2 | SampleCountFlags.Count4 | SampleCountFlags.Count8,
      SupportsGeometryShader = true,
      SupportsTessellation = true,
      SupportsComputeShader = true,
      SupportsMultiDrawIndirect = true,
      SupportsAnisotropicFiltering = true,
      SupportsTextureCompressionBC = true
    };
  }

  public string Name => "Mock Graphics Device";
  public API API => API.DirectX12;
  public DeviceCapabilities Capabilities { get; }


  public ITexture CreateTexture(TextureDescription _description)
  {
    Console.WriteLine($"  [GPU] Creating texture: {_description.Name} ({_description.Width}x{_description.Height}x{_description.Depth}, {_description.Format}, Mips: {_description.MipLevels})");
    return new MockTexture(p_textureIdCounter++, _description);
  }

  public IBuffer CreateBuffer(BufferDescription _description)
  {
    Console.WriteLine($"  [GPU] Creating buffer: {_description.Name} ({_description.Size} bytes, {_description.BufferUsage}, Stride: {_description.Stride})");
    return new MockBuffer(p_bufferIdCounter++, _description);
  }

  public IShader CreateShader(ShaderDescription _description)
  {
    Console.WriteLine($"  [GPU] Creating shader: {_description.Name} ({_description.Stage})");
    return new MockShader(p_shaderIdCounter++, _description);
  }

  public IRenderState CreateRenderState(RenderStateDescription _description)
  {
    Console.WriteLine($"  [GPU] Creating render state");
    return new MockRenderState(_description);
  }

  public ISampler CreateSampler(SamplerDescription _description)
  {
    Console.WriteLine($"  [GPU] Creating sampler: {_description.Name}");
    return new MockSampler(_description);
  }

  public CommandBuffer CreateCommandBuffer()
  {
    return new MockCommandBuffer(CommandBufferType.Direct);
  }

  public CommandBuffer CreateCommandBuffer(CommandBufferType _type)
  {
    Console.WriteLine($"  [GPU] Creating command buffer ({_type})");
    return new MockCommandBuffer(_type);
  }

  public void ExecuteCommandBuffer(CommandBuffer _commandBuffer)
  {
    var mockCmd = (MockCommandBuffer)_commandBuffer;
    Console.WriteLine($"  [GPU] Executing command buffer ({mockCmd.Type}) with {mockCmd.CommandCount} commands");
    Thread.Sleep(10 + mockCmd.CommandCount); // Simulate GPU work
  }

  public void ExecuteCommandBuffers(CommandBuffer[] _commandBuffers)
  {
    Console.WriteLine($"  [GPU] Executing {_commandBuffers.Length} command buffers");
    foreach(var cmd in _commandBuffers)
    {
      ExecuteCommandBuffer(cmd);
    }
  }

  public void WaitForGPU()
  {
    Console.WriteLine("  [GPU] Waiting for GPU...");
    Thread.Sleep(20);
  }

  public void WaitForFence(IFence _fence)
  {
    Console.WriteLine($"  [GPU] Waiting for fence (value: {_fence.Value})");
    Thread.Sleep(5);
  }

  public IFence CreateFence(ulong _initialValue = 0)
  {
    Console.WriteLine($"  [GPU] Creating fence (initial value: {_initialValue})");
    return new MockFence(_initialValue);
  }

  public MemoryInfo GetMemoryInfo()
  {
    return new MemoryInfo
    {
      TotalMemory = 8UL * 1024 * 1024 * 1024, // 8GB
      AvailableMemory = 6UL * 1024 * 1024 * 1024, // 6GB
      UsedMemory = 2UL * 1024 * 1024 * 1024, // 2GB
      Budget = 7UL * 1024 * 1024 * 1024,
      CurrentUsage = 1UL * 1024 * 1024 * 1024,
      CurrentReservation = 500UL * 1024 * 1024
    };
  }

  public ulong GetTotalMemory() => GetMemoryInfo().TotalMemory;
  public ulong GetAvailableMemory() => GetMemoryInfo().AvailableMemory;


  public ISwapChain CreateSwapChain(SwapChainDescription _description, nint _windowHandle)
  {
    Console.WriteLine($"  [GPU] Creating swapchain ({_description.Width}x{_description.Height}, {_description.Format})");
    return new MockSwapChain(_description);
  }

  public void Present()
  {
    Console.WriteLine("  [GPU] Present frame");
    Thread.Sleep(16);
  }

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

  public SampleCountFlags GetSupportedSampleCounts(TextureFormat _format) => SampleCountFlags.Count1 | SampleCountFlags.Count2 | SampleCountFlags.Count4 | SampleCountFlags.Count8;

  public void Dispose()
  {
    if(p_disposed)
      return;

    Console.WriteLine("  [GPU] Graphics device disposed");
    p_disposed = true;

  }
}