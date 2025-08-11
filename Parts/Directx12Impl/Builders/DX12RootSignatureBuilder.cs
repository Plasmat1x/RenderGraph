using Directx12Impl.Extensions;

using GraphicsAPI.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

using System.Runtime.InteropServices;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Directx12Impl.Builders;

/// <summary>
/// Обновление Root Signature для автоматической генерации из рефлексии шейдеров
/// </summary>
public unsafe class DX12RootSignatureBuilder
{
  private readonly List<RootParameter> p_parameters = new();
  private readonly List<StaticSamplerDesc> p_staticSamplers = new();
  private RootSignatureFlags p_flags = RootSignatureFlags.AllowInputAssemblerInputLayout;

  public static unsafe ID3D12RootSignature* CreateFromShaderReflection(
      ID3D12Device* _device,
      params DX12Shader[] _shaders)
  {
    var builder = new DX12RootSignatureBuilder();

    var cbSlots = new HashSet<uint>();
    var srvSlots = new HashSet<uint>();
    var uavSlots = new HashSet<uint>();
    var samplerSlots = new HashSet<uint>();

    foreach(var shader in _shaders.Where(s => s != null))
    {
      var reflection = shader.GetReflection();

      foreach(var cb in reflection.ConstantBuffers)
      {
        if(cbSlots.Add(cb.BindPoint))
        {
          builder.AddConstantBufferView(cb.BindPoint, 0);
        }
      }

      foreach(var srv in reflection.BoundResources)
      {
        if(srv.Type == ResourceBindingType.ShaderResource)
        {
          if(srvSlots.Add(srv.BindPoint))
          {
            builder.AddShaderResourceView(srv.BindPoint, 0);
          }
        }
      }

      foreach(var uav in reflection.UnorderedAccessViews)
      {
        if(uavSlots.Add(uav.BindPoint))
        {
          builder.AddUnorderedAccessView(uav.BindPoint, 0);
        }
      }

      foreach(var sampler in reflection.Samplers)
      {
        if(samplerSlots.Add(sampler.BindPoint))
        {
          builder.AddSampler(sampler.BindPoint, 0);
        }
      }
    }

    return builder.Build(_device);
  }

  public void AddConstantBufferView(uint _shaderRegister, uint _registerSpace)
  {
    p_parameters.Add(new RootParameter
    {
      ParameterType = RootParameterType.TypeDescriptorTable,
      ShaderVisibility = ShaderVisibility.All,
      Anonymous = new RootParameterUnion
      {
        DescriptorTable = new RootDescriptorTable
        {
          NumDescriptorRanges = 1,
          PDescriptorRanges = CreateDescriptorRange(DescriptorRangeType.Cbv, 1, _shaderRegister, _registerSpace)
        }
      }
    });
  }

  public void AddShaderResourceView(uint _shaderRegister, uint _registerSpace)
  {
    p_parameters.Add(new RootParameter
    {
      ParameterType = RootParameterType.TypeDescriptorTable,
      ShaderVisibility = ShaderVisibility.All,
      Anonymous = new RootParameterUnion
      {
        DescriptorTable = new RootDescriptorTable
        {
          NumDescriptorRanges = 1,
          PDescriptorRanges = CreateDescriptorRange(DescriptorRangeType.Srv, 1, _shaderRegister, _registerSpace)
        }
      }
    });
  }

  public void AddUnorderedAccessView(uint _shaderRegister, uint _registerSpace)
  {
    p_parameters.Add(new RootParameter
    {
      ParameterType = RootParameterType.TypeDescriptorTable,
      ShaderVisibility = ShaderVisibility.All,
      Anonymous = new RootParameterUnion
      {
        DescriptorTable = new RootDescriptorTable
        {
          NumDescriptorRanges = 1,
          PDescriptorRanges = CreateDescriptorRange(DescriptorRangeType.Uav, 1, _shaderRegister, _registerSpace)
        }
      }
    });
  }

  public void AddSampler(uint _shaderRegister, uint _registerSpace)
  {
    p_parameters.Add(new RootParameter
    {
      ParameterType = RootParameterType.TypeDescriptorTable,
      ShaderVisibility = ShaderVisibility.All,
      Anonymous = new RootParameterUnion
      {
        DescriptorTable = new RootDescriptorTable
        {
          NumDescriptorRanges = 1,
          PDescriptorRanges = CreateDescriptorRange(DescriptorRangeType.Sampler, 1, _shaderRegister, _registerSpace)
        }
      }
    });
  }

  private DescriptorRange* CreateDescriptorRange(DescriptorRangeType _type, uint _count, uint _baseRegister, uint _registerSpace)
  {
    var range = (DescriptorRange*)Marshal.AllocHGlobal(sizeof(DescriptorRange));
    *range = new DescriptorRange
    {
      RangeType = _type,
      NumDescriptors = _count,
      BaseShaderRegister = _baseRegister,
      RegisterSpace = _registerSpace,
      OffsetInDescriptorsFromTableStart = uint.MaxValue // D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND
    };
    return range;
  }

  public ID3D12RootSignature* Build(ComPtr<ID3D12Device> _device)
  {
    ID3D10Blob* signature = null;
    ID3D10Blob* error = null;

    ID3D12RootSignature* newRootSignature;
    HResult createHr = _device.CreateRootSignature(
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

    return newRootSignature;
  }
}
