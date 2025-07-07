namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс swapchain
/// </summary>
public interface ISwapChain: IDisposable
{
  SwapChainDescription Description { get; }
  uint CurrentBackBufferIndex { get; }
  ITexture GetBackBuffer(uint index);
  void Present(uint syncInterval = 0);
  void Resize(uint width, uint height);
  IntPtr GetNativeHandle();
}