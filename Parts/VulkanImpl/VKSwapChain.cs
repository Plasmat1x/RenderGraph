using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace VulkanImpl;

public class VKSwapChain : ISwapChain
{
  private readonly SwapChainDescription p_description;
  private bool p_disposed;
  private uint p_currentBackBufferIndex;

  public VKSwapChain(SwapChainDescription _description)
  {
    p_description = _description;
  }

  public SwapChainDescription Description => p_description;
  public uint Width => p_description.Width;
  public uint Height => p_description.Height;
  public TextureFormat Format => p_description.Format;
  public uint BufferCount => p_description.BufferCount;
  public uint CurrentBackBufferIndex => p_currentBackBufferIndex;

  public bool IsFullscreen() => false;

  public void SetFullscreenState(bool _fullscreen, IMonitor _monitor) { }

  public ITexture GetBackBuffer(uint _index)
  {
    Console.WriteLine($"  [Vulkan] GetBackBuffer {_index}");
    throw new NotImplementedException("VKSwapChain - GetBackBuffer not implemented");
  }

  public ITextureView GetBackBufferRTV(uint _index)
  {
    Console.WriteLine($"  [Vulkan] GetBackBufferRTV {_index}");
    throw new NotImplementedException("VKSwapChain - GetBackBufferRTV not implemented");
  }

  public void Present(uint _syncInterval = 0)
  {
    Console.WriteLine($"  [Vulkan] Present");
  }

  public void Resize(uint _width, uint _height)
  {
    Console.WriteLine($"  [Vulkan] Resize {_width}x{_height}");
    throw new NotImplementedException("VKSwapChain - Resize not implemented");
  }

  public IntPtr GetNativeHandle() => IntPtr.Zero;

  public void Dispose()
  {
    if(p_disposed)
      return;
    Console.WriteLine($"  [Vulkan] Dispose swapchain");
    p_disposed = true;
  }
}
