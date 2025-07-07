using Core;
using Core.Enums;

using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using System.Numerics;

namespace Passes;

public class GeometryPass: RenderPass
{
  public ResourceHandle ColorTarget { get; private set; }
  public ResourceHandle DepthTarget { get; private set; }
  public ResourceHandle CameraBuffer { get; private set; }

  // Параметры прохода
  public uint ViewportWidth { get; set; } = 1920;
  public uint ViewportHeight { get; set; } = 1080;
  public bool ClearColor { get; set; } = true;
  public bool ClearDepth { get; set; } = true;
  public Vector4 ClearColorValue { get; set; } = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
  public float ClearDepthValue { get; set; } = 1.0f;

  // Данные для рендеринга (в реальной реализации это будет приходить из ECS)
  public List<RenderableObject> RenderableObjects { get; set; } = new();

  public GeometryPass() : base("GeometryPass")
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.High;
  }

  public override void Setup(RenderGraphBuilder builder)
  {
    // Создаем color target
    ColorTarget = builder.CreateColorTarget("GeometryColor", ViewportWidth, ViewportHeight, TextureFormat.R8G8B8A8_UNORM);

    // Создаем depth target
    DepthTarget = builder.CreateDepthTarget("GeometryDepth", ViewportWidth, ViewportHeight, TextureFormat.D32_FLOAT);

    // Создаем константный буфер для камеры
    CameraBuffer = builder.CreateConstantBuffer("CameraConstants", 256); // 256 байт для матриц камеры

    // Указываем что мы пишем в эти targets
    builder.WriteTexture(ColorTarget);
    builder.WriteTextureAsDepth(DepthTarget);
    builder.WriteBuffer(CameraBuffer);

    // Устанавливаем persistent lifetime для targets (могут использоваться в других проходах)
    builder.SetResourceLifetime(ColorTarget, ResourceLifetime.Persistent);
    builder.SetResourceLifetime(DepthTarget, ResourceLifetime.Persistent);
    builder.SetResourceLifetime(CameraBuffer, ResourceLifetime.Persistent);
  }

  public override void Execute(RenderPassContext context)
  {
    var commandBuffer = context.CommandBuffer;

    // Получаем ресурсы
    var colorTexture = context.GetTexture(ColorTarget);
    var depthTexture = context.GetTexture(DepthTarget);
    var cameraBuffer = context.GetBuffer(CameraBuffer);

    // Получаем views
    var colorView = colorTexture.GetDefaultRenderTargetView();
    var depthView = depthTexture.GetDefaultDepthStencilView();

    // Устанавливаем render targets
    commandBuffer.SetRenderTarget(colorView, depthView);

    // Устанавливаем viewport
    context.SetFullScreenViewport();

    // Очищаем targets если нужно
    if(ClearColor)
    {
      commandBuffer.ClearRenderTarget(colorView, ClearColorValue);
    }

    if(ClearDepth)
    {
      commandBuffer.ClearDepthStencil(depthView, ClearFlags.DepthStencil, ClearDepthValue, 0);
    }

    // Обновляем camera constants
    UpdateCameraConstants(context, cameraBuffer);

    // Рендерим все объекты
    RenderObjects(context);
  }

  private void UpdateCameraConstants(RenderPassContext context, IBuffer cameraBuffer)
  {
    // Создаем структуру констант камеры
    var cameraConstants = new CameraConstants
    {
      ViewMatrix = context.FrameData.ViewMatrix,
      ProjectionMatrix = context.FrameData.ProjectionMatrix,
      ViewProjectionMatrix = context.FrameData.ViewProjectionMatrix,
      CameraPosition = new Vector4(context.FrameData.CameraPosition, 1.0f),
      ScreenResolution = new Vector4(ViewportWidth, ViewportHeight, 1.0f / ViewportWidth, 1.0f / ViewportHeight)
    };

    // Записываем константы в буфер
    var mappedData = cameraBuffer.Map(MapMode.WriteDiscard);
    unsafe
    {
      *(CameraConstants*)mappedData = cameraConstants;
    }
    cameraBuffer.Unmap();
  }

  private void RenderObjects(RenderPassContext context)
  {
    var commandBuffer = context.CommandBuffer;

    // В реальной реализации здесь будет:
    // 1. Сортировка объектов по материалам/расстоянию
    // 2. Установка шейдеров и состояний
    // 3. Batch'инг draw calls

    foreach(var obj in RenderableObjects)
    {
      if(!obj.Visible)
        continue;

      // Устанавливаем vertex/index буферы
      if(obj.VertexBuffer.IsValid())
      {
        var vertexBuffer = context.GetBuffer(obj.VertexBuffer);
        var vertexView = vertexBuffer.GetDefaultShaderResourceView(); // Для vertex buffer нужен специальный view
        commandBuffer.SetVertexBuffer(vertexView, 0);
      }

      if(obj.IndexBuffer.IsValid())
      {
        var indexBuffer = context.GetBuffer(obj.IndexBuffer);
        var indexView = indexBuffer.GetDefaultShaderResourceView();
        commandBuffer.SetIndexBuffer(indexView, obj.IndexFormat);
      }

      // Устанавливаем материал/текстуры
      SetupMaterial(context, obj.Material);

      // Draw call
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

  private void SetupMaterial(RenderPassContext context, Material material)
  {
    if(material == null)
      return;

    var commandBuffer = context.CommandBuffer;

    // Устанавливаем шейдеры
    if(material.VertexShader != null)
      commandBuffer.SetVertexShader(material.VertexShader);

    if(material.PixelShader != null)
      commandBuffer.SetPixelShader(material.PixelShader);

    // Устанавливаем текстуры
    for(int i = 0; i < material.Textures.Count; i++)
    {
      if(material.Textures[i].IsValid())
      {
        var texture = context.GetTexture(material.Textures[i]);
        var textureView = texture.GetDefaultShaderResourceView();
        commandBuffer.SetShaderResource(ShaderStage.Pixel, (uint)i, textureView);
      }
    }

    // Устанавливаем константные буферы материала
    if(material.ConstantBuffer.IsValid())
    {
      var constantBuffer = context.GetBuffer(material.ConstantBuffer);
      var constantView = constantBuffer.GetDefaultShaderResourceView();
      commandBuffer.SetConstantBuffer(ShaderStage.Pixel, 1, constantView); // Slot 1, slot 0 для камеры
    }

    // Устанавливаем семплеры
    for(int i = 0; i < material.Samplers.Count; i++)
    {
      if(material.Samplers[i] != null)
      {
        commandBuffer.SetSampler(ShaderStage.Pixel, (uint)i, material.Samplers[i]);
      }
    }
  }
}
