using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

namespace VulkanImpl;

public class VKSwapChain : ISwapChain
{
  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public SwapChainDescription Description { get; }
  public uint CurrentBackBufferIndex { get; }

  public ITexture GetBackBuffer(uint _index)
  {
    throw new NotImplementedException();
  }

  public ITextureView GetBackBufferRTV(uint _index)
  {
    throw new NotImplementedException();
  }

  public void Present(uint _syncInterval = 0)
  {
    throw new NotImplementedException();
  }

  public void Resize(uint _width, uint _height)
  {
    throw new NotImplementedException();
  }

  public void SetFullscreenState(bool _fullscreen, IMonitor _monitor = null)
  {
    throw new NotImplementedException();
  }

  public bool IsFullscreen()
  {
    throw new NotImplementedException();
  }

  public IntPtr GetNativeHandle()
  {
    throw new NotImplementedException();
  }
}
