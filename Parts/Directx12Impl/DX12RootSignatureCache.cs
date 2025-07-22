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

  public ComPtr<ID3D12RootSignature> GetDefaultGraphicsRootSignature()
  {
    var desc = new RootSignatureDesc
    {
      NumParameters = 0,
      PParameters = null,
      NumStaticSamplers = 0,
      PStaticSamplers = null,
      Flags = RootSignatureFlags.AllowInputAssemblerInputLayout
    };

    return GetOrCreate(desc);
  }

  public ComPtr<ID3D12RootSignature> GetDefaultComputeRootSignature()
  {
    var desc = new RootSignatureDesc
    {
      NumParameters = 0,
      PParameters = null,
      NumStaticSamplers = 0,
      PStaticSamplers = null,
      Flags = RootSignatureFlags.None
    };

    return GetOrCreate(desc);
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

public class RootSignatureBuilder
{
  private readonly List<RootParameter> p_parameters = [];
  private readonly List<StaticSamplerDesc> p_staticSamplers = [];
  private RootSignatureFlags p_flags = RootSignatureFlags.None;

  public RootSignatureBuilder AllowInputAssemblerInputLayout()
  {
    p_flags |= RootSignatureFlags.AllowInputAssemblerInputLayout;
    return this;
  }

  public RootSignatureBuilder AddConstantBufferView(uint _shaderRegister, uint _registerSpace = 0)
  {
    var parameter = new RootParameter
    {
      ParameterType = RootParameterType.TypeCbv,
      ShaderVisibility = ShaderVisibility.All,
    };
    parameter.Anonymous.Descriptor = new RootDescriptor
    {
      ShaderRegister = _shaderRegister,
      RegisterSpace = _registerSpace,
    };

    p_parameters.Add(parameter);
    return this;
  }

  public RootSignatureBuilder AddShaderResourceView(uint _shaderRegister, uint _registerSpace = 0)
  {
    var parameter = new RootParameter
    {
      ParameterType = RootParameterType.TypeSrv,
      ShaderVisibility = ShaderVisibility.All
    };
    parameter.Anonymous.Descriptor = new RootDescriptor
    {
      ShaderRegister = _shaderRegister,
      RegisterSpace = _registerSpace
    };

    p_parameters.Add(parameter);
    return this;
  }

  public RootSignatureBuilder AddUnorderedAccessView(uint _shaderRegister, uint _registerSpace = 0)
  {
    var parameter = new RootParameter
    {
      ParameterType = RootParameterType.TypeUav,
      ShaderVisibility = ShaderVisibility.All
    };
    parameter.Anonymous.Descriptor = new RootDescriptor
    {
      ShaderRegister = _shaderRegister,
      RegisterSpace = _registerSpace
    };

    p_parameters.Add(parameter);
    return this;
  }

  public unsafe RootSignatureDesc Build()
  {
    fixed(RootParameter* pParams = p_parameters.ToArray())
    fixed(StaticSamplerDesc* pSamplers = p_staticSamplers.ToArray())
    {
      return new RootSignatureDesc
      {
        NumParameters = (uint)p_parameters.Count,
        PParameters = pParams,
        NumStaticSamplers = (uint)p_staticSamplers.Count,
        PStaticSamplers = pSamplers,
        Flags = p_flags
      };
    }
  }
}
