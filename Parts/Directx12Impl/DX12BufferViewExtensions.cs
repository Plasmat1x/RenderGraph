using Resources.Enums;

using Silk.NET.Direct3D12;

namespace Directx12Impl;

/// <summary>
/// Расширения для DX12 буферных представлений
/// </summary>
public static class DX12BufferViewExtensions
{
  /// <summary>
  /// Получить описание Vertex Buffer View
  /// </summary>
  public static VertexBufferView GetVertexBufferView(this DX12BufferView _view)
  {
    var buffer = _view.Buffer as DX12Buffer;
    if(buffer == null)
      throw new InvalidOperationException("Invalid buffer type");

    return new VertexBufferView
    {
      BufferLocation = buffer.GetGPUVirtualAddress() + _view.Description.FirstElement * buffer.Description.Stride,
      SizeInBytes = (uint)(_view.Description.NumElements * buffer.Description.Stride),
      StrideInBytes = buffer.Description.Stride
    };
  }

  /// <summary>
  /// Получить описание Index Buffer View
  /// </summary>
  public static IndexBufferView GetIndexBufferView(this DX12BufferView _view, IndexFormat _format)
  {
    var buffer = _view.Buffer as DX12Buffer;
    if(buffer == null)
      throw new InvalidOperationException("Invalid buffer type");

    var dxgiFormat = _format switch
    {
      IndexFormat.UInt16 => Silk.NET.DXGI.Format.FormatR16Uint,
      IndexFormat.UInt32 => Silk.NET.DXGI.Format.FormatR32Uint,
      _ => throw new ArgumentException($"Unsupported index format: {_format}")
    };

    var elementSize = _format == IndexFormat.UInt16 ? 2u : 4u;

    return new IndexBufferView
    {
      BufferLocation = buffer.GetGPUVirtualAddress() + _view.Description.FirstElement * elementSize,
      SizeInBytes = (uint)(_view.Description.NumElements * elementSize),
      Format = dxgiFormat
    };
  }

  /// <summary>
  /// Получить handle дескриптора для Constant Buffer View
  /// </summary>
  public static CpuDescriptorHandle GetConstantBufferView(this DX12BufferView _view)
  {
    return _view.GetDescriptorHandle().CpuHandle;
  }

  /// <summary>
  /// Получить handle дескриптора для Shader Resource View
  /// </summary>
  public static DX12DescriptorHandle GetShaderResourceView(this DX12BufferView _view)
  {
    return _view.GetDescriptorHandle();
  }

  /// <summary>
  /// Получить handle дескриптора для Unordered Access View
  /// </summary>
  public static DX12DescriptorHandle GetUnorderedAccessView(this DX12BufferView _view)
  {
    return _view.GetDescriptorHandle();
  }
}