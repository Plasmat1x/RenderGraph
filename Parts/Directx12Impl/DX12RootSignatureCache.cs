using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;

namespace Directx12Impl;

public class DX12RootSignatureCache: IDisposable
{
  private class RootSignatureDescComparer: IEqualityComparer<RootSignatureDesc>
  {
    public unsafe bool Equals(RootSignatureDesc _x, RootSignatureDesc _y)
    {
      return _x.NumParameters == _y.NumParameters &&
             _x.NumStaticSamplers == _y.NumStaticSamplers &&
             _x.Flags == _y.Flags &&
             _x.PStaticSamplers == _y.PStaticSamplers &&
             _x.PParameters == _y.PParameters;
    }

    public int GetHashCode(RootSignatureDesc _obj)
    {
      return HashCode.Combine(
          _obj.NumParameters,
          _obj.NumStaticSamplers,
          _obj.Flags);
    }
  }

  private readonly ComPtr<ID3D12Device> p_device;
  private readonly D3D12 p_d3d12;
  private Dictionary<RootSignatureDesc, ComPtr<ID3D12RootSignature>> p_cache = [];
  private bool p_disposed;

  public DX12RootSignatureCache(ComPtr<ID3D12Device> _device, D3D12 _d3d12) 
  { 
    p_device = _device;
    p_d3d12 = _d3d12;
  }

  public unsafe ComPtr<ID3D12RootSignature> GetOrCreate(RootSignatureDesc _desc)
  {
    if(p_cache.TryGetValue(_desc, out var rootSignature))
      return rootSignature;

    ID3D10Blob* signature = null;
    ID3D10Blob* error = null;

    HResult hr = p_d3d12.SerializeRootSignature(
        &_desc,
        D3DRootSignatureVersion.Version10,
        &signature,
        &error);

    if(hr.IsFailure)
    {
      if(error != null)
      {
        var errorMessage = SilkMarshal.PtrToString((nint)error->GetBufferPointer());
        error->Release();
        throw new InvalidOperationException($"Failed to serialize root signature: {errorMessage}");
      }
      throw new InvalidOperationException($"Failed to serialize root signature: {hr}");
    }

    ID3D12RootSignature* newRootSignature;
    hr = p_device.CreateRootSignature(
        0,
        signature->GetBufferPointer(),
        signature->GetBufferSize(),
        SilkMarshal.GuidPtrOf<ID3D12RootSignature>(),
        (void**)&newRootSignature);

    signature->Release();
    if(error != null)
      error->Release();

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create root signature: {hr}");

    p_cache[_desc] = newRootSignature;
    return newRootSignature;
  }

  /// <summary>
  /// Создает простой root signature для базовых шейдеров
  /// </summary>
  public ComPtr<ID3D12RootSignature> GetDefaultGraphicsRootSignature()
  {
    var desc = RootSignatureLayouts.CreateBasicGraphics();
    return GetOrCreateFromDesc1(desc);
  }

  /// <summary>
  /// Создает root signature для compute шейдеров
  /// </summary>
  public ComPtr<ID3D12RootSignature> GetDefaultComputeRootSignature()
  {
    var desc = RootSignatureLayouts.CreateBasicCompute();
    return GetOrCreateFromDesc1(desc);
  }

  /// <summary>
  /// Создает root signature для постобработки
  /// </summary>
  public ComPtr<ID3D12RootSignature> GetPostProcessRootSignature()
  {
    var desc = RootSignatureLayouts.CreatePostProcess();
    return GetOrCreateFromDesc1(desc);
  }

  /// <summary>
  /// Получает или создает root signature из RootSignatureDesc1
  /// </summary>
  public unsafe ComPtr<ID3D12RootSignature> GetOrCreateFromDesc1(RootSignatureDesc1 _desc)
  {
    var key = new RootSignatureDesc
    {
      NumParameters = _desc.NumParameters,
      NumStaticSamplers = _desc.NumStaticSamplers,
      Flags = _desc.Flags
    };

    if(p_cache.TryGetValue(key, out var rootSignature))
      return rootSignature;

    var versionedDesc = new VersionedRootSignatureDesc
    {
      Version = D3DRootSignatureVersion.Version11
    };
    versionedDesc.Anonymous.Desc11 = _desc;

    ID3D10Blob* signature = null;
    ID3D10Blob* error = null;

    HResult hr = p_d3d12.SerializeVersionedRootSignature(
        &versionedDesc,
        &signature,
        &error);

    if(hr.IsFailure)
    {
      if(error != null)
      {
        var errorMessage = SilkMarshal.PtrToString((nint)error->GetBufferPointer());
        error->Release();
        throw new InvalidOperationException($"Failed to serialize root signature: {errorMessage}");
      }
      throw new InvalidOperationException($"Failed to serialize root signature: {hr}");
    }

    ID3D12RootSignature* newRootSignature;
    HResult createHr = p_device.CreateRootSignature(
        0,
        signature->GetBufferPointer(),
        signature->GetBufferSize(),
        SilkMarshal.GuidPtrOf<ID3D12RootSignature>(),
        (void**)&newRootSignature);

    signature->Release();
    if(error != null)
      error->Release();

    if(createHr.IsFailure)
      throw new InvalidOperationException($"Failed to create root signature: {createHr}");

    p_cache[key] = newRootSignature;
    return newRootSignature;
  }

  public void Clear()
  {
    foreach(var kvp in p_cache)
    {
      kvp.Value.Dispose();
    }
    p_cache.Clear();
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    Clear();
    p_disposed = true;
  }
}
