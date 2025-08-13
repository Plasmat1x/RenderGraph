using Directx12Impl.Parts.Utils;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Extensions;
using GraphicsAPI.Interfaces;
using GraphicsAPI.Reflections.Enums;
using GraphicsAPI.Utils;

using Resources.Enums;
using Resources.Extensions;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Builders;

public class DX12PipelineStateBuilder
{
  private readonly DX12GraphicsDevice p_device;
  private readonly PipelineStateDescription p_pipelineDesc;
  private readonly RenderStateDescription p_renderDesc;

  public DX12PipelineStateBuilder(DX12GraphicsDevice _device, string _name = null)
  {
    p_device = _device;
    p_pipelineDesc = new PipelineStateDescription();
    p_renderDesc = new RenderStateDescription
    {
      Name = _name ?? "PipelineState"
    };
  }

  // === Shader setup ===
  public DX12PipelineStateBuilder WithVertexShader(string _filePath, string _entryPoint = "VSMain")
  {
    var shaderDesc = new ShaderDescription
    {
      Name = Path.GetFileNameWithoutExtension(_filePath),
      Stage = ShaderStage.Vertex,
      FilePath = _filePath,
      EntryPoint = _entryPoint
    };
    p_pipelineDesc.VertexShader = p_device.CreateShader(shaderDesc);
    return this;
  }

  public DX12PipelineStateBuilder WithPixelShader(string _filePath, string _entryPoint = "PSMain")
  {
    var shaderDesc = new ShaderDescription
    {
      Name = Path.GetFileNameWithoutExtension(_filePath),
      Stage = ShaderStage.Pixel,
      FilePath = _filePath,
      EntryPoint = _entryPoint
    };
    p_pipelineDesc.PixelShader = p_device.CreateShader(shaderDesc);
    return this;
  }

  public DX12PipelineStateBuilder WithComputeShader(string _filePath, string _entryPoint = "CSMain")
  {
    var shaderDesc = new ShaderDescription { Name = Path.GetFileNameWithoutExtension(_filePath), Stage = ShaderStage.Compute, FilePath = _filePath, EntryPoint = _entryPoint };
    p_pipelineDesc.ComputeShader = p_device.CreateShader(shaderDesc);
    return this;
  }

  public DX12PipelineStateBuilder WithShaders(IShader _vertexShader, IShader _pixelShader)
  {
    p_pipelineDesc.VertexShader = _vertexShader;
    p_pipelineDesc.PixelShader = _pixelShader;
    return this;
  }

  // === Input layout from vertex shader reflection ===
  public DX12PipelineStateBuilder WithAutoInputLayout()
  {
    if(p_pipelineDesc.VertexShader == null)
      throw new InvalidOperationException("Vertex shader must be set before auto input layout");

    var reflection = p_pipelineDesc.VertexShader.GetReflection();

    if(reflection?.InputParameters == null || reflection.InputParameters.Count == 0)
    {
      Console.WriteLine("Warning: No input parameters found in shader reflection, using fallback layout");
      // Используем предопределенный layout
      p_pipelineDesc.InputLayout = CreateSimpleInputLayout();
      return this;
    }

    var inputLayout = new InputLayoutDescription();

    uint offset = 0;

    foreach(var param in reflection.InputParameters)
    {
      var element = new InputElementDescription
      {
        SemanticName = param.SemanticName,
        SemanticIndex = param.SemanticIndex,
        Format = GetFormatFromMask(param.Mask, param.ComponentType),
        InputSlot = 0,
        AlignedByteOffset = offset,
        InputSlotClass = GraphicsAPI.Enums.InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      };
      inputLayout.Elements.Add(element);
      offset += element.Format.GetFormatSize();
    }

    p_pipelineDesc.InputLayout = inputLayout;
    return this;
  }

  public DX12PipelineStateBuilder WithInputLayout(InputLayoutDescription _layout)
  {
    p_pipelineDesc.InputLayout = _layout;
    return this;
  }

  // === Render state setup ===
  public DX12PipelineStateBuilder WithBlendState(BlendStateDescription _blendState)
  {
    p_renderDesc.BlendState = _blendState;
    return this;
  }

  public DX12PipelineStateBuilder WithDepthStencilState(DepthStencilStateDescription _depthStencilState)
  {
    p_renderDesc.DepthStencilState = _depthStencilState;
    return this;
  }

  public DX12PipelineStateBuilder WithRasterizerState(RasterizerStateDescription _rasterizerState)
  {
    p_renderDesc.RasterizerState = _rasterizerState;
    return this;
  }

  // === Presets ===
  public DX12PipelineStateBuilder WithDefaultStates()
  {
    // Default blend state - opaque
    p_renderDesc.BlendState = new BlendStateDescription
    {
      AlphaToCoverageEnable = false,
      IndependentBlendEnable = false,
      RenderTargets = new[]
      {
        new RenderTargetBlendDescription
        {
          BlendEnable = false,
          SrcBlend = BlendFactor.One,
          DstBlend = BlendFactor.Zero,
          BlendOp = BlendOperation.Add,
          SrcBlendAlpha = BlendFactor.One,
          DstBlendAlpha = BlendFactor.Zero,
          BlendOpAlpha = BlendOperation.Add,
          WriteMask = ColorWriteMask.All
        }
      }
    };

    // Default depth stencil - depth test and write enabled
    p_renderDesc.DepthStencilState = new DepthStencilStateDescription
    {
      DepthEnable = true,
      DepthWriteEnable = true,
      DepthFunction = ComparisonFunction.Less,
      StencilEnable = false,
      StencilReadMask = 0xFF,
      StencilWriteMask = 0xFF,
      FrontFace = new StencilOpDescription
      {
        StencilFailOp = StencilOperation.Keep,
        StencilDepthFailOp = StencilOperation.Keep,
        StencilPassOp = StencilOperation.Keep,
        StencilFunction = ComparisonFunction.Always
      },
      BackFace = new StencilOpDescription
      {
        StencilFailOp = StencilOperation.Keep,
        StencilDepthFailOp = StencilOperation.Keep,
        StencilPassOp = StencilOperation.Keep,
        StencilFunction = ComparisonFunction.Always
      }
    };

    // Default rasterizer - solid fill, cull back
    p_renderDesc.RasterizerState = new RasterizerStateDescription
    {
      FillMode = GraphicsAPI.Enums.FillMode.Solid,
      CullMode = GraphicsAPI.Enums.CullMode.Back,
      FrontCounterClockwise = false,
      DepthBias = 0,
      DepthBiasClamp = 0.0f,
      SlopeScaledDepthBias = 0.0f,
      DepthClipEnable = true,
      MultisampleEnable = false,
      AntialiasedLineEnable = false,
    };

    return this;
  }

  public DX12PipelineStateBuilder WithAlphaBlending()
  {
    p_renderDesc.BlendState = new BlendStateDescription
    {
      AlphaToCoverageEnable = false,
      IndependentBlendEnable = false,
      RenderTargets = new[]
      {
        new RenderTargetBlendDescription
        {
          BlendEnable = true,
          SrcBlend = BlendFactor.SrcAlpha,
          DstBlend = BlendFactor.InvSrcAlpha,
          BlendOp = BlendOperation.Add,
          SrcBlendAlpha = BlendFactor.One,
          DstBlendAlpha = BlendFactor.InvSrcAlpha,
          BlendOpAlpha = BlendOperation.Add,
          WriteMask = ColorWriteMask.All
        }
      }
    };
    return this;
  }

  public DX12PipelineStateBuilder WithWireframe()
  {
    if(p_renderDesc.RasterizerState == null)
      p_renderDesc.RasterizerState = new RasterizerStateDescription();

    p_renderDesc.RasterizerState.FillMode = GraphicsAPI.Enums.FillMode.Wireframe;
    return this;
  }

  public DX12PipelineStateBuilder WithNoCulling()
  {
    if(p_renderDesc.RasterizerState == null)
      p_renderDesc.RasterizerState = new RasterizerStateDescription();

    p_renderDesc.RasterizerState.CullMode = GraphicsAPI.Enums.CullMode.None;
    return this;
  }

  // === Render target setup ===
  public DX12PipelineStateBuilder WithRenderTargets(params TextureFormat[] _formats)
  {
    p_pipelineDesc.RenderTargetCount = (uint)_formats.Length;
    p_pipelineDesc.RenderTargetFormats = _formats;
    return this;
  }

  public DX12PipelineStateBuilder WithDepthStencilFormat(TextureFormat _format)
  {
    p_pipelineDesc.DepthStencilFormat = _format;
    return this;
  }

  // === Topology ===
  public DX12PipelineStateBuilder WithPrimitiveTopology(PrimitiveTopology _topology)
  {
    p_pipelineDesc.PrimitiveTopology = _topology;
    return this;
  }

  // === Build ===
  public DX12RenderState Build()
  {
    // Validate
    if(p_pipelineDesc.ComputeShader == null)
    {
      if(p_pipelineDesc.VertexShader == null)
        throw new InvalidOperationException("Vertex shader is required for graphics pipeline");

      if(p_pipelineDesc.InputLayout == null)
        WithAutoInputLayout(); // Try auto layout
    }

    // Set defaults if not specified
    if(p_renderDesc.BlendState == null &&
        p_renderDesc.DepthStencilState == null &&
        p_renderDesc.RasterizerState == null)
    {
      WithDefaultStates();
    }

    // Create render state
    return p_device.CreateRenderState(p_renderDesc, p_pipelineDesc) as DX12RenderState;
  }

  // === Helper methods ===

  private static InputLayoutDescription CreateSimpleInputLayout()
  {
    return new InputLayoutDescription
    {
      Elements = new List<InputElementDescription>
        {
            new InputElementDescription
            {
                SemanticName = "POSITION",
                SemanticIndex = 0,
                Format = TextureFormat.R32G32B32_FLOAT,
                InputSlot = 0,
                AlignedByteOffset = 0,
                InputSlotClass = GraphicsAPI.Enums.InputClassification.PerVertexData,
                InstanceDataStepRate = 0
            },
            new InputElementDescription
            {
                SemanticName = "COLOR",
                SemanticIndex = 0,
                Format = TextureFormat.R32G32B32A32_FLOAT,
                InputSlot = 0,
                AlignedByteOffset = 12, // sizeof(float3)
                InputSlotClass = GraphicsAPI.Enums.InputClassification.PerVertexData,
                InstanceDataStepRate = 0
            }
        }
    };
  }

  private TextureFormat GetFormatFromMask(byte _mask, RegisterComponentType _componentType)
  {
    var componentCount = 0;
    if((_mask & 0x1) != 0)
      componentCount++;
    if((_mask & 0x2) != 0)
      componentCount++;
    if((_mask & 0x4) != 0)
      componentCount++;
    if((_mask & 0x8) != 0)
      componentCount++;

    return (_componentType, componentCount) switch
    {
      (RegisterComponentType.Float32, 1) => TextureFormat.R32_FLOAT,
      (RegisterComponentType.Float32, 2) => TextureFormat.R32G32_FLOAT,
      (RegisterComponentType.Float32, 3) => TextureFormat.R32G32B32_FLOAT,
      (RegisterComponentType.Float32, 4) => TextureFormat.R32G32B32A32_FLOAT,
      (RegisterComponentType.UInt32, 1) => TextureFormat.R32_UINT,
      (RegisterComponentType.UInt32, 2) => TextureFormat.R32G32_UINT,
      (RegisterComponentType.UInt32, 3) => TextureFormat.R32G32B32_UINT,
      (RegisterComponentType.UInt32, 4) => TextureFormat.R32G32B32A32_UINT,
      (RegisterComponentType.SInt32, 1) => TextureFormat.R32_SINT,
      (RegisterComponentType.SInt32, 2) => TextureFormat.R32G32_SINT,
      (RegisterComponentType.SInt32, 3) => TextureFormat.R32G32B32_SINT,
      (RegisterComponentType.SInt32, 4) => TextureFormat.R32G32B32A32_SINT,
      _ => TextureFormat.R32G32B32A32_FLOAT
    };
  }
}
