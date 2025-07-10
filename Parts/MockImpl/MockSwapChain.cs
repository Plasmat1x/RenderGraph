using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace MockImpl;

public class MockSwapChain: ISwapChain
{
  private readonly List<ITexture> p_backBuffers;

  public MockSwapChain(SwapChainDescription _description)
  {
    Description = _description;
    p_backBuffers = new List<ITexture>();

    for(uint i = 0; i < _description.BufferCount; i++)
    {
      var texDesc = new TextureDescription
      {
        Name = $"BackBuffer{i}",
        Width = _description.Width,
        Height = _description.Height,
        Depth = 1,
        MipLevels = 1,
        ArraySize = 1,
        Format = _description.Format,
        SampleCount = _description.SampleCount,
        TextureUsage = TextureUsage.RenderTarget
      };
      p_backBuffers.Add(new MockTexture(i + 100, texDesc));
    }
  }

  public SwapChainDescription Description { get; }
  public uint CurrentBackBufferIndex { get; private set; }

  public ITexture GetBackBuffer(uint _index)
  {
    if(_index >= p_backBuffers.Count)
      throw new ArgumentOutOfRangeException(nameof(_index));
    return p_backBuffers[(int)_index];
  }

  public void Present(uint _syncInterval = 0)
  {
    CurrentBackBufferIndex = (CurrentBackBufferIndex + 1) % Description.BufferCount;
    Console.WriteLine($"    [SwapChain] Present (sync: {_syncInterval}, next buffer: {CurrentBackBufferIndex})");
  }

  public void Resize(uint _width, uint _height)
  {
    Console.WriteLine($"    [SwapChain] Resize to {_width}x{_height}");
  }

  public IntPtr GetNativeHandle() => new IntPtr(11111);

  public void Dispose()
  {
    Console.WriteLine($"    [SwapChain] Disposed swapchain");
    foreach(var buffer in p_backBuffers)
    {
      buffer?.Dispose();
    }
    p_backBuffers.Clear();
  }
}
