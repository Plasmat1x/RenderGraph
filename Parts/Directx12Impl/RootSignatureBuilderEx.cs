using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;
public unsafe class RootSignatureBuilderEx : IDisposable
{
  private readonly List<RootParameter1> p_parameters = [];
  private readonly List<StaticSamplerDesc> p_staticSamplers = [];

  private RootSignatureFlags p_flags = RootSignatureFlags.None;

  private readonly HashSet<(ShaderVisibility, uint)> p_usedCbvSlots = [];
  private readonly HashSet<(ShaderVisibility, uint)> p_usedSrvSlots = [];
  private readonly HashSet<(ShaderVisibility, uint)> p_usedUavSlots = [];
  private readonly HashSet<(ShaderVisibility, uint)> p_usedSamplerSlots = [];

  public RootSignatureBuilderEx AllowInputAssemblerInputLayout()
  {
    p_flags |= RootSignatureFlags.AllowInputAssemblerInputLayout;
    return this;
  }

  public RootSignatureBuilderEx AllowStreamOutput()
  {
    p_flags |= RootSignatureFlags.AllowStreamOutput;
    return this;
  }

  public RootSignatureBuilderEx AddRootCBV(
    uint _shaderRegister, 
    uint _registerSpace = 0, 
    ShaderVisibility _visibility = ShaderVisibility.All)
  {
    var key = (_visibility, _shaderRegister);
    if(p_usedCbvSlots.Contains(key))
      throw new InvalidOperationException($"CBV slot b{_shaderRegister} already used for visibility {_visibility}");

    p_usedCbvSlots.Add(key);

    var parameter = new RootParameter1
    {
      ParameterType = RootParameterType.Type32BitConstants,
      ShaderVisibility = _visibility,
    };

    parameter.Anonymous.Descriptor = new RootDescriptor1
    {
      ShaderRegister = _shaderRegister,
      RegisterSpace = _registerSpace,
      Flags = RootDescriptorFlags.DataStaticWhileSetATExecute,
    };

    p_parameters.Add(parameter);

    return this;
  }

  public RootSignatureBuilderEx AddRoot32BitConstants(
    uint _num32BitValues,
    uint _shaderRegister,
    uint _registerSpace = 0,
    ShaderVisibility _visibility = ShaderVisibility.All)
  {
    var parameter = new RootParameter1
    {
      ParameterType = RootParameterType.Type32BitConstants,
      ShaderVisibility = _visibility
    };
    parameter.Anonymous.Constants = new RootConstants
    {
      ShaderRegister = _shaderRegister,
      RegisterSpace = _registerSpace,
      Num32BitValues = _num32BitValues
    };

    p_parameters.Add(parameter);
    return this;
  }

  public RootSignatureBuilderEx AddDescriptorTableSRV(
    uint _baseRegister,
    uint _descriptorCount,
    uint _registerSpace = 0,
    ShaderVisibility _visibility = ShaderVisibility.Pixel)
  {
    var ranges = new DescriptorRange1[]
    {
      new DescriptorRange1
      {
        RangeType = DescriptorRangeType.Srv,
        NumDescriptors = _descriptorCount,
        BaseShaderRegister = _baseRegister,
        RegisterSpace = _registerSpace,
        Flags = DescriptorRangeFlags.DataStaticWhileSetATExecute,
        OffsetInDescriptorsFromTableStart = 0
      }
    };

    return AddDescriptorTable(ranges, _visibility);
  }

  public RootSignatureBuilderEx AddDescriptorTableUAV(
    uint _baseRegister,
    uint _descriptorCount,
    uint _registerSpace = 0,
    ShaderVisibility _visibility = ShaderVisibility.All)
  {
    var ranges = new DescriptorRange1[]
    {
      new DescriptorRange1
      {
        RangeType = DescriptorRangeType.Uav,
        NumDescriptors = _descriptorCount,
        BaseShaderRegister = _baseRegister,
        RegisterSpace = _registerSpace,
        Flags = DescriptorRangeFlags.DataVolatile,
        OffsetInDescriptorsFromTableStart = 0
      }
    };

    return AddDescriptorTable(ranges, _visibility);
  }

  public RootSignatureBuilderEx AddDescriptorTableSamplers(
    uint _baseRegister,
    uint _descriptorCount,
    uint _registerSpace = 0,
    ShaderVisibility _visibility = ShaderVisibility.Pixel)
  {
    var ranges = new DescriptorRange1[]
    {
      new DescriptorRange1
      {
        RangeType = DescriptorRangeType.Sampler,
        NumDescriptors = _descriptorCount,
        BaseShaderRegister = _baseRegister,
        RegisterSpace = _registerSpace,
        Flags = DescriptorRangeFlags.None,
        OffsetInDescriptorsFromTableStart = 0
      }
    };

    return AddDescriptorTable(ranges, _visibility);
  }

  public RootSignatureBuilderEx AddDescriptorTable(
    DescriptorRange1[] _ranges,
    ShaderVisibility _visibility = ShaderVisibility.All)
  {
    var parameter = new RootParameter1
    {
      ParameterType = RootParameterType.TypeDescriptorTable,
      ShaderVisibility = _visibility
    };

    var rangesPtr = (DescriptorRange1*)SilkMarshal.Allocate(_ranges.Length);

    parameter.Anonymous.DescriptorTable = new RootDescriptorTable1
    {
      NumDescriptorRanges = (uint)_ranges.Length,
      PDescriptorRanges = rangesPtr
    };

    p_parameters.Add(parameter);
    return this;
  }

  public RootSignatureBuilderEx AddStaticSampler(
    uint _shaderRegister,
    Filter _filter = Filter.MinMagMipLinear,
    TextureAddressMode _addressMode = TextureAddressMode.Wrap,
    uint _registerSpace = 0,
    ShaderVisibility _visibility = ShaderVisibility.Pixel)
  {
    var sampler = new StaticSamplerDesc
    {
      Filter = _filter,
      AddressU = _addressMode,
      AddressV = _addressMode,
      AddressW = _addressMode,
      MipLODBias = 0,
      MaxAnisotropy = 0,
      ComparisonFunc = ComparisonFunc.Never,
      BorderColor = StaticBorderColor.TransparentBlack,
      MinLOD = 0.0f,
      MaxLOD = float.MaxValue,
      ShaderRegister = _shaderRegister,
      RegisterSpace = _registerSpace,
      ShaderVisibility = _visibility
    };

    p_staticSamplers.Add(sampler);
    return this;
  }

  public RootSignatureDesc1 Build()
  {
    var parametersArray = p_parameters.ToArray();
    var staticSamplersArray = p_staticSamplers.ToArray();

    fixed(RootParameter1* pParams = parametersArray)
    fixed(StaticSamplerDesc* pSamplers = staticSamplersArray)
    {
      return new RootSignatureDesc1
      {
        NumParameters = (uint)p_parameters.Count,
        PParameters = pParams,
        NumStaticSamplers = (uint)p_staticSamplers.Count,
        PStaticSamplers = pSamplers,
        Flags = p_flags
      };
    }
  }

  public ComPtr<ID3D10Blob> BuildSerialized(D3D12 _d3d12, out ID3D10Blob* _errorBlob)
  {
    _errorBlob = null;
    var desc = Build();

    var versionedDesc = new VersionedRootSignatureDesc
    {
      Version = D3DRootSignatureVersion.Version11
    };
    versionedDesc.Anonymous.Desc11 = desc;

    ID3D10Blob* signature;
    ID3D10Blob* errorBlob = _errorBlob;
    HResult hr = _d3d12.SerializeVersionedRootSignature(
      &versionedDesc,
      &signature,
      &errorBlob);

    if(hr.IsFailure)
      return null;

    return signature;
  }

  public void Dispose()
  {
    foreach(var param in p_parameters)
      if(param.ParameterType == RootParameterType.TypeDescriptorTable)
        if(param.Anonymous.DescriptorTable.PDescriptorRanges != null)
          SilkMarshal.Free((nint)param.Anonymous.DescriptorTable.PDescriptorRanges);
  }
}

public static class RootSignatureLayouts
{
  /// <summary>
  /// Простой макет для базового рендеринга
  /// </summary>
  public static RootSignatureDesc1 CreateBasicGraphics()
  {
    using var builder = new RootSignatureBuilderEx()
        .AllowInputAssemblerInputLayout()
        // b0 - View/Projection constants
        .AddRootCBV(0, 0, ShaderVisibility.Vertex)
        // b1 - Per-object constants  
        .AddRootCBV(1, 0, ShaderVisibility.All)
        // t0-t7 - Textures
        .AddDescriptorTableSRV(0, 8, 0, ShaderVisibility.Pixel)
        // s0-s7 - Samplers
        .AddDescriptorTableSamplers(0, 8, 0, ShaderVisibility.Pixel);

    return builder.Build();
  }

  /// <summary>
  /// Макет для compute шейдеров
  /// </summary>
  public static RootSignatureDesc1 CreateBasicCompute()
  {
    using var builder = new RootSignatureBuilderEx()
        // b0 - Compute constants
        .AddRootCBV(0, 0)
        // t0-t7 - Input textures/buffers
        .AddDescriptorTableSRV(0, 8)
        // u0-u7 - Output textures/buffers
        .AddDescriptorTableUAV(0, 8);

    return builder.Build();
  }

  /// <summary>
  /// Макет для постобработки (fullscreen effects)
  /// </summary>
  public static RootSignatureDesc1 CreatePostProcess()
  {
    using var builder = new RootSignatureBuilderEx()
        // 16 32-bit constants directly in root signature
        .AddRoot32BitConstants(16, 0, 0, ShaderVisibility.Pixel)
        // t0 - Input texture
        .AddDescriptorTableSRV(0, 1, 0, ShaderVisibility.Pixel)
        // Static sampler for point sampling
        .AddStaticSampler(0, Filter.MinMagMipPoint)
        // Static sampler for linear sampling
        .AddStaticSampler(1, Filter.MinMagMipLinear);

    return builder.Build();
  }
}