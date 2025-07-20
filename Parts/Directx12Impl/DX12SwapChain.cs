using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;
public class DX12SwapChain: ISwapChain
{
  private ComPtr<IDXGISwapChain3> p_swapChain;
  private ComPtr<ID3D12Device> p_device;
  private SwapChainDescription p_swapChainDescription;
  private List<DX12Texture> p_backBuffers;
  private uint p_currentBackBufferIndex;
  private uint p_frameCount;

  public DX12SwapChain()
  {

  }

  public SwapChainDescription Description => p_swapChainDescription;

  public uint CurrentBackBufferIndex => p_currentBackBufferIndex;

  public ITexture GetBackBuffer(uint _index)
  {
    throw new NotImplementedException();
  }

  public nint GetNativeHandle()
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

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  private void CreateBackBuffers()
  {
    throw new NotImplementedException();
  }

  private void ReleaseBackBuffers()
  {
    throw new NotImplementedException();
  }
}
