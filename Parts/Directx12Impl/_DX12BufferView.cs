//using GraphicsAPI.Descriptions;
//using GraphicsAPI.Interfaces;

//using Resources.Enums;

//using Silk.NET.Direct3D12;
//using Silk.NET.DXGI;

//namespace Directx12Impl;

//public class _DX12BufferView : IBufferView
//{
//  private _DX12Buffer p_buffer;
//  private BufferViewType p_viewType;
//  private _BufferViewDescription p_description;
//  private DescriptorAllocation p_allocation;
//  private bool p_disposed;

//  public _DX12BufferView(
//    _DX12Buffer _buffer,
//    BufferViewType _viewType,
//    _BufferViewDescription _desc,
//    DescriptorAllocation _allocation) 
//  {
//    p_buffer = _buffer ?? throw new ArgumentNullException(nameof(_buffer));
//    p_viewType = _viewType;
//    p_description = _desc ?? throw new ArgumentNullException(nameof(_desc));
//    p_allocation = _allocation ?? throw new ArgumentNullException(nameof(_allocation));
//  }

//  public IBuffer Buffer => p_buffer;

//  public BufferViewType ViewType => p_viewType;

//  public _BufferViewDescription Description => p_description;

//  public ConstantBufferViewDesc GetConstantBufferViewDesc()
//  {
//    if(p_viewType != BufferViewType.ConstantBuffer)
//      throw new InvalidOperationException("This is not a constant buffer view");

//    return new ConstantBufferViewDesc
//    {
//      BufferLocation = p_buffer.GetGPUVirtualAddress(),
//      SizeInBytes = (uint)DX12Helpers.AlignUp(p_buffer.Size, 256)
//    };
//  }

//  public VertexBufferView GetVertexBufferView()
//  {
//    if(p_viewType != BufferViewType.VertexBuffer)
//      throw new InvalidOperationException("This is not a vertex buffer view");

//    return new VertexBufferView
//    {
//      BufferLocation = p_buffer.GetGPUVirtualAddress(),
//      SizeInBytes = (uint)p_buffer.Size,
//      StrideInBytes = p_buffer.Stride
//    };
//  }

//  public IndexBufferView GetIndexBufferView()
//  {
//    if(p_viewType != BufferViewType.IndexBuffer)
//      throw new InvalidOperationException("This is not an index buffer view");

//    var format = p_buffer.Stride == 2
//        ? Format.FormatR16Uint
//        : Format.FormatR32Uint;

//    return new IndexBufferView
//    {
//      BufferLocation = p_buffer.GetGPUVirtualAddress(),
//      SizeInBytes = (uint)p_buffer.Size,
//      Format = format
//    };
//  }

//  public IndexBufferView GetIndexBufferView(IndexFormat _format)
//  {
//    if(p_viewType != BufferViewType.IndexBuffer)
//      throw new InvalidOperationException("This is not an index buffer view");

//    return new IndexBufferView
//    {
//      BufferLocation = p_buffer.GetGPUVirtualAddress(),
//      SizeInBytes = (uint)p_buffer.Size,
//      Format = DX12Helpers.ConvertIndexFormat(_format)
//    };
//  }

//  public IntPtr GetNativeHandle()
//  {
//    ThrowIfDisposed();
//    return (IntPtr)p_allocation.CpuHandle.Ptr;
//  }

//  public CpuDescriptorHandle GetDescriptorHandle()
//  {
//    ThrowIfDisposed();
//    return p_allocation.CpuHandle;
//  }

//  public void Dispose()
//  {
//    if(p_disposed)
//      return;

//    p_allocation.Dispose();

//    p_disposed = true;
//    GC.SuppressFinalize(this);
//  }

//  private void ThrowIfDisposed()
//  {
//    if(p_disposed)
//      throw new ObjectDisposedException(nameof(_DX12BufferView));
//  }
//}
