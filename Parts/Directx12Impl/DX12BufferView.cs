using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl;

// <summary>
/// Класс для работы с DX12 представлениями буферов
/// </summary>
public unsafe class DX12BufferView: IBufferView
{
  private readonly DX12Buffer p_buffer;
  private readonly BufferViewDescription p_description;
  private readonly DX12DescriptorHandle p_descriptorHandle;
  private bool p_disposed;

  public DX12BufferView(DX12Buffer _buffer, BufferViewDescription _description, DX12DescriptorHeapManager _descriptorManager)
  {
    p_buffer = _buffer ?? throw new ArgumentNullException(nameof(_buffer));
    p_description = _description;

    // Создаем дескриптор для CBV/SRV/UAV
    p_descriptorHandle = CreateDescriptor(_descriptorManager);
  }

  public IBuffer Buffer => p_buffer;
  public BufferViewDescription Description => p_description;
  public bool IsDisposed => p_disposed;

  public BufferViewType ViewType => throw new NotImplementedException();

  /// <summary>
  /// Получить handle дескриптора
  /// </summary>
  public DX12DescriptorHandle GetDescriptorHandle() => p_descriptorHandle;

  /// <summary>
  /// Получить связанный DX12 ресурс
  /// </summary>
  public ID3D12Resource* GetResource() => p_buffer.GetResource();
  public nint GetNativeHandle() => (nint)p_descriptorHandle.CpuHandle.Ptr;

  private DX12DescriptorHandle CreateDescriptor(DX12DescriptorHeapManager _descriptorManager)
  {
    var allocation = _descriptorManager.AllocateCBVSRVUAV();

    // Определяем тип представления на основе usage буфера
    if(p_buffer.Description.BufferUsage == BufferUsage.Constant)
    {
      CreateConstantBufferView(allocation.CpuHandle);
    }
    else if((p_buffer.Description.BindFlags & BindFlags.UnorderedAccess) != 0)
    {
      CreateUnorderedAccessView(allocation.CpuHandle);
    }
    else
    {
      CreateShaderResourceView(allocation.CpuHandle);
    }

    return new DX12DescriptorHandle(allocation.CpuHandle, allocation.GpuHandle);
  }

  private void CreateConstantBufferView(CpuDescriptorHandle _handle)
  {
    var cbvDesc = new ConstantBufferViewDesc
    {
      BufferLocation = p_buffer.GetGPUVirtualAddress() + p_description.FirstElement * p_buffer.Description.Stride,
      SizeInBytes = (uint)(p_description.NumElements * p_buffer.Description.Stride)
    };

    // Выравниваем размер до 256 байт (требование DX12)
    cbvDesc.SizeInBytes = (cbvDesc.SizeInBytes + 255) & ~255u;

    p_buffer.GetResource()->GetDevice(out ComPtr<ID3D12Device> device);
    device.CreateConstantBufferView(&cbvDesc, _handle);
  }

  private void CreateShaderResourceView(CpuDescriptorHandle _handle)
  {
    var srvDesc = new ShaderResourceViewDesc();

    if((p_description.Flags & BufferViewFlags.Raw) != 0)
    {
      // Raw buffer (ByteAddressBuffer)
      srvDesc.Format = Silk.NET.DXGI.Format.FormatR32Typeless;
      srvDesc.ViewDimension = SrvDimension.Buffer;
      srvDesc.Shader4ComponentMapping = DX12Helpers.D3D12DefaultShader4ComponentMapping;
      srvDesc.Anonymous.Buffer.FirstElement = p_description.FirstElement;
      srvDesc.Anonymous.Buffer.NumElements = (uint)(p_description.NumElements * p_buffer.Description.Stride / 4);
      srvDesc.Anonymous.Buffer.StructureByteStride = 0;
      srvDesc.Anonymous.Buffer.Flags = BufferSrvFlags.Raw;
    }
    else if(p_buffer.Description.Stride > 0)
    {
      // Structured buffer
      srvDesc.Format = Silk.NET.DXGI.Format.FormatUnknown;
      srvDesc.ViewDimension = SrvDimension.Buffer;
      srvDesc.Shader4ComponentMapping = DX12Helpers.D3D12DefaultShader4ComponentMapping;
      srvDesc.Anonymous.Buffer.FirstElement = p_description.FirstElement;
      srvDesc.Anonymous.Buffer.NumElements = (uint)p_description.NumElements;
      srvDesc.Anonymous.Buffer.StructureByteStride = p_buffer.Description.Stride;
      srvDesc.Anonymous.Buffer.Flags = BufferSrvFlags.None;
    }
    else
    {
      throw new InvalidOperationException("Buffer must have either stride > 0 or Raw flag for SRV");
    }

    p_buffer.GetResource()->GetDevice(out ComPtr<ID3D12Device> device);
    device.CreateShaderResourceView(p_buffer.GetResource(), &srvDesc, _handle);
  }

  private void CreateUnorderedAccessView(CpuDescriptorHandle _handle)
  {
    var uavDesc = new UnorderedAccessViewDesc();

    if((p_description.Flags & BufferViewFlags.Raw) != 0)
    {
      // Raw buffer (RWByteAddressBuffer)
      uavDesc.Format = Silk.NET.DXGI.Format.FormatR32Typeless;
      uavDesc.ViewDimension = UavDimension.Buffer;
      uavDesc.Anonymous.Buffer.FirstElement = p_description.FirstElement;
      uavDesc.Anonymous.Buffer.NumElements = (uint)(p_description.NumElements * p_buffer.Description.Stride / 4);
      uavDesc.Anonymous.Buffer.StructureByteStride = 0;
      uavDesc.Anonymous.Buffer.CounterOffsetInBytes = 0;
      uavDesc.Anonymous.Buffer.Flags = BufferUavFlags.Raw;
    }
    else if(p_buffer.Description.Stride > 0)
    {
      // Structured buffer
      uavDesc.Format = Silk.NET.DXGI.Format.FormatUnknown;
      uavDesc.ViewDimension = UavDimension.Buffer;
      uavDesc.Anonymous.Buffer.FirstElement = p_description.FirstElement;
      uavDesc.Anonymous.Buffer.NumElements = (uint)p_description.NumElements;
      uavDesc.Anonymous.Buffer.StructureByteStride = p_buffer.Description.Stride;
      uavDesc.Anonymous.Buffer.CounterOffsetInBytes = 0;
      uavDesc.Anonymous.Buffer.Flags = BufferUavFlags.None;
    }
    else
    {
      throw new InvalidOperationException("Buffer must have either stride > 0 or Raw flag for UAV");
    }

    p_buffer.GetResource()->GetDevice(out ComPtr<ID3D12Device> device);
    device.CreateUnorderedAccessView(p_buffer.GetResource(), (ID3D12Resource*)null, &uavDesc, _handle);
  }

  public void Dispose()
  {
    if(!p_disposed)
    {
      // Дескрипторы освобождаются автоматически через DescriptorHeapManager
      p_disposed = true;
    }
  }
}