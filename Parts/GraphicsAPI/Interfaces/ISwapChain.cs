namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс swapchain
/// </summary>
public interface ISwapChain: IDisposable
{
  SwapChainDescription Description { get; }
  uint CurrentBackBufferIndex { get; }
  ITexture GetBackBuffer(uint _index);
  void Present(uint _syncInterval = 0);
  void Resize(uint _width, uint _height);
  IntPtr GetNativeHandle();
}