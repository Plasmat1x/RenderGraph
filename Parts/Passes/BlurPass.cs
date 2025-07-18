using Core;
using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Passes.Enums;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace Passes;

public class BlurPass: RenderPass
{
  private ResourceHandle p_intermediateTexture;
  private ResourceHandle p_blurParamsBuffer;

  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }

  private uint p_textureWidth;
  private uint p_textureHeight;

  private IShader p_vertexShader;
  private IShader p_horizontalBlurShader;
  private IShader p_verticalBlurShader;
  private ISampler p_linearSampler;

  public BlurPass() : base("BlurPass")
  {
    Category = PassCategory.PostProcessing;
    Priority = PassPriority.Normal;
  }

  public float BlurRadius { get; set; } = 5.0f;
  public float BlurSigma { get; set; } = 2.0f;
  public BlurDirection BlurDirection { get; set; } = BlurDirection.Both;
  public BlurQuality Quality { get; set; } = BlurQuality.High;

  public override void Setup(RenderGraphBuilder _builder)
  {
    if(!InputTexture.IsValid())
      throw new InvalidOperationException("BlurPass requires valid InputTexture");

    var inputDesc = (TextureDescription)_builder.GetResourceDescription(InputTexture);
    p_textureWidth = inputDesc.Width;
    p_textureHeight = inputDesc.Height;

    _builder.ReadTexture(InputTexture);

    var outputDesc = (TextureDescription)inputDesc.Clone();
    outputDesc.Name = "BlurOutput";
    outputDesc.TextureUsage = TextureUsage.RenderTarget;
    outputDesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;

    OutputTexture = _builder.CreateTexture("BlurOutput", outputDesc);
    _builder.WriteTexture(OutputTexture);

    if(BlurDirection == BlurDirection.Both)
    {
      var intermediateDesc = (TextureDescription)outputDesc.Clone();
      intermediateDesc.Name = "BlurIntermediate";
      p_intermediateTexture = _builder.CreateTexture("BlurIntermediate", intermediateDesc);
      _builder.WriteTexture(p_intermediateTexture);
      _builder.ReadTexture(p_intermediateTexture);
    }

    p_blurParamsBuffer = _builder.CreateConstantBuffer("BlurParams", 64);
    _builder.WriteBuffer(p_blurParamsBuffer);

    _builder.SetResourceLifetime(OutputTexture, ResourceLifetime.Persistent);
  }

  public override void OnGraphCompiled(RenderGraph _renderGraph) => CreateShaders(_renderGraph);

  public override void Execute(RenderPassContext _context)
  {
    var commandBuffer = _context.CommandBuffer;
    UpdateBlurParameters(_context);

    switch(BlurDirection)
    {
      case BlurDirection.Horizontal:
        PerformBlur(_context, InputTexture, OutputTexture, BlurDirection.Horizontal);
        break;

      case BlurDirection.Vertical:
        PerformBlur(_context, InputTexture, OutputTexture, BlurDirection.Vertical);
        break;

      case BlurDirection.Both:
        PerformBlur(_context, InputTexture, p_intermediateTexture, BlurDirection.Horizontal);
        PerformBlur(_context, p_intermediateTexture, OutputTexture, BlurDirection.Vertical);
        break;
    }
  }
  public override void Dispose()
  {
    p_vertexShader?.Dispose();
    p_horizontalBlurShader?.Dispose();
    p_verticalBlurShader?.Dispose();
    p_linearSampler?.Dispose();
    base.Dispose();
  }

  private void CreateShaders(RenderGraph _renderGraph)
  {

    var device = _renderGraph.GetGraphicsDevice();
    var vsDesc = new ShaderDescription
    {
      Name = "BlurVertexShader",
      Stage = ShaderStage.Vertex,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "VSMain"
    };
    p_vertexShader = device.CreateShader(vsDesc);

    var hBlurDesc = new ShaderDescription
    {
      Name = "HorizontalBlurShader",
      Stage = ShaderStage.Pixel,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "PSHorizontalBlur"
    };
    p_horizontalBlurShader = device.CreateShader(hBlurDesc);

    var vBlurDesc = new ShaderDescription
    {
      Name = "VerticalBlurShader",
      Stage = ShaderStage.Pixel,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "PSVerticalBlur"
    };
    p_verticalBlurShader = device.CreateShader(vBlurDesc);

    var samplerDesc = new SamplerDescription
    {
      Name = "LinearSampler",
      MinFilter = FilterMode.Linear,
      MagFilter = FilterMode.Linear,
      AddressModeU = AddressMode.Clamp,
      AddressModeV = AddressMode.Clamp
    };
    p_linearSampler = device.CreateSampler(samplerDesc);
  }

  private void UpdateBlurParameters(RenderPassContext _context)
  {
    var blurBuffer = _context.GetBuffer(p_blurParamsBuffer);

    var kernelSize = GetKernelSize();
    var texelSize = new Vector2(1.0f / p_textureWidth, 1.0f / p_textureHeight);

    var blurParams = new BlurParameters
    {
      BlurRadius = BlurRadius,
      BlurSigma = BlurSigma,
      TexelSize = texelSize,
      KernelSize = kernelSize
    };

    var mappedData = blurBuffer.Map(MapMode.WriteDiscard);
    unsafe
    {
      *(BlurParameters*)mappedData = blurParams;
    }
    blurBuffer.Unmap();
  }

  private void PerformBlur(RenderPassContext _context, ResourceHandle _input, ResourceHandle _output, BlurDirection _direction)
  {
    var commandBuffer = _context.CommandBuffer;

    var inputTexture = _context.GetTexture(_input);
    var outputTexture = _context.GetTexture(_output);
    var blurParamsBuffer = _context.GetBuffer(p_blurParamsBuffer);

    var inputView = inputTexture.GetDefaultShaderResourceView();
    var outputView = outputTexture.GetDefaultRenderTargetView();
    var paramsView = blurParamsBuffer.GetDefaultShaderResourceView();

    commandBuffer.SetRenderTarget(outputView);
    _context.SetFullScreenViewport();
    SetupBlurShaders(commandBuffer, _direction);
    commandBuffer.SetShaderResource(ShaderStage.Pixel, 0, inputView);
    commandBuffer.SetSampler(ShaderStage.Pixel, 0, p_linearSampler);
    commandBuffer.SetConstantBuffer(ShaderStage.Pixel, 0, paramsView);
    commandBuffer.DrawFullscreenQuad();
  }

  private void SetupBlurShaders(CommandBuffer _commandBuffer, BlurDirection _direction)
  {
    _commandBuffer.SetVertexShader(p_vertexShader);

    switch(_direction)
    {
      case BlurDirection.Horizontal:
        _commandBuffer.SetPixelShader(p_horizontalBlurShader);
        break;
      case BlurDirection.Vertical:
        _commandBuffer.SetPixelShader(p_verticalBlurShader);
        break;
      default:
        _commandBuffer.SetPixelShader(p_horizontalBlurShader);
        break;
    }
  }

  private int GetKernelSize()
  {
    return Quality switch
    {
      BlurQuality.Low => 3,
      BlurQuality.Medium => 7,
      BlurQuality.High => 15,
      _ => 7
    };
  }

}
