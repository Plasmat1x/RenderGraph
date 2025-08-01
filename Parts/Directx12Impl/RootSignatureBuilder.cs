using Silk.NET.Direct3D12;

namespace Directx12Impl;

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
