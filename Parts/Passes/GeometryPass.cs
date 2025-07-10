using Core;
using Core.Enums;

using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using System.Numerics;

namespace Passes;

public class GeometryPass: RenderPass
{
  public GeometryPass() : base("GeometryPass")
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.High;
  }

  public ResourceHandle ColorTarget { get; private set; }
  public ResourceHandle DepthTarget { get; private set; }
  public ResourceHandle CameraBuffer { get; private set; }

  public uint ViewportWidth { get; set; } = 1920;
  public uint ViewportHeight { get; set; } = 1080;
  public bool ClearColor { get; set; } = true;
  public bool ClearDepth { get; set; } = true;
  public Vector4 ClearColorValue { get; set; } = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
  public float ClearDepthValue { get; set; } = 1.0f;

  public List<RenderableObject> RenderableObjects { get; set; } = new();

  public override void Setup(RenderGraphBuilder _builder)
  {

    ColorTarget = _builder.CreateColorTarget("GeometryColor", ViewportWidth, ViewportHeight, TextureFormat.R8G8B8A8_UNORM);
    DepthTarget = _builder.CreateDepthTarget("GeometryDepth", ViewportWidth, ViewportHeight, TextureFormat.D32_FLOAT);
    CameraBuffer = _builder.CreateConstantBuffer("CameraConstants", 256);

    _builder.WriteTexture(ColorTarget);
    _builder.WriteTextureAsDepth(DepthTarget);
    _builder.WriteBuffer(CameraBuffer);

    _builder.SetResourceLifetime(ColorTarget, ResourceLifetime.Persistent);
    _builder.SetResourceLifetime(DepthTarget, ResourceLifetime.Persistent);
    _builder.SetResourceLifetime(CameraBuffer, ResourceLifetime.Persistent);
  }

  public override void Execute(RenderPassContext _context)
  {
    var commandBuffer = _context.CommandBuffer;

    var colorTexture = _context.GetTexture(ColorTarget);
    var depthTexture = _context.GetTexture(DepthTarget);
    var cameraBuffer = _context.GetBuffer(CameraBuffer);

    var colorView = colorTexture.GetDefaultRenderTargetView();
    var depthView = depthTexture.GetDefaultDepthStencilView();

    commandBuffer.SetRenderTarget(colorView, depthView);
    _context.SetFullScreenViewport();

    if(ClearColor)
    {
      commandBuffer.ClearRenderTarget(colorView, ClearColorValue);
    }

    if(ClearDepth)
    {
      commandBuffer.ClearDepthStencil(depthView, ClearFlags.DepthStencil, ClearDepthValue, 0);
    }

    UpdateCameraConstants(_context, cameraBuffer);
    RenderObjects(_context);
  }

  private void UpdateCameraConstants(RenderPassContext _context, IBuffer _cameraBuffer)
  {
    var cameraConstants = new CameraConstants
    {
      ViewMatrix = _context.FrameData.ViewMatrix,
      ProjectionMatrix = _context.FrameData.ProjectionMatrix,
      ViewProjectionMatrix = _context.FrameData.ViewProjectionMatrix,
      CameraPosition = new Vector4(_context.FrameData.CameraPosition, 1.0f),
      ScreenResolution = new Vector4(ViewportWidth, ViewportHeight, 1.0f / ViewportWidth, 1.0f / ViewportHeight)
    };

    // Записываем константы в буфер
    var mappedData = _cameraBuffer.Map(MapMode.WriteDiscard);
    unsafe
    {
      *(CameraConstants*)mappedData = cameraConstants;
    }
    _cameraBuffer.Unmap();
  }

  private void RenderObjects(RenderPassContext _context)
  {
    var commandBuffer = _context.CommandBuffer;

    foreach(var obj in RenderableObjects)
    {
      if(!obj.Visible)
        continue;

      if(obj.VertexBuffer.IsValid())
      {
        var vertexBuffer = _context.GetBuffer(obj.VertexBuffer);
        var vertexView = vertexBuffer.GetDefaultShaderResourceView();
        commandBuffer.SetVertexBuffer(vertexView, 0);
      }

      if(obj.IndexBuffer.IsValid())
      {
        var indexBuffer = _context.GetBuffer(obj.IndexBuffer);
        var indexView = indexBuffer.GetDefaultShaderResourceView();
        commandBuffer.SetIndexBuffer(indexView, obj.IndexFormat);
      }

      SetupMaterial(_context, obj.Material);

      if(obj.IndexBuffer.IsValid())
      {
        commandBuffer.DrawIndexed(obj.IndexCount, obj.InstanceCount);
      }
      else
      {
        commandBuffer.Draw(obj.VertexCount, obj.InstanceCount);
      }
    }
  }

  private void SetupMaterial(RenderPassContext _context, Material _material)
  {
    if(_material == null)
      return;

    var commandBuffer = _context.CommandBuffer;

    if(_material.VertexShader != null)
      commandBuffer.SetVertexShader(_material.VertexShader);

    if(_material.PixelShader != null)
      commandBuffer.SetPixelShader(_material.PixelShader);

    for(int i = 0; i < _material.Textures.Count; i++)
    {
      if(_material.Textures[i].IsValid())
      {
        var texture = _context.GetTexture(_material.Textures[i]);
        var textureView = texture.GetDefaultShaderResourceView();
        commandBuffer.SetShaderResource(ShaderStage.Pixel, (uint)i, textureView);
      }
    }

    if(_material.ConstantBuffer.IsValid())
    {
      var constantBuffer = _context.GetBuffer(_material.ConstantBuffer);
      var constantView = constantBuffer.GetDefaultShaderResourceView();
      commandBuffer.SetConstantBuffer(ShaderStage.Pixel, 1, constantView);
    }

    for(int i = 0; i < _material.Samplers.Count; i++)
    {
      if(_material.Samplers[i] != null)
      {
        commandBuffer.SetSampler(ShaderStage.Pixel, (uint)i, _material.Samplers[i]);
      }
    }
  }
}
