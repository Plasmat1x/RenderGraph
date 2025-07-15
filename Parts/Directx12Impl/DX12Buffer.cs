using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl;

public unsafe class DX12Buffer: IBuffer
{
  private ComPtr<ID3D12Resource> p_resource;
  private BufferDescription p_description;
  private ResourceDesc p_resourceDesc;
  private ResourceStates p_currentState;
  private ComPtr<ID3D12Device> p_device;
  private void* p_mappedData;
  private CpuDescriptorHandle p_srvDescriptor;
  private CpuDescriptorHandle p_uavDescriptor;
  private CpuDescriptorHandle p_cbvDescriptor;
  private Dictionary<BufferViewDescription, DX12BufferView> p_views;

  public DX12Buffer() { }

  public BufferDescription Description => throw new NotImplementedException();

  public ulong Size => throw new NotImplementedException();

  public uint Stride => throw new NotImplementedException();

  public BufferUsage Usage => throw new NotImplementedException();

  public bool IsMapped => throw new NotImplementedException();

  public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

  public ResourceType ResourceType => throw new NotImplementedException();

  public bool IsDisposed => throw new NotImplementedException();

  public IBufferView CreateView(BufferViewDescription _description)
  {
    throw new NotImplementedException();
  }

  public T[] GetData<T>(ulong _offset = 0, ulong _count = 0) where T : struct
  {
    throw new NotImplementedException();
  }

  public T GetData<T>(ulong _offset = 0) where T : struct
  {
    throw new NotImplementedException();
  }

  public IBufferView GetDefaultShaderResourceView()
  {
    throw new NotImplementedException();
  }

  public IBufferView GetDefaultUnorderedAccessView()
  {
    throw new NotImplementedException();
  }

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public nint Map(MapMode _mode = MapMode.Write)
  {
    throw new NotImplementedException();
  }

  public void SetData<T>(T[] _data, ulong _offset = 0) where T : struct
  {
    throw new NotImplementedException();
  }

  public void SetData<T>(T _data, ulong _offset = 0) where T : struct
  {
    throw new NotImplementedException();
  }

  public void Unmap()
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  private void CreateResource()
  {
    throw new NotImplementedException();
  }

  private void CreateDefaultView()
  {
    throw new NotImplementedException();
  }

  private ResourceStates DetermineResourceStates()
  {
    throw new NotImplementedException();
  }
}