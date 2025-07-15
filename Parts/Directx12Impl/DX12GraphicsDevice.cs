using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Directx12Impl;
public class DX12GraphicsDevice: IGraphicsDevice
{
  private ComPtr<ID3D12Device> p_device;
  private ComPtr<IDXGIFactory4> p_dxgiFactory;
  private ComPtr<ID3D12CommandQueue> p_directQueue;
  private ComPtr<ID3D12CommandQueue> p_computeQueue;
  private ComPtr<ID3D12CommandQueue> p_copyQueue;
  private uint p_rtvDescriptorSize;
  private uint p_dsvDescriptorSize;
  private uint p_cbvSrvUavDescriptorSize;
  private uint p_samplerDescriprotSize;
  private ComPtr<ID3D12DescriptorHeap> p_rtvHeap;
  private ComPtr<ID3D12DescriptorHeap> p_dsvHeap;
  private List<ComPtr<ID3D12DescriptorHeap>> p_shaderVisibleHeaps;
  private uint p_currentFrameIndex;
  private List<ulong> p_fenceValues;
  private ComPtr<ID3D12Fence> p_fence;
  private nint p_fenceEvent;

  public DX12GraphicsDevice()
  {

  }

  public string Name { get; private set; }

  public API API { get; private set; }

  public DeviceCapabilities Capabilities { get; private set; }

  public ITexture CreateTexture(TextureDescription _desc)
  {
    throw new NotImplementedException();
  }

  public IBuffer CreateBuffer(BufferDescription _desc)
  {
    throw new NotImplementedException();
  }

  public IShader CreateShader(ShaderDescription _desc)
  {
    throw new NotImplementedException();
  }

  public IRenderState CreateRenderState(RenderStateDescription _desc)
  {
    throw new NotImplementedException();
  }

  public ISampler CreateSampler(SamplerDescription _desc)
  {
    throw new NotImplementedException();
  }

  public CommandBuffer CreateCommandBuffer()
  {
    throw new NotImplementedException();
  }

  public CommandBuffer CreateCommandBuffer(CommandBufferType _type)
  {
    throw new NotImplementedException();
  }

  public IFence CreateFence(ulong _initialValue = 0)
  {
    throw new NotImplementedException();
  }

  public void ExecuteCommandBuffer(CommandBuffer _commandBuffer)
  {
    throw new NotImplementedException();
  }

  public void ExecuteCommandBuffers(CommandBuffer[] _commandBuffers)
  {
    throw new NotImplementedException();
  }

  public void WaitForGPU()
  {
    throw new NotImplementedException();
  }

  public void WaitForFence(IFence _fence)
  {
    throw new NotImplementedException();
  }

  public ISwapChain CreateSwapChain(SwapChainDescription _desc)
  {
    throw new NotImplementedException();
  }

  public void Present()
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public MemoryInfo GetMemoryInfo()
  {
    throw new NotImplementedException();
  }

  public ulong GetTotalMemory()
  {
    throw new NotImplementedException();
  }

  public bool SupportsFormat(TextureFormat _format, FormatUsage _usage)
  {
    throw new NotImplementedException();
  }

  public uint GetFormatBytesPerPixel(TextureFormat _format)
  {
    throw new NotImplementedException();
  }

  public SampleCountFlags GetSupportedSampleCounts(TextureFormat _textureFormat)
  {
    throw new NotImplementedException();
  }

  public ulong GetAvailableMemory()
  {
    throw new NotImplementedException();
  }

  private void CreateDevice()
  {
    throw new NotImplementedException();
  }

  private void CreateCommandQueues()
  {
    throw new NotImplementedException();
  }

  private void CreateDescriptorHeaps()
  {
    throw new NotImplementedException();
  }

  private void CreateFence()
  {
    throw new NotImplementedException();
  }

  private void WaitForFenceValue(ulong _fenceValue)
  {
    throw new NotImplementedException();
  }

  private ComPtr<ID3D12Device> GetID3D12Device()
  {
    throw new NotImplementedException();
  }

  private CpuDescriptorHandle AllocateDescriptor(DescriptorHeapType _type)
  {
    throw new NotImplementedException();
  }
}
