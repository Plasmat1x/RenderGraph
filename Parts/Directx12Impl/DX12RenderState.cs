using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Directx12Impl;
public class DX12RenderState: IRenderState
{
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly D3D12 p_d3d12;
  private ComPtr<ID3D12PipelineState> p_pipelineState;
  private ComPtr<ID3D12RootSignature> p_rootSignature;
  private RenderStateDescription p_description;
  private bool p_disposed;

  public DX12RenderState(
    ComPtr<ID3D12Device> _device,
    D3D12 _d3d12,
    RenderStateDescription _desc)
  {
    p_description = _desc;
    p_d3d12 = _d3d12;

    CreatePipelineState();
  }

  public RenderStateDescription Description => p_description;

  public string Name => p_description.Name;

  public ResourceType ResourceType => ResourceType.RenderState;

  public bool IsDisposed => p_disposed;

  public ComPtr<ID3D12PipelineState> GetPipelineState()
  {
    ThrowIfDisposed();
    return p_pipelineState;
  }

  public ComPtr<ID3D12RootSignature> GetRootSignature()
  {
    ThrowIfDisposed();
    return p_rootSignature;
  }

  public unsafe IntPtr GetNativeHandle()
  {
    ThrowIfDisposed();
    return (IntPtr)p_pipelineState.Handle;
  }

  public ulong GetMemorySize()
  {
    // PSO size is not directly queryable
    return 0;
  }

  public unsafe void Dispose()
  {
    if(p_disposed)
      return;

    if(p_pipelineState.Handle != null)
      p_pipelineState.Dispose();


    if(p_rootSignature.Handle != null)
      p_rootSignature.Dispose();

    p_disposed = true;
    GC.SuppressFinalize(this);
  }

  private unsafe void CreatePipelineState()
  {
    CreateDefaultRootSignature();

    if(p_description.PipelineState.ComputeShader != null)
      CreateComputePipelineState();
    else
      CreateGraphicsPipelineState();
  }

  private unsafe void CreateGraphicsPipelineState()
  {
    var vs = p_description.PipelineState.VertexShader as DX12Shader;
    var ps = p_description.PipelineState.PixelShader as DX12Shader;
    var ds = p_description.PipelineState.DomainShader as DX12Shader;
    var hs = p_description.PipelineState.HullShader as DX12Shader;
    var gs = p_description.PipelineState.GeometryShader as DX12Shader;

    if(vs == null)
      throw new ArgumentException("Vertex shader is required for graphics pipeline");

    var inputElements = CreateInputLayout(p_description.PipelineState.InputLayout);

    var psoDesc = new GraphicsPipelineStateDesc
    {
      PRootSignature = p_rootSignature,
      VS = vs.GetD3D12Bytecode(),
      PS = ps?.GetD3D12Bytecode() ?? default,
      DS = ds?.GetD3D12Bytecode() ?? default,
      HS = hs?.GetD3D12Bytecode() ?? default,
      GS = gs?.GetD3D12Bytecode() ?? default,
      StreamOutput = default, // TODO: Stream output support
      BlendState = ConvertBlendState(p_description.BlendState),
      SampleMask = p_description.PipelineState.SampleMask,
      RasterizerState = ConvertRasterizerState(p_description.RasterizerState),
      DepthStencilState = ConvertDepthStencilState(p_description.DepthStencilState),
      InputLayout = new InputLayoutDesc
      {
        PInputElementDescs = inputElements.Length > 0 ? (InputElementDesc*)SilkMarshal.Allocate(inputElements.Length) : null,
        NumElements = (uint)inputElements.Length
      },
      IBStripCutValue = IndexBufferStripCutValue.ValueDisabled,
      PrimitiveTopologyType = ConvertPrimitiveTopologyType(p_description.PipelineState.PrimitiveTopology),
      NumRenderTargets = p_description.PipelineState.RenderTargetCount,
      DSVFormat = ConvertFormat(p_description.PipelineState.DepthStencilFormat),
      SampleDesc = new SampleDesc
      {
        Count = p_description.PipelineState.SampleCount,
        Quality = p_description.PipelineState.SampleQuality
      },
      NodeMask = 0,
      Flags = PipelineStateFlags.None
    };

    for(int i = 0; i < p_description.PipelineState.RenderTargetCount; i++)
    {
      psoDesc.RTVFormats[i] = ConvertFormat(p_description.PipelineState.RenderTargetFormats[i]);
    }

    HResult hr = p_device.CreateGraphicsPipelineState(
        ref psoDesc,
        out p_pipelineState);

    if(psoDesc.InputLayout.PInputElementDescs != null)
    {
      SilkMarshal.Free((nint)psoDesc.InputLayout.PInputElementDescs);
    }

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create graphics pipeline state: {hr}");
  }

  private void CreateComputePipelineState()
  {
    var cs = p_description.PipelineState.ComputeShader as DX12Shader;

    if(cs == null)
      throw new ArgumentException("Compute shader is required for compute pipeline");

    var psoDesc = new ComputePipelineStateDesc
    {
      PRootSignature = p_rootSignature,
      CS = cs.GetD3D12Bytecode(),
      NodeMask = 0,
      Flags = PipelineStateFlags.None
    };

    HResult hr = p_device.CreateComputePipelineState(
        ref psoDesc,
        out p_pipelineState);

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create compute pipeline state: {hr}");
  }

  private unsafe void CreateDefaultRootSignature()
  {
    var rootSignatureDesc = new RootSignatureDesc
    {
      NumParameters = 0,
      PParameters = null,
      NumStaticSamplers = 0,
      PStaticSamplers = null,
      Flags = RootSignatureFlags.AllowInputAssemblerInputLayout
    };

    ID3D10Blob* signature = null;
    ID3D10Blob* error = null;

    HResult hr = p_d3d12.SerializeRootSignature(
        &rootSignatureDesc,
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

    ID3D12RootSignature* rootSig;
    hr = p_device.CreateRootSignature(
        0,
        signature->GetBufferPointer(),
        signature->GetBufferSize(),
        SilkMarshal.GuidPtrOf<ID3D12RootSignature>(),
        (void**)&rootSig);

    signature->Release();
    if(error != null)
      error->Release();

    if(hr.IsFailure)
      throw new InvalidOperationException($"Failed to create root signature: {hr}");

    p_rootSignature = rootSig;
  }

  private unsafe InputElementDesc[] CreateInputLayout(InputLayoutDescription _layoutDesc)
  {
    if(_layoutDesc?.Elements == null || _layoutDesc.Elements.Count == 0)
      return Array.Empty<InputElementDesc>();

    var elements = new InputElementDesc[_layoutDesc.Elements.Count];

    for(int i = 0; i < _layoutDesc.Elements.Count; i++)
    {
      var element = _layoutDesc.Elements[i];
      elements[i] = new InputElementDesc
      {
        SemanticName = (byte*)SilkMarshal.StringToPtr(element.SemanticName),
        SemanticIndex = element.SemanticIndex,
        Format = ConvertFormat(element.Format),
        InputSlot = element.InputSlot,
        AlignedByteOffset = element.AlignedByteOffset == uint.MaxValue
              ? D3D12.AppendAlignedElement
              : element.AlignedByteOffset,
        InputSlotClass = element.InputSlotClass == GraphicsAPI.Enums.InputClassification.PerVertexData
              ? Silk.NET.Direct3D12.InputClassification.PerVertexData
              : Silk.NET.Direct3D12.InputClassification.PerInstanceData,
        InstanceDataStepRate = element.InstanceDataStepRate
      };
    }

    return elements;
  }

  private BlendDesc ConvertBlendState(BlendStateDescription _desc)
  {
    if(_desc == null)
      _desc = new BlendStateDescription();

    var blendDesc = new BlendDesc
    {
      AlphaToCoverageEnable = _desc.AlphaToCoverageEnable,
      IndependentBlendEnable = _desc.IndependentBlendEnable
    };

    for(int i = 0; i < 8; i++)
    {
      if(i < _desc.RenderTargets.Length)
      {
        var rt = _desc.RenderTargets[i];
        blendDesc.RenderTarget[i] = new RenderTargetBlendDesc
        {
          BlendEnable = rt.BlendEnable,
          LogicOpEnable = false,
          SrcBlend = ConvertBlend(rt.SrcBlend),
          DestBlend = ConvertBlend(rt.DstBlend),
          BlendOp = ConvertBlendOp(rt.BlendOp),
          SrcBlendAlpha = ConvertBlend(rt.SrcBlendAlpha),
          DestBlendAlpha = ConvertBlend(rt.DstBlendAlpha),
          BlendOpAlpha = ConvertBlendOp(rt.BlendOpAlpha),
          LogicOp = LogicOp.Noop,
          RenderTargetWriteMask = (byte)rt.WriteMask
        };
      }
      else
      {
        blendDesc.RenderTarget[i] = new RenderTargetBlendDesc
        {
          BlendEnable = false,
          LogicOpEnable = false,
          SrcBlend = Blend.One,
          DestBlend = Blend.Zero,
          BlendOp = BlendOp.Add,
          SrcBlendAlpha = Blend.One,
          DestBlendAlpha = Blend.Zero,
          BlendOpAlpha = BlendOp.Add,
          LogicOp = LogicOp.Noop,
          RenderTargetWriteMask = (byte)ColorWriteMask.All
        };
      }
    }

    return blendDesc;
  }

  private RasterizerDesc ConvertRasterizerState(RasterizerStateDescription _desc)
  {
    if(_desc == null)
      _desc = new RasterizerStateDescription();

    return new RasterizerDesc
    {
      FillMode = _desc.FillMode == GraphicsAPI.Enums.FillMode.Solid ? Silk.NET.Direct3D12.FillMode.Solid : Silk.NET.Direct3D12.FillMode.Wireframe,
      CullMode = ConvertCullMode(_desc.CullMode),
      FrontCounterClockwise = _desc.FrontCounterClockwise,
      DepthBias = _desc.DepthBias,
      DepthBiasClamp = _desc.DepthBiasClamp,
      SlopeScaledDepthBias = _desc.SlopeScaledDepthBias,
      DepthClipEnable = _desc.DepthClipEnable,
      MultisampleEnable = _desc.MultisampleEnable,
      AntialiasedLineEnable = _desc.AntialiasedLineEnable,
      ForcedSampleCount = 0,
      ConservativeRaster = ConservativeRasterizationMode.Off
    };
  }

  private DepthStencilDesc ConvertDepthStencilState(DepthStencilStateDescription _desc)
  {
    if(_desc == null)
      _desc = new DepthStencilStateDescription();

    return new DepthStencilDesc
    {
      DepthEnable = _desc.DepthEnable,
      DepthWriteMask = _desc.DepthWriteEnable
            ? DepthWriteMask.All
            : DepthWriteMask.Zero,
      DepthFunc = ConvertComparisonFunc(_desc.DepthFunction),
      StencilEnable = _desc.StencilEnable,
      StencilReadMask = _desc.StencilReadMask,
      StencilWriteMask = _desc.StencilWriteMask,
      FrontFace = ConvertStencilOp(_desc.FrontFace),
      BackFace = ConvertStencilOp(_desc.BackFace)
    };
  }
  private DepthStencilopDesc ConvertStencilOp(StencilOpDescription _desc)
  {
    return new DepthStencilopDesc
    {
      StencilFailOp = ConvertStencilOperation(_desc.StencilFailOp),
      StencilDepthFailOp = ConvertStencilOperation(_desc.StencilDepthFailOp),
      StencilPassOp = ConvertStencilOperation(_desc.StencilPassOp),
      StencilFunc = ConvertComparisonFunc(_desc.StencilFunction)
    };
  }

  private Format ConvertFormat(TextureFormat _format) => DX12Helpers.ConvertFormat(_format);

  private Blend ConvertBlend(BlendFactor _option) => DX12Helpers.ConvertBlend(_option);

  private BlendOp ConvertBlendOp(BlendOperation _operation) => DX12Helpers.ConvertBlendOp(_operation);

  private Silk.NET.Direct3D12.CullMode ConvertCullMode(GraphicsAPI.Enums.CullMode _mode) => DX12Helpers.ConvertCullMode(_mode);

  private ComparisonFunc ConvertComparisonFunc(ComparisonFunction _func) => DX12Helpers.ConvertComparisonFunc(_func);

  private StencilOp ConvertStencilOperation(StencilOperation _operation) => DX12Helpers.ConvertStencilOperation(_operation);

  private PrimitiveTopologyType ConvertPrimitiveTopologyType(PrimitiveTopology _topology) => DX12Helpers.ConvertPrimitiveTopologyType(_topology);

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(DX12RenderState));
  }
}
