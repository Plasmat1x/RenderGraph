using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace MockImpl;

public class MockSwapChain: ISwapChain
{
  public SwapChainDescription Description { get; }
  public uint CurrentBackBufferIndex { get; private set; }
  private readonly List<ITexture> _backBuffers;

  public MockSwapChain(SwapChainDescription description)
  {
    Description = description;
    _backBuffers = new List<ITexture>();

    // Create back buffers
    for(uint i = 0; i < description.BufferCount; i++)
    {
      var texDesc = new TextureDescription
      {
        Name = $"BackBuffer{i}",
        Width = description.Width,
        Height = description.Height,
        Depth = 1,
        MipLevels = 1,
        ArraySize = 1,
        Format = description.Format,
        SampleCount = description.SampleCount,
        TextureUsage = TextureUsage.RenderTarget
      };
      _backBuffers.Add(new MockTexture(i + 100, texDesc));
    }
  }

  public ITexture GetBackBuffer(uint index)
  {
    if(index >= _backBuffers.Count)
      throw new ArgumentOutOfRangeException(nameof(index));
    return _backBuffers[(int)index];
  }

  public void Present(uint syncInterval = 0)
  {
    CurrentBackBufferIndex = (CurrentBackBufferIndex + 1) % Description.BufferCount;
    Console.WriteLine($"    [SwapChain] Present (sync: {syncInterval}, next buffer: {CurrentBackBufferIndex})");
  }

  public void Resize(uint width, uint height)
  {
    Console.WriteLine($"    [SwapChain] Resize to {width}x{height}");
    // В реальной реализации пересоздали бы back buffers
  }

  public IntPtr GetNativeHandle() => new IntPtr(11111);

  public void Dispose()
  {
    Console.WriteLine($"    [SwapChain] Disposed swapchain");
    foreach(var buffer in _backBuffers)
    {
      buffer?.Dispose();
    }
    _backBuffers.Clear();
  }
}
