using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System.Xml.Linq;

namespace Directx12Impl;

public unsafe class DX12Buffer: IBuffer
{
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly D3D12 p_d3d12;
  private readonly Dictionary<BufferViewType, DX12BufferView> p_views = [];
  private readonly BufferDescription p_description;
  private readonly Action<CpuDescriptorHandle> p_releaseDescriptorCallback;

  private ComPtr<ID3D12Resource> p_resource;
  private ResourceStates p_currentState;
  private void* p_mappedData;
  private ulong p_gpuVirtualAddress;
  private bool p_disposed;


  public DX12Buffer(
    ComPtr<ID3D12Device> _device,
    D3D12 _d3d12,
    BufferDescription _desc,
    Action<CpuDescriptorHandle> _releaseDescriptorCallback) 
  {
    p_device = _device;
    p_d3d12 = _d3d12;
    p_description = _desc ?? throw new ArgumentNullException(nameof(_desc));
    p_releaseDescriptorCallback = _releaseDescriptorCallback;

    CreateResource();
  }

  public BufferDescription Description => p_description;

  public ulong Size => p_description.Size;

  public uint Stride => p_description.Stride;

  public BufferUsage Usage => p_description.BufferUsage;

  public bool IsMapped => p_mappedData != null;

  public string Name => p_description.Name;

  public ResourceType ResourceType => ResourceType.Buffer;

  public bool IsDisposed => p_disposed;

  public IBufferView CreateView(BufferViewDescription _desc)
  {
    ThrowIfDisposed();

    BufferViewType viewType;

    if((p_description.BindFlags & BindFlags.VertexBuffer) != 0)
      viewType = BufferViewType.VertexBuffer;
    else if((p_description.BindFlags & BindFlags.IndexBuffer) != 0)
      viewType = BufferViewType.IndexBuffer;
    else if((p_description.BindFlags & BindFlags.ConstantBuffer) != 0)
      viewType = BufferViewType.ConstantBuffer;
    else if((p_description.BindFlags & BindFlags.ShaderResource) != 0)
      viewType = BufferViewType.ShaderResource;
    else if((p_description.BindFlags & BindFlags.UnorderedAccess) != 0)
      viewType = BufferViewType.UnorderedAccess;
    else
      throw new InvalidOperationException("Buffer has no valid bind flags for view creation");

    if(p_views.TryGetValue(viewType, out var existingView))
      return existingView;

    if(viewType == BufferViewType.VertexBuffer || viewType == BufferViewType.IndexBuffer)
    {
      var view = new DX12BufferView(this, viewType, _desc, default);
      p_views[viewType] = view;
      return view;
    }

    throw new NotImplementedException("CBV/SRV/UAV creation requires descriptor allocation");
  }

  public T[] GetData<T>(ulong _offset = 0, ulong _count = 0) where T : unmanaged
  {
    ThrowIfDisposed();

    if(p_description.Usage != ResourceUsage.Staging)
      throw new InvalidOperationException("Can only read data from Staging buffers");

    var elementSize = (ulong)sizeof(T);
    var dataSize = _count * elementSize;

    if(_offset +  dataSize > p_description.Size)
      throw new ArgumentException("Read exceeds buffer bounds");

    var result = new T[_count];
    var ptr = Map(MapMode.Read);

    fixed(T* pResult = result)
    {
      Buffer.MemoryCopy((byte*)ptr.ToPointer() + _offset, pResult, dataSize, dataSize);
    }

    Unmap();

    return result;
  }

  public T GetData<T>(ulong _offset = 0) where T : unmanaged
  {
    var array = GetData<T>(_offset, 1);
    return array[0];
  }

  public IBufferView GetDefaultShaderResourceView()
  {
    var desc = new BufferViewDescription
    {
      Offset = 0,
      Size = p_description.Size,
      Stride = p_description.StructureByteStride
    };
    return CreateView(desc);
  }

  public IBufferView GetDefaultUnorderedAccessView()
  {
    var desc = new BufferViewDescription
    {
      Offset = 0,
      Size = p_description.Size,
      Stride = p_description.StructureByteStride
    };
    return CreateView(desc);
  }

  public ulong GetMemorySize() => p_description.Size;
  
  public IntPtr GetNativeHandle()
  {
    ThrowIfDisposed();
    return (IntPtr)p_resource.Handle;
  }

  public IntPtr Map(MapMode _mode = MapMode.Write)
  {
    ThrowIfDisposed();

    if(p_mappedData == null)
      return (IntPtr)p_mappedData;

    if(p_description.Usage != ResourceUsage.Dynamic && p_description.Usage != ResourceUsage.Staging)
      throw new InvalidOperationException("Can only map Dynamic or Staging buffers");

    void* mappedPtr = default;
    var range = new Silk.NET.Direct3D12.Range { Begin = 0, End = 0};
    
    HResult hr = p_resource.Map(0, ref range, ref mappedPtr);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to map buffer: {hr}");

    p_mappedData = mappedPtr;

    return (IntPtr)p_mappedData;
  }

  public void SetData<T>(T[] _data, ulong _offset = 0) where T : unmanaged
  {
    ThrowIfDisposed();

    if(_data == null || _data.Length == 0)
      return;

    var dataSize = (ulong)(_data.Length * sizeof(T));
    if(_offset + dataSize > p_description.Size)
      throw new ArgumentException("Data exceeds buffer bounds");

    if(p_mappedData != null)
    {
      fixed(T* pData = _data)
      {
        Buffer.MemoryCopy(pData, (byte*)p_mappedData + _offset, p_description.Size - _offset, dataSize);
      }
    }
    else if(p_description.Usage == ResourceUsage.Dynamic || p_description.Usage == ResourceUsage.Staging)
    {
      var ptr = Map(MapMode.Write);
      fixed(T* pData = _data)
      {
        Buffer.MemoryCopy(pData, (byte*)ptr.ToPointer() + _offset, p_description.Size - _offset, dataSize);
      }
      Unmap();
    }
    else
    {
      throw new InvalidOperationException("Cannot set data on non-dynamic buffer. Use upload buffer and copy commands.");
    }
  }

  public void SetData<T>(T _data, ulong _offset = 0) where T : unmanaged
  {
    var array = new[] { _data };
    SetData(array, _offset);
  }

  public void Unmap()
  {
    ThrowIfDisposed();

    if(p_mappedData == null)
      return;

    var range = new Silk.NET.Direct3D12.Range { Begin = 0, End = (nuint)p_description.Size};

    p_resource.Unmap(0, ref range);
    p_mappedData = null;
  }

  public ulong GetGPUVirtualAddress()
  {
    ThrowIfDisposed();
    return p_gpuVirtualAddress;
  }

  public ResourceStates GetCurrentState() => p_currentState;
 
  public void SetCurrentState(ResourceStates _state) => p_currentState = _state;

  public ComPtr<ID3D12Resource> GetResource()
  {
    ThrowIfDisposed();
    return p_resource;
  }

  public void Dispose()
  {
    if(p_disposed) 
      return;

    foreach(var view in p_views.Values)
      view.Dispose();
    p_views.Clear();

    if(p_mappedData != null)
      Unmap();

    p_resource.Dispose();

    p_disposed = true;
    GC.SuppressFinalize(this);
  }

  private void CreateResource()
  {
    var heapType = p_description.Usage switch
    {
      ResourceUsage.Default => HeapType.Default,
      ResourceUsage.Immutable => HeapType.Default,
      ResourceUsage.Dynamic => HeapType.Upload,
      ResourceUsage.Staging => HeapType.Readback,
      _ => HeapType.Default
    };

    p_currentState = heapType switch
    {
      HeapType.Upload => ResourceStates.GenericRead,
      HeapType.Readback => ResourceStates.CopyDest,
      _ => ResourceStates.Common
    };

    var heapPops = new HeapProperties
    {
      Type = heapType,
      CPUPageProperty = CpuPageProperty.Unknown,
      MemoryPoolPreference = MemoryPool.Unknown,
      CreationNodeMask = 1,
      VisibleNodeMask = 1,
    };

    var resourceDesc = new ResourceDesc
    {
      Dimension = ResourceDimension.Buffer,
      Alignment = 0,
      Width = p_description.Size,
      Height = 1,
      DepthOrArraySize = 1,
      MipLevels = 1,
      Format = Format.FormatUnknown,
      SampleDesc = new SampleDesc { Count = 1, Quality = 0 },
      Layout = TextureLayout.LayoutRowMajor,
      Flags = ConvertToResourceFlags()
    };

    HResult hr = p_device.CreateCommittedResource(
        ref heapPops,
        HeapFlags.None,
        ref resourceDesc,
        p_currentState,
        null,
        out p_resource
      );

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create buffer: {hr}");

    p_gpuVirtualAddress = p_resource.GetGPUVirtualAddress();

    if(!string.IsNullOrEmpty(p_description.Name))
      SetDebugName(p_description.Name);
    
    if(heapType  == HeapType.Upload || heapType == HeapType.Readback)
    {
      var range = new Silk.NET.Direct3D12.Range { Begin = 0, End = 0 };
      void* mappedPtr = default;
      hr = p_resource.Map(0, ref range, ref mappedPtr);
      if(hr.IsSuccess)
        p_mappedData = mappedPtr;
    }
  }

  private void CreateDefaultView()
  {
    throw new NotImplementedException();
  }

  private ResourceStates DetermineResourceStates()
  {
    throw new NotImplementedException();
  }

  private ResourceFlags ConvertToResourceFlags()
  {
    var flags = ResourceFlags.None;

    if((p_description.BindFlags & BindFlags.UnorderedAccess) != 0)
      flags |= ResourceFlags.AllowUnorderedAccess;

    return flags;
  }

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(DX12Buffer));
  }

  private void SetDebugName(string _name)
  {
    var nameBytes = System.Text.Encoding.Unicode.GetBytes(_name + "\0");
    fixed(byte* pName = nameBytes)
    {
      p_resource.SetName((char*)pName);
    }
  }
}