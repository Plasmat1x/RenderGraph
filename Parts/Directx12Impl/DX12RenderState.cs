using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl;
public class DX12RenderState: IRenderState
{
  private readonly ComPtr<ID3D12Device> p_device;
  private ComPtr<ID3D12PipelineState> p_pipelineState;
  private RenderStateDescription p_description;
  private BlendDesc p_blendDesc;
  private DepthStencilDesc p_depthStencilDesc;
  private RasterizerDesc p_rasterizerDesc;

  public DX12RenderState(ComPtr<ID3D12Device> _device, RenderStateDescription _desc)
  {
    p_description = _desc;
  }

  public RenderStateDescription Description => p_description;

  public string Name { get; set; }

  public ResourceType ResourceType { get; private set; }

  public bool IsDisposed => throw new NotImplementedException();

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  private void CreatePipelineState()
  {
    throw new NotImplementedException();
  }

  private BlendDesc ConvertBlendState(BlendStateDescription _desc)
  {
    throw new NotImplementedException();
  }

  private DepthStencilDesc ConvertDepthStencilState(DepthStencilStateDescription _desc)
  {
    throw new NotImplementedException();
  }

  private RasterizerDesc ConvertRasterizerState(RasterizerStateDescription _desc)
  {
    throw new NotImplementedException();
  }
}
