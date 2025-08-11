using GraphicsAPI.Enums;
using GraphicsAPI.Reflections;
using GraphicsAPI.Reflections.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;

using System.Runtime.InteropServices;

namespace Directx12Impl.Parts;

/// <summary>
/// Провайдер рефлексии для DirectX 12 шейдеров
/// </summary>
public class DX12ShaderReflectionProvider: ShaderReflectionProviderBase
{
  private static readonly D3DCompiler s_compiler = D3DCompiler.GetApi();

  public override unsafe ShaderReflection CreateReflection(byte[] _bytecode, ShaderStage _stage)
  {
    if(_bytecode == null || _bytecode.Length == 0)
      throw new ArgumentException("Invalid bytecode");

    ComPtr<ID3D12ShaderReflection> reflector = default;

    fixed(byte* pBytecode = _bytecode)
    {
      fixed(Guid* pIID = &ID3D12ShaderReflection.Guid)
      {
        HResult hr = s_compiler.Reflect(pBytecode, (nuint)_bytecode.Length, pIID, (void**)&reflector);

        if(hr.IsFailure)
        {
          throw new InvalidOperationException($"Failed to create shader reflection: 0x{hr.Value:X8}");
        }
      }
    }

    try
    {
      var reflection = new ShaderReflection();

      ShaderDesc shaderDesc;
      reflector.GetDesc(&shaderDesc);

      reflection.Info = new ShaderInfo
      {
        Version = shaderDesc.Version,
        Creator = Marshal.PtrToStringAnsi((nint)shaderDesc.Creator),
        InstructionCount = shaderDesc.InstructionCount,
        ShaderModel = GetShaderModelString(shaderDesc.Version)
      };

      ParseConstantBuffers(reflector, shaderDesc.ConstantBuffers, reflection);
      ParseBoundResources(reflector, shaderDesc.BoundResources, reflection);

      if(_stage == ShaderStage.Vertex)
      {
        ParseInputParameters(reflector, shaderDesc.InputParameters, reflection);
      }

      ParseOutputParameters(reflector, shaderDesc.OutputParameters, reflection);

      if(_stage == ShaderStage.Compute)
      {
        uint x, y, z;
        reflector.GetThreadGroupSize(&x, &y, &z);
        reflection.ThreadGroupSize = new ThreadGroupSize { X = x, Y = y, Z = z };
      }

      return reflection;
    }
    finally
    {
      reflector.Dispose();
    }
  }

  private unsafe void ParseConstantBuffers(ComPtr<ID3D12ShaderReflection> _reflector, uint _count, ShaderReflection _reflection)
  {
    for(uint i = 0; i < _count; i++)
    {
      var cbReflection = _reflector.GetConstantBufferByIndex(i);

      ShaderBufferDesc bufferDesc;
      cbReflection->GetDesc(&bufferDesc);

      var cbInfo = new ConstantBufferInfo
      {
        Name = Marshal.PtrToStringAnsi((nint)bufferDesc.Name),
        Size = bufferDesc.Size,
        BindPoint = i,
        BindCount = 1,
        Type = ParseConstantBufferType(bufferDesc.Type),
        Variables = new List<ShaderVariableInfo>()
      };

      for(uint j = 0; j < bufferDesc.Variables; j++)
      {
        var varReflection = cbReflection->GetVariableByIndex(j);

        ShaderVariableDesc varDesc;
        varReflection->GetDesc(&varDesc);

        var varInfo = new ShaderVariableInfo
        {
          Name = Marshal.PtrToStringAnsi((nint)varDesc.Name),
          Offset = varDesc.StartOffset,
          Size = varDesc.Size,
          Type = ParseVariableType(Marshal.PtrToStringAnsi((nint)varDesc.DefaultValue))
        };

        cbInfo.Variables.Add(varInfo);
      }

      _reflection.ConstantBuffers.Add(cbInfo);
    }
  }

  private unsafe void ParseBoundResources(ComPtr<ID3D12ShaderReflection> _reflector, uint _count, ShaderReflection _reflection)
  {
    for(uint i = 0; i < _count; i++)
    {
      ShaderInputBindDesc bindDesc;
      _reflector.GetResourceBindingDesc(i, &bindDesc);

      var resourceInfo = new ResourceBindingInfo
      {
        Name = Marshal.PtrToStringAnsi((nint)bindDesc.Name),
        Type = ConvertResourceType(bindDesc.Type),
        BindPoint = bindDesc.BindPoint,
        BindCount = bindDesc.BindCount,
        Dimension = ConvertDimension(bindDesc.Dimension),
        ReturnType = ConvertReturnType(bindDesc.ReturnType)
      };

      switch(bindDesc.Type)
      {
        case D3DShaderInputType.D3DSitSampler:
          _reflection.Samplers.Add(new SamplerBindingInfo
          {
            Name = resourceInfo.Name,
            BindPoint = resourceInfo.BindPoint,
            BindCount = resourceInfo.BindCount
          });
          break;

        case D3DShaderInputType.D3DSitTexture:
        case D3DShaderInputType.D3DSitStructured:
        case D3DShaderInputType.D3DSitByteaddress:
          _reflection.BoundResources.Add(resourceInfo);
          break;

        case D3DShaderInputType.D3DSitUavRwtyped:
        case D3DShaderInputType.D3DSitUavRwstructured:
        case D3DShaderInputType.D3DSitUavRwbyteaddress:
        case D3DShaderInputType.D3DSitUavAppendStructured:
        case D3DShaderInputType.D3DSitUavConsumeStructured:
        case D3DShaderInputType.D3DSitUavRwstructuredWithCounter:
          _reflection.UnorderedAccessViews.Add(resourceInfo);
          break;
      }
    }
  }

  private unsafe void ParseInputParameters(ComPtr<ID3D12ShaderReflection> _reflector, uint _count, ShaderReflection _reflection)
  {
    for(uint i = 0; i < _count; i++)
    {
      SignatureParameterDesc paramDesc;
      _reflector.GetInputParameterDesc(i, &paramDesc);

      var inputParam = new InputParameterInfo
      {
        SemanticName = Marshal.PtrToStringAnsi((nint)paramDesc.SemanticName),
        SemanticIndex = paramDesc.SemanticIndex,
        Register = paramDesc.Register,
        SystemValueType = ConvertSystemValue(paramDesc.SystemValueType),
        ComponentType = ConvertComponentType(paramDesc.ComponentType),
        Mask = paramDesc.Mask,
        ReadWriteMask = paramDesc.ReadWriteMask
      };

      _reflection.InputParameters.Add(inputParam);
    }
  }

  private unsafe void ParseOutputParameters(ComPtr<ID3D12ShaderReflection> _reflector, uint _count, ShaderReflection _reflection)
  {
    for(uint i = 0; i < _count; i++)
    {
      SignatureParameterDesc paramDesc;
      _reflector.GetOutputParameterDesc(i, &paramDesc);

      var outputParam = new OutputParameterInfo
      {
        SemanticName = Marshal.PtrToStringAnsi((nint)paramDesc.SemanticName),
        SemanticIndex = paramDesc.SemanticIndex,
        Register = paramDesc.Register,
        SystemValueType = ConvertSystemValue(paramDesc.SystemValueType),
        ComponentType = ConvertComponentType(paramDesc.ComponentType),
        Mask = paramDesc.Mask,
        ReadWriteMask = paramDesc.ReadWriteMask
      };

      _reflection.OutputParameters.Add(outputParam);
    }
  }

  public override bool IsBytecodeSupported(byte[] _bytecode)
  {
    if(_bytecode == null || _bytecode.Length < 4)
      return false;

    return _bytecode[0] == 0x44 && _bytecode[1] == 0x58 &&
           _bytecode[2] == 0x42 && _bytecode[3] == 0x43;
  }

  public override string GetShaderModel(byte[] _bytecode)
  {
    try
    {
      var reflection = CreateReflection(_bytecode, ShaderStage.Vertex);
      return reflection.Info?.ShaderModel ?? "Unknown";
    }
    catch
    {
      return "Unknown";
    }
  }

  private string GetShaderModelString(uint _version)
  {
    var major = _version >> 8 & 0xFF;
    var minor = _version & 0xFF;
    return $"{major}.{minor}";
  }

  private ConstantBufferType ParseConstantBufferType(D3DCBufferType _type)
  {
    return _type switch
    {
      D3DCBufferType.D3DCTCbuffer => ConstantBufferType.ConstantBuffer,
      D3DCBufferType.D3DCTTbuffer => ConstantBufferType.TextureBuffer,
      D3DCBufferType.D3DCTInterfacePointers => ConstantBufferType.InterfacePointers,
      D3DCBufferType.D3DCTResourceBindInfo => ConstantBufferType.ResourceBindInfo,
      _ => ConstantBufferType.ConstantBuffer
    };
  }

  private ResourceBindingType ConvertResourceType(D3DShaderInputType _type)
  {
    return _type switch
    {
      D3DShaderInputType.D3DSitCbuffer => ResourceBindingType.ConstantBuffer,
      D3DShaderInputType.D3DSitTbuffer => ResourceBindingType.TextureBuffer,
      D3DShaderInputType.D3DSitTexture => ResourceBindingType.ShaderResource,
      D3DShaderInputType.D3DSitSampler => ResourceBindingType.Sampler,
      D3DShaderInputType.D3DSitUavRwtyped => ResourceBindingType.UnorderedAccess,
      D3DShaderInputType.D3DSitStructured => ResourceBindingType.StructuredBuffer,
      D3DShaderInputType.D3DSitUavRwstructured => ResourceBindingType.RWStructuredBuffer,
      D3DShaderInputType.D3DSitByteaddress => ResourceBindingType.ByteAddressBuffer,
      D3DShaderInputType.D3DSitUavRwbyteaddress => ResourceBindingType.RWByteAddressBuffer,
      D3DShaderInputType.D3DSitUavAppendStructured => ResourceBindingType.AppendStructuredBuffer,
      D3DShaderInputType.D3DSitUavConsumeStructured => ResourceBindingType.ConsumeStructuredBuffer,
      D3DShaderInputType.D3DSitUavRwstructuredWithCounter => ResourceBindingType.RWStructuredBufferWithCounter,
      _ => ResourceBindingType.Unknown
    };
  }

  private GraphicsAPI.Enums.ResourceDimension ConvertDimension(D3DSrvDimension _dimension)
  {
    return _dimension switch
    {
      D3DSrvDimension.D3DSrvDimensionUnknown => GraphicsAPI.Enums.ResourceDimension.Unknown,
      D3DSrvDimension.D3DSrvDimensionBuffer => GraphicsAPI.Enums.ResourceDimension.Buffer,
      D3DSrvDimension.D3DSrvDimensionTexture1D => GraphicsAPI.Enums.ResourceDimension.Texture1D,
      D3DSrvDimension.D3DSrvDimensionTexture1Darray => GraphicsAPI.Enums.ResourceDimension.Texture1DArray,
      D3DSrvDimension.D3DSrvDimensionTexture2D => GraphicsAPI.Enums.ResourceDimension.Texture2D,
      D3DSrvDimension.D3DSrvDimensionTexture2Darray => GraphicsAPI.Enums.ResourceDimension.Texture2DArray,
      D3DSrvDimension.D3DSrvDimensionTexture2Dms => GraphicsAPI.Enums.ResourceDimension.Texture2D,
      D3DSrvDimension.D3DSrvDimensionTexture2Dmsarray => GraphicsAPI.Enums.ResourceDimension.Texture2DArray,
      D3DSrvDimension.D3DSrvDimensionTexture3D => GraphicsAPI.Enums.ResourceDimension.Texture3D,
      D3DSrvDimension.D3DSrvDimensionTexturecube => GraphicsAPI.Enums.ResourceDimension.TextureCube,
      D3DSrvDimension.D3DSrvDimensionTexturecubearray => GraphicsAPI.Enums.ResourceDimension.TextureCubeArray,
      D3DSrvDimension.D3DSrvDimensionBufferex => GraphicsAPI.Enums.ResourceDimension.BufferEx,
      _ => GraphicsAPI.Enums.ResourceDimension.Unknown
    };
  }

  private ResourceReturnType ConvertReturnType(D3DResourceReturnType _type)
  {
    return _type switch
    {
      D3DResourceReturnType.D3DReturnTypeUnorm => ResourceReturnType.UNorm,
      D3DResourceReturnType.D3DReturnTypeSNorm => ResourceReturnType.SNorm,
      D3DResourceReturnType.D3DReturnTypeSint => ResourceReturnType.SInt,
      D3DResourceReturnType.D3DReturnTypeUint => ResourceReturnType.UInt,
      D3DResourceReturnType.D3DReturnTypeFloat => ResourceReturnType.Float,
      D3DResourceReturnType.D3DReturnTypeMixed => ResourceReturnType.Mixed,
      D3DResourceReturnType.D3DReturnTypeDouble => ResourceReturnType.Double,
      D3DResourceReturnType.D3DReturnTypeContinued => ResourceReturnType.Continued,
      _ => ResourceReturnType.Unknown
    };
  }

  private SystemValueType ConvertSystemValue(D3DName _systemValue)
  {
    return _systemValue switch
    {
      D3DName.D3DNameUndefined => SystemValueType.Undefined,
      D3DName.D3DNamePosition => SystemValueType.Position,
      D3DName.D3DNameClipDistance => SystemValueType.ClipDistance,
      D3DName.D3DNameCullDistance => SystemValueType.CullDistance,
      D3DName.D3DNameRenderTargetArrayIndex => SystemValueType.RenderTargetArrayIndex,
      D3DName.D3DNameViewportArrayIndex => SystemValueType.ViewportArrayIndex,
      D3DName.D3DNameVertexID => SystemValueType.VertexID,
      D3DName.D3DNamePrimitiveID => SystemValueType.PrimitiveID,
      D3DName.D3DNameInstanceID => SystemValueType.InstanceID,
      D3DName.D3DNameIsFrontFace => SystemValueType.IsFrontFace,
      D3DName.D3DNameSampleIndex => SystemValueType.SampleIndex,
      D3DName.D3DNameFinalQuadEdgeTessfactor => SystemValueType.FinalQuadEdgeTessFactor,
      D3DName.D3DNameFinalQuadInsideTessfactor => SystemValueType.FinalQuadInsideTessFactor,
      D3DName.D3DNameFinalTriEdgeTessfactor => SystemValueType.FinalTriEdgeTessFactor,
      D3DName.D3DNameFinalTriInsideTessfactor => SystemValueType.FinalTriInsideTessFactor,
      D3DName.D3DNameFinalLineDetailTessfactor => SystemValueType.FinalLineDetailTessFactor,
      D3DName.D3DNameFinalLineDensityTessfactor => SystemValueType.FinalLineDensityTessFactor,
      D3DName.D3DNameTarget => SystemValueType.Target,
      D3DName.D3DNameDepth => SystemValueType.Depth,
      D3DName.D3DNameCoverage => SystemValueType.Coverage,
      D3DName.D3DNameDepthGreaterEqual => SystemValueType.DepthGreaterEqual,
      D3DName.D3DNameDepthLessEqual => SystemValueType.DepthLessEqual,
      D3DName.D3DNameStencilRef => SystemValueType.StencilRef,
      D3DName.D3DNameInnerCoverage => SystemValueType.InnerCoverage,
      _ => SystemValueType.Undefined
    };
  }

  private static RegisterComponentType ConvertComponentType(D3DRegisterComponentType _type)
  {
    return _type switch
    {
      D3DRegisterComponentType.D3DRegisterComponentUnknown => RegisterComponentType.Unknown,
      D3DRegisterComponentType.D3DRegisterComponentUint32 => RegisterComponentType.UInt32,
      D3DRegisterComponentType.D3DRegisterComponentSint32 => RegisterComponentType.SInt32,
      D3DRegisterComponentType.D3DRegisterComponentFloat32 => RegisterComponentType.Float32,
      _ => RegisterComponentType.Unknown
    };
  }
}