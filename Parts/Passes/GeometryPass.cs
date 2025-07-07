using Core;
using Core.Enums;
using Core.Extensions;

using GraphicsAPI.Interfaces;

using Resources.Enums;

using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace Passes;

public class GeometryPass: RenderPass
{
  public GeometryPass() : base("GeometryPass")
  {

  }

  public ResourceHandle ColorTarget { get; private set; }
  public ResourceHandle DepthTarget { get; private set; }
  public ResourceHandle CameraBuffer { get; private set; }

  public uint ViewportWidth { get; set; } = 1280;
  public uint ViewportHeight { get; set; } = 720;
  public bool ClearColor { get; set; } = true;
  public bool ClearDepth { get; set; } = true;
  public Vector4 ClearColorValue { get; set; } = new Vector4(0f, 0f, 0f, 1f);
  public float ClearDepthValue { get; set; } = 1f;

  public List<RenderableObject> RenderableObjects { get; set; } = [];

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

  public override void Execute(RenderPassContext _ctx)
  {
    var commandBuffer = _ctx.CommandBuffer;

    var colorTexture = _ctx.GetTexture(ColorTarget);
    var depthTexture = _ctx.GetTexture(DepthTarget);
    var cameraBuffer = _ctx.GetBuffer(CameraBuffer);

    commandBuffer.SetRenderTargets(new[] { colorTexture }, depthTexture);

    _ctx.SetFullScreenViewport();

    if(ClearColor)
    {
      commandBuffer.ClearRenderTarget(colorTexture, ClearColorValue);
    }

    if(ClearDepth)
    {
      commandBuffer.ClearDepthStencil(depthTexture, ClearDepthValue, 0);
    }

    UpdateCameraConstants(_ctx, cameraBuffer);

    RenderObjects(_ctx);
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

    var mappedData = _cameraBuffer.Map();

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
        commandBuffer.SetVertexBuffer(vertexBuffer, obj.VertexStride);
      }

      if(obj.IndexBuffer.IsValid())
      {
        var indexBuffer = _context.GetBuffer(obj.IndexBuffer);
        commandBuffer.SetIndexBuffer(indexBuffer, obj.IndexFormat);
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
        commandBuffer.SetShaderResource(i, texture);
      }
    }

    if(_material.ConstantBuffer.IsValid())
    {
      var constantBuffer = _context.GetBuffer(_material.ConstantBuffer);
      commandBuffer.SetConstantBuffer(1, constantBuffer);
    }
  }
}
