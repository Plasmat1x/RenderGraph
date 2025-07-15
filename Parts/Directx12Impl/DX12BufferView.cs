using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl;

public class DX12BufferView: IBufferView
{
  private DX12Buffer p_buffer;
  private BufferViewType p_viewType;
  private BufferViewDescription p_description;
  private CpuDescriptorHandle p_dsecriptor;

  public DX12BufferView() { }

  public IBuffer Buffer => throw new NotImplementedException();

  public BufferViewType ViewType => throw new NotImplementedException();

  public BufferViewDescription Description => throw new NotImplementedException();

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  private void CreateView()
  {

  }
}
