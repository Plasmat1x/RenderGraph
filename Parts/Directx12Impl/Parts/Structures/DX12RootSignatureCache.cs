using Directx12Impl.Builders;
using Directx12Impl.Extensions;
using Directx12Impl.Parts.Utils;

using GraphicsAPI.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts.Structures;

public partial class DX12RootSignatureCache: IDisposable
{
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly D3D12 p_d3d12;
  private Dictionary<RootSignatureDesc, ComPtr<ID3D12RootSignature>> p_cache = [];
  private bool p_disposed;

  public DX12RootSignatureCache(ComPtr<ID3D12Device> _device, D3D12 _d3d12)
  {
    p_device = _device;
    p_d3d12 = _d3d12;
  }

  public unsafe ComPtr<ID3D12RootSignature> GetOrCreateFromDesc(RootSignatureDesc _desc)
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
      throw new InvalidOperationException($"Failed to serialize root signature: {hr}", hr.GetException());
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
      throw new InvalidOperationException($"Failed to create root signature: {hr}", hr.GetException());

    p_cache[_desc] = newRootSignature;
    return newRootSignature;
  }

  public ComPtr<ID3D12RootSignature> GetOrCreateFromShaderReflection(
    params DX12Shader?[] _shaders)
  {
    var builder = new DX12RootSignatureDescBuilder();

    var cbSlots = new HashSet<uint>();
    var srvSlots = new HashSet<uint>();
    var uavSlots = new HashSet<uint>();
    var samplerSlots = new HashSet<uint>();

    builder.AllowInputAssemblerInputLayout();

    foreach(var shader in _shaders.Where(_s => _s != null))
    {
      var reflection = shader.GetReflection();

      foreach(var cb in reflection.ConstantBuffers)
      {
        if(cbSlots.Add(cb.BindPoint))
        {
          builder.AddRootCBV(cb.BindPoint, 0);
        }
      }

      foreach(var srv in reflection.BoundResources)
      {
        if(srv.Type == ResourceBindingType.ShaderResource)
        {
          if(srvSlots.Add(srv.BindPoint))
          {
            builder.AddDescriptorTableSRV(srv.BindPoint, 0);
          }
        }
      }

      foreach(var uav in reflection.UnorderedAccessViews)
      {
        if(uavSlots.Add(uav.BindPoint))
        {
          builder.AddDescriptorTableUAV(uav.BindPoint, 0);
        }
      }

      foreach(var sampler in reflection.Samplers)
      {
        if(samplerSlots.Add(sampler.BindPoint))
        {
          builder.AddDescriptorTableSamplers(sampler.BindPoint, 0);
        }
      }
    }

    return GetOrCreateFromDesc1(builder.Build());
  }

  /// <summary>
  /// Создает простой root signature для базовых шейдеров без текстур
  /// </summary>
  public ComPtr<ID3D12RootSignature> GetDefaultGraphicsRootSignature0()
  {
    var desc = DX12RootSignatureLayouts.CreateBasicGraphics0();
    return GetOrCreateFromDesc1(desc);
  }

  /// <summary>
  /// Создает простой root signature для базовых шейдеров
  /// </summary>
  public ComPtr<ID3D12RootSignature> GetDefaultGraphicsRootSignature()
  {
    var desc = DX12RootSignatureLayouts.CreateBasicGraphics();
    return GetOrCreateFromDesc1(desc);
  }

  /// <summary>
  /// Создает root signature для compute шейдеров
  /// </summary>
  public ComPtr<ID3D12RootSignature> GetDefaultComputeRootSignature()
  {
    var desc = DX12RootSignatureLayouts.CreateBasicCompute();
    return GetOrCreateFromDesc1(desc);
  }

  /// <summary>
  /// Создает root signature для постобработки
  /// </summary>
  public ComPtr<ID3D12RootSignature> GetPostProcessRootSignature()
  {
    var desc = DX12RootSignatureLayouts.CreatePostProcess();
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
      Version = D3DRootSignatureVersion.Version11,
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
      throw new InvalidOperationException($"Failed to serialize root signature: {hr}", hr.GetException());
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
      throw new InvalidOperationException($"Failed to create root signature: {createHr}", createHr.GetException());

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
