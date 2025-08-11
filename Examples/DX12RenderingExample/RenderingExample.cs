using System;
using System.Numerics;
using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using Resources.Enums;
using Directx12Impl;
using Resources;
using Directx12Impl.Builders;

namespace DX12RenderingExample;

/// <summary>
/// Полный пример использования DX12 рендеринга
/// </summary>
public class RenderingExample
{
  private DX12GraphicsDevice device;
  private DX12SwapChain swapChain;
  private DX12CommandBuffer commandBuffer;
  private DX12RenderState simpleRenderState;
  private DX12Buffer vertexBuffer;
  private DX12Buffer indexBuffer;
  private DX12Buffer constantBuffer;
  private DX12Texture depthTexture;
  private DX12TextureView depthStencilView;

  public void Initialize(IntPtr windowHandle, uint width, uint height)
  {

    // 1. Создаем устройство
    device = new DX12GraphicsDevice(true);

    // 2. Создаем SwapChain
    var swapChainDesc = new SwapChainDescription
    {
      Width = width,
      Height = height,
      Format = TextureFormat.R8G8B8A8_UNORM,
      BufferCount = 2,
      SampleCount = 1,
      SwapEffect = SwapEffect.FlipDiscard
    };

    swapChain = device.CreateSwapChain(swapChainDesc, windowHandle) as DX12SwapChain;

    // 3. Создаем Pipeline State используя builder
    simpleRenderState = new DX12PipelineStateBuilder(device, "SimplePipeline")
        .WithVertexShader("Shaders/Simple.vs.hlsl")
        .WithPixelShader("Shaders/Simple.ps.hlsl")
        .WithAutoInputLayout()
        .WithDefaultStates()
        .WithRenderTargets(TextureFormat.R8G8B8A8_UNORM)
        .WithDepthStencilFormat(TextureFormat.D32_FLOAT)
        .Build();

    // 4. Создаем Command Buffer
    commandBuffer = device.CreateCommandBuffer(
        CommandBufferType.Direct,
        CommandBufferExecutionMode.Immediate) as DX12CommandBuffer;

    // 5. Создаем ресурсы
    CreateResources();
  }

  private void CreateResources()
  {
    // Vertex Buffer - треугольник
    float[] vertices = new float[]
    {
            // Position (x, y, z)    Color (r, g, b, a)
             0.0f,  0.5f, 0.0f,     1.0f, 0.0f, 0.0f, 1.0f,  // Top (red)
            -0.5f, -0.5f, 0.0f,     0.0f, 1.0f, 0.0f, 1.0f,  // Bottom-left (green)
             0.5f, -0.5f, 0.0f,     0.0f, 0.0f, 1.0f, 1.0f   // Bottom-right (blue)
    };

    var vbDesc = new BufferDescription
    {
      Name = "VertexBuffer",
      Size = (ulong)(vertices.Length * sizeof(float)),
      BufferUsage = BufferUsage.Vertex,
      BindFlags = BindFlags.VertexBuffer,
      Usage = ResourceUsage.Default,
      Stride = 7 * sizeof(float) // 3 floats position + 4 floats color
    };
    vertexBuffer = device.CreateBuffer(vbDesc) as DX12Buffer;
    // TODO: Upload vertices data when data transfer is implemented
    // vertexBuffer.SetData(vertices);

    // Index Buffer
    ushort[] indices = new ushort[] { 0, 1, 2 };

    var ibDesc = new BufferDescription
    {
      Name = "IndexBuffer",
      Size = (ulong)(indices.Length * sizeof(ushort)),
      BufferUsage = BufferUsage.Index,
      BindFlags = BindFlags.IndexBuffer,
      Usage = ResourceUsage.Default,
      Stride = sizeof(ushort)
    };
    indexBuffer = device.CreateBuffer(ibDesc) as DX12Buffer;
    // TODO: Upload indices data
    // indexBuffer.SetData(indices);

    // Constant Buffer для матриц
    var cbDesc = new BufferDescription
    {
      Name = "ConstantBuffer",
      Size = 256, // Минимум для constant buffer (выравнено по 256)
      BufferUsage = BufferUsage.Constant,
      BindFlags = BindFlags.ConstantBuffer,
      Usage = ResourceUsage.Dynamic,
      CPUAccessFlags = CPUAccessFlags.Write
    };
    constantBuffer = device.CreateBuffer(cbDesc) as DX12Buffer;

    // Depth Buffer
    var depthDesc = new TextureDescription
    {
      Name = "DepthBuffer",
      Width = swapChain.Description.Width,
      Height = swapChain.Description.Height,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Format = TextureFormat.D32_FLOAT,
      SampleCount = 1,
      TextureUsage = TextureUsage.DepthStencil,
      BindFlags = BindFlags.DepthStencil,
      Usage = ResourceUsage.Default
    };
    depthTexture = device.CreateTexture(depthDesc) as DX12Texture;
    depthStencilView = depthTexture.GetDefaultDepthStencilView() as DX12TextureView;
  }

  public void Render()
  {
    // Начинаем кадр
    device.BeginFrame();

    // Получаем текущий back buffer
    uint backBufferIndex = swapChain.CurrentBackBufferIndex;
    var backBuffer = swapChain.GetBackBuffer(backBufferIndex);
    var renderTargetView = swapChain.GetBackBufferRTV(backBufferIndex);

    // Начинаем запись команд
    commandBuffer.Begin();

    // Переводим back buffer в состояние render target
    commandBuffer.TransitionResource(backBuffer, ResourceState.RenderTarget);

    // Устанавливаем render targets
    commandBuffer.SetRenderTarget(renderTargetView, depthStencilView);

    // Устанавливаем viewport
    commandBuffer.SetViewport(new Viewport
    {
      X = 0,
      Y = 0,
      Width = swapChain.Description.Width,
      Height = swapChain.Description.Height,
      MinDepth = 0.0f,
      MaxDepth = 1.0f
    });

    // Устанавливаем scissor rect
    commandBuffer.SetScissorRect(new Rectangle
    {
      X = 0,
      Y = 0,
      Width = (int)swapChain.Description.Width,
      Height = (int)swapChain.Description.Height
    });

    // Очищаем render target и depth buffer
    commandBuffer.ClearRenderTarget(renderTargetView, new Vector4(0.2f, 0.3f, 0.4f, 1.0f));
    commandBuffer.ClearDepthStencil(depthStencilView, ClearFlags.Depth, 1.0f, 0);

    // === Рендеринг ===

    // Устанавливаем Pipeline State
    commandBuffer.SetRenderState(simpleRenderState);

    // Устанавливаем примитивную топологию
    commandBuffer.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

    // Привязываем vertex и index buffers
    commandBuffer.SetVertexBuffer(vertexBuffer.GetDefaultShaderResourceView(), 0);
    commandBuffer.SetIndexBuffer(indexBuffer.GetDefaultShaderResourceView(), IndexFormat.UInt16);

    // Привязываем constant buffer
    commandBuffer.SetConstantBuffer(ShaderStage.Vertex, 0, constantBuffer.GetDefaultShaderResourceView());

    // Рисуем
    commandBuffer.DrawIndexed(3, 1, 0, 0, 0);

    // Переводим back buffer в состояние present
    commandBuffer.TransitionResource(backBuffer, ResourceState.Present);

    // Заканчиваем запись команд
    commandBuffer.End();

    // Выполняем команды
    device.ExecuteCommandBuffer(commandBuffer);

    // Презентуем кадр
    //swapChain.Present(1); // VSync on

    // Заканчиваем кадр
    device.EndFrame();
  }

  public void UpdateConstantBuffer(Matrix4x4 worldViewProj)
  {
    // TODO: Когда data transfer будет реализован
    // var mapped = constantBuffer.Map(MapMode.WriteDiscard);
    // Marshal.StructureToPtr(worldViewProj, mapped, false);
    // constantBuffer.Unmap();
  }

  public void Cleanup()
  {
    // Ждем завершения GPU
    device.WaitForGPU();

    // Освобождаем ресурсы
    depthStencilView?.Dispose();
    depthTexture?.Dispose();
    constantBuffer?.Dispose();
    indexBuffer?.Dispose();
    vertexBuffer?.Dispose();
    simpleRenderState?.Dispose();
    commandBuffer?.Dispose();
    swapChain?.Dispose();
    device?.Dispose();
  }
}
