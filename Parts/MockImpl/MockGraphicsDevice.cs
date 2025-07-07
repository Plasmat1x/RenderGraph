using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace MockImpl;
public class MockGraphicsDevice: IGraphicsDevice
{
  private uint _textureIdCounter = 1;
  private uint _bufferIdCounter = 1;
  private uint _shaderIdCounter = 1;
  private bool _disposed = false;

  public string Name => "Mock Graphics Device";
  public GraphicsAPI.Enums.GraphicsAPI API => GraphicsAPI.Enums.GraphicsAPI.DirectX12;
  public DeviceCapabilities Capabilities { get; }

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

  public ITexture CreateTexture(TextureDescription description)
  {
    Console.WriteLine($"  [GPU] Creating texture: {description.Name} ({description.Width}x{description.Height}x{description.Depth}, {description.Format}, Mips: {description.MipLevels})");
    return new MockTexture(_textureIdCounter++, description);
  }

  public IBuffer CreateBuffer(BufferDescription description)
  {
    Console.WriteLine($"  [GPU] Creating buffer: {description.Name} ({description.Size} bytes, {description.BufferUsage}, Stride: {description.Stride})");
    return new MockBuffer(_bufferIdCounter++, description);
  }

  public IShader CreateShader(ShaderDescription description)
  {
    Console.WriteLine($"  [GPU] Creating shader: {description.Name} ({description.Stage})");
    return new MockShader(_shaderIdCounter++, description);
  }

  public IRenderState CreateRenderState(RenderStateDescription description)
  {
    Console.WriteLine($"  [GPU] Creating render state");
    return new MockRenderState(description);
  }

  public ISampler CreateSampler(SamplerDescription description)
  {
    Console.WriteLine($"  [GPU] Creating sampler: {description.Name}");
    return new MockSampler(description);
  }

  public CommandBuffer CreateCommandBuffer()
  {
    return new MockCommandBuffer(CommandBufferType.Direct);
  }

  public CommandBuffer CreateCommandBuffer(CommandBufferType type)
  {
    Console.WriteLine($"  [GPU] Creating command buffer ({type})");
    return new MockCommandBuffer(type);
  }

  public void ExecuteCommandBuffer(CommandBuffer commandBuffer)
  {
    var mockCmd = (MockCommandBuffer)commandBuffer;
    Console.WriteLine($"  [GPU] Executing command buffer ({mockCmd.Type}) with {mockCmd.CommandCount} commands");
    Thread.Sleep(10 + mockCmd.CommandCount); // Simulate GPU work
  }

  public void ExecuteCommandBuffers(CommandBuffer[] commandBuffers)
  {
    Console.WriteLine($"  [GPU] Executing {commandBuffers.Length} command buffers");
    foreach(var cmd in commandBuffers)
    {
      ExecuteCommandBuffer(cmd);
    }
  }

  public void WaitForGPU()
  {
    Console.WriteLine("  [GPU] Waiting for GPU...");
    Thread.Sleep(20);
  }

  public void WaitForFence(IFence fence)
  {
    Console.WriteLine($"  [GPU] Waiting for fence (value: {fence.Value})");
    Thread.Sleep(5);
  }

  public IFence CreateFence(ulong initialValue = 0)
  {
    Console.WriteLine($"  [GPU] Creating fence (initial value: {initialValue})");
    return new MockFence(initialValue);
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

  public ISwapChain CreateSwapChain(SwapChainDescription description)
  {
    Console.WriteLine($"  [GPU] Creating swapchain ({description.Width}x{description.Height}, {description.Format})");
    return new MockSwapChain(description);
  }

  public void Present()
  {
    Console.WriteLine("  [GPU] Present frame");
    Thread.Sleep(16); // Simulate 60 FPS
  }

  public bool SupportsFormat(TextureFormat format, FormatUsage usage)
  {
    // Для демо все форматы поддерживаются
    return true;
  }

  public uint GetFormatBytesPerPixel(TextureFormat format)
  {
    return format switch
    {
      TextureFormat.R8G8B8A8_UNORM => 4,
      TextureFormat.R16G16B16A16_FLOAT => 8,
      TextureFormat.R32G32B32A32_FLOAT => 16,
      TextureFormat.D32_FLOAT => 4,
      TextureFormat.D24_UNORM_S8_UINT => 4,
      _ => 4
    };
  }

  public SampleCountFlags GetSupportedSampleCounts(TextureFormat format)
  {
    return SampleCountFlags.Count1 | SampleCountFlags.Count2 | SampleCountFlags.Count4 | SampleCountFlags.Count8;
  }

  public void Dispose()
  {
    if(!_disposed)
    {
      Console.WriteLine("  [GPU] Graphics device disposed");
      _disposed = true;
    }
  }
}