using Core;
using Core.Enums;

using GraphicsAPI;
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

  private uint p_textureWidth;
  private uint p_textureHeight;

  public BlurPass() : base("BlurPass")
  {
  }

  public ResourceHandle InputTexture { get; set; }
  public ResourceHandle OutputTexture { get; private set; }

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

    var mappedData = blurBuffer.Map();
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

    commandBuffer.SetRenderTargets(new[] { outputTexture }, null);

    _context.SetFullScreenViewport();

    SetupBlurShaders(commandBuffer, _direction);

    commandBuffer.SetShaderResource(0, inputTexture);

    commandBuffer.SetConstantBuffer(0, blurParamsBuffer);

    commandBuffer.DrawFullscreenQuad();
  }

  private void SetupBlurShaders(CommandBuffer _commandBuffer, BlurDirection _direction)
  {
    switch(Quality)
    {
      case BlurQuality.Low:
        _commandBuffer.SetVertexShader(GetBlurVertexShader());
        _commandBuffer.SetPixelShader(GetBlurPixelShader(_direction, 3));
        break;

      case BlurQuality.Medium:
        _commandBuffer.SetVertexShader(GetBlurVertexShader());
        _commandBuffer.SetPixelShader(GetBlurPixelShader(_direction, 7));
        break;

      case BlurQuality.High:
        _commandBuffer.SetVertexShader(GetBlurVertexShader());
        _commandBuffer.SetPixelShader(GetBlurPixelShader(_direction, 15));
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

  private IShader GetBlurVertexShader() => null;
  private IShader GetBlurPixelShader(BlurDirection _direction, int _taps) => null;
}
