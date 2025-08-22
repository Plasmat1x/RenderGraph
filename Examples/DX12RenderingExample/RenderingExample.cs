using Directx12Impl;
using Directx12Impl.Builders;

using Examples;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Resources;
using Resources.Enums;

using System.Numerics;

/// <summary>
/// Полный пример использования DX12 рендеринга с рабочей загрузкой данных
/// </summary>
public unsafe class RenderingExample
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

  private struct Vertex
  {
    public Vector3 Position;
    public Vector4 Color;

    public Vertex(Vector3 position, Vector4 color)
    {
      Position = position;
      Color = color;
    }
  }

  public void Initialize(IntPtr windowHandle, uint width, uint height)
  {
    device = new DX12GraphicsDevice(true);

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

    CreatePipelineState();

    commandBuffer = device.CreateCommandBuffer(
        CommandBufferType.Direct,
        CommandBufferExecutionMode.Immediate) as DX12CommandBuffer;

    CreateAndUploadResources();

    Console.WriteLine("✅ DX12 Rendering Example initialized successfully!");
    Console.WriteLine($"   Device: {device.Name}");
    Console.WriteLine($"   SwapChain: {width}x{height}, Format: {swapChainDesc.Format}");
    Console.WriteLine($"   Vertex Buffer: {vertexBuffer.Size} bytes");
    Console.WriteLine($"   Index Buffer: {indexBuffer.Size} bytes");
    Console.WriteLine($"   Constant Buffer: {constantBuffer.Size} bytes");
  }

  private void CreatePipelineState()
  {
    var vsDesc = new ShaderDescription
    {
      Name = "SimpleVertexShader",
      Stage = ShaderStage.Vertex,
      SourceCode = ShaderSources.SimpleVertexShader,
      ShaderModel = "5_0",
      EntryPoint = "VSMain"
    };

    var psDesc = new ShaderDescription
    {
      Name = "SimplePixelShader",
      Stage = ShaderStage.Pixel,
      SourceCode = ShaderSources.SimplePixelShader,
      ShaderModel = "5_0",
      EntryPoint = "PSMain"
    };

    var vertexShader = device.CreateShader(vsDesc);
    var pixelShader = device.CreateShader(psDesc);

    var renderStateDesc = new RenderStateDescription
    {
      Name = "SimpleRenderState",
      BlendState = BlendStateDescription.Opaque,
      RasterizerState = RasterizerStateDescription.CullBack,
      DepthStencilState = DepthStencilStateDescription.Default
    };

    var pipelineStateDesc = new PipelineStateDescription
    {
      Name = "SimplePipeline",
      VertexShader = vertexShader,
      PixelShader = pixelShader,
      InputLayout = CreateInputLayout(),
      PrimitiveTopology = PrimitiveTopology.TriangleList,
      RenderTargetFormats = new[] { TextureFormat.R8G8B8A8_UNORM },
      DepthStencilFormat = TextureFormat.D24_UNORM_S8_UINT,
      SampleCount = 1,
      SampleQuality = 0
    };

    simpleRenderState = device.CreateRenderState(renderStateDesc, pipelineStateDesc) as DX12RenderState;
  }

  private void CreateAndUploadResources()
  {
    var vertices = new Vertex[]
    {
            new Vertex(new Vector3( 0.0f,  0.5f, 0.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)), // Top (red)
            new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f)), // Bottom-left (green)
            new Vertex(new Vector3( 0.5f, -0.5f, 0.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f))  // Bottom-right (blue)
    };

    var vbDesc = new BufferDescription
    {
      Name = "TriangleVertexBuffer",
      Size = (ulong)(vertices.Length * sizeof(Vertex)),
      BufferUsage = BufferUsage.Vertex,
      BindFlags = BindFlags.VertexBuffer,
      Usage = ResourceUsage.Default,
      Stride = (uint)sizeof(Vertex)
    };

    vertexBuffer = device.CreateBuffer(vbDesc) as DX12Buffer;

    vertexBuffer.SetData(vertices);
    Console.WriteLine($"✅ Uploaded {vertices.Length} vertices to vertex buffer");

    var indices = new ushort[] { 0, 1, 2 };

    var ibDesc = new BufferDescription
    {
      Name = "TriangleIndexBuffer",
      Size = (ulong)(indices.Length * sizeof(ushort)),
      BufferUsage = BufferUsage.Index,
      BindFlags = BindFlags.IndexBuffer,
      Usage = ResourceUsage.Default,
      Stride = sizeof(ushort)
    };

    indexBuffer = device.CreateBuffer(ibDesc) as DX12Buffer;

    indexBuffer.SetData(indices);
    Console.WriteLine($"✅ Uploaded {indices.Length} indices to index buffer");

    var cbDesc = new BufferDescription
    {
      Name = "ConstantBuffer",
      Size = 256,
      BufferUsage = BufferUsage.Constant,
      BindFlags = BindFlags.ConstantBuffer,
      Usage = ResourceUsage.Dynamic,
      CPUAccessFlags = CPUAccessFlags.Write
    };

    constantBuffer = device.CreateBuffer(cbDesc) as DX12Buffer;

    var mvpMatrix = Matrix4x4.Identity;
    UpdateConstantBuffer(mvpMatrix);
    Console.WriteLine("✅ Initialized constant buffer with identity matrix");

    var depthDesc = new TextureDescription
    {
      Name = "DepthBuffer",
      Width = swapChain.Description.Width,
      Height = swapChain.Description.Height,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Format = TextureFormat.D24_UNORM_S8_UINT,
      SampleCount = 1,
      TextureUsage = TextureUsage.DepthStencil,
      BindFlags = BindFlags.DepthStencil,
      Usage = ResourceUsage.Default
    };

    depthTexture = device.CreateTexture(depthDesc) as DX12Texture;
    depthStencilView = depthTexture.CreateView(
        TextureViewDescription.CreateDSV(TextureFormat.D24_UNORM_S8_UINT)) as DX12TextureView;

    Console.WriteLine($"✅ Created depth buffer: {depthDesc.Width}x{depthDesc.Height}");
  }

  public void Render()
  {
    try
    {
      var time = (float)(DateTime.Now.TimeOfDay.TotalSeconds);
      var rotationMatrix = Matrix4x4.CreateRotationZ(time * 0.5f);
      UpdateConstantBuffer(rotationMatrix);

      device.BeginFrame();

      var backBuffer = swapChain.GetCurrentBackBuffer();
      var renderTargetView = backBuffer.GetDefaultRenderTargetView();

      commandBuffer.Begin();

      try
      {
        using var debugScope = commandBuffer.BeginDebugScope("Triangle Render Pass");

        commandBuffer.SetRenderTarget(renderTargetView, depthStencilView);

        var viewport = new Resources.Viewport
        {
          X = 0,
          Y = 0,
          Width = swapChain.Description.Width,
          Height = swapChain.Description.Height,
          MinDepth = 0.0f,
          MaxDepth = 1.0f
        };
        commandBuffer.SetViewport(viewport);

        commandBuffer.ClearRenderTarget(renderTargetView, new Vector4(0.2f, 0.3f, 0.4f, 1.0f));
        commandBuffer.ClearDepthStencil(depthStencilView, ClearFlags.Depth, 1.0f, 0);

        commandBuffer.SetRenderState(simpleRenderState);

        var vertexView = vertexBuffer.GetDefaultShaderResourceView();
        var indexView = indexBuffer.GetDefaultShaderResourceView();
        var constantView = constantBuffer.GetDefaultShaderResourceView();

        commandBuffer.SetVertexBuffer(vertexView, 0);
        commandBuffer.SetIndexBuffer(indexView, IndexFormat.UInt16);
        commandBuffer.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
        commandBuffer.SetConstantBuffer(ShaderStage.Vertex, 0, constantView);

        commandBuffer.DrawIndexed(3, 1, 0, 0, 0);

        if(commandBuffer is DX12CommandBuffer dx12Buffer && backBuffer is DX12Texture dx12BackBuffer)
        {
          dx12Buffer.TransitionBackBufferForPresent(dx12BackBuffer);
        }

        Console.WriteLine("🎨 Drew triangle with 3 indices");
      }
      finally
      {
        commandBuffer.End();
      }

      device.Submit(commandBuffer);
      swapChain.Present();
      device.EndFrame();
    }
    catch(TimeoutException ex)
    {
      Console.WriteLine($"⚠️ GPU Timeout: {ex.Message}");
      try
      {
        device.WaitForGPU();
      }
      catch
      {
        Console.WriteLine("❌ Failed to recover from GPU timeout");
        throw;
      }
    }
    catch(Exception ex)
    {
      Console.WriteLine($"❌ Render error: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
      throw;
    }
  }

  private void UpdateConstantBuffer(Matrix4x4 worldViewProj)
  {
    var mappedPtr = device.MapBuffer(constantBuffer, MapMode.WriteDiscard);

    try
    {
      unsafe
      {
        var matrixPtr = (Matrix4x4*)mappedPtr.ToPointer();
        *matrixPtr = worldViewProj;
      }
    }
    finally
    {
      device.UnmapBuffer(constantBuffer);
    }
  }

  public void Cleanup()
  {
    try
    {
      Console.WriteLine("🧹 Starting cleanup...");

      device?.WaitForGPU();

      depthStencilView?.Dispose();
      depthTexture?.Dispose();
      constantBuffer?.Dispose();
      indexBuffer?.Dispose();
      vertexBuffer?.Dispose();
      simpleRenderState?.Dispose();
      commandBuffer?.Dispose();
      swapChain?.Dispose();
      device?.Dispose();

      Console.WriteLine("✅ Cleanup completed successfully!");
    }
    catch(Exception ex)
    {
      Console.WriteLine($"⚠️ Error during cleanup: {ex.Message}");
      // Не re-throw в cleanup
    }

    Console.WriteLine("🧹 Cleanup completed successfully!");
  }

  private InputLayoutDescription CreateInputLayout()
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
          InputSlotClass = InputClassification.PerVertexData,
          InstanceDataStepRate = 0
        },
        new InputElementDescription
        {
          SemanticName = "COLOR",
          SemanticIndex = 0,
          Format = TextureFormat.R32G32B32A32_FLOAT,
          InputSlot = 0,
          AlignedByteOffset = 12,
          InputSlotClass = InputClassification.PerVertexData,
          InstanceDataStepRate = 0
        }
      }
    };
  }

  /// <summary>
  /// Демонстрация пакетной загрузки данных
  /// </summary>
  public void DemonstrateBatchUpload()
  {
    Console.WriteLine("\n🚀 Demonstrating batch upload capabilities...");

    var textureDescs = new[]
    {
      new TextureDescription
      {
        Name = "DemoTexture1",
        Width = 256, Height = 256, Depth = 1,
        MipLevels = 1, ArraySize = 1,
        Format = TextureFormat.R8G8B8A8_UNORM,
        TextureUsage = TextureUsage.ShaderResource,
        BindFlags = BindFlags.ShaderResource,
        Usage = ResourceUsage.Default
      },
      new TextureDescription
      {
        Name = "DemoTexture2",
        Width = 512, Height = 512, Depth = 1,
        MipLevels = 1, ArraySize = 1,
        Format = TextureFormat.R8G8B8A8_UNORM,
        TextureUsage = TextureUsage.ShaderResource,
        BindFlags = BindFlags.ShaderResource,
        Usage = ResourceUsage.Default
      }
    };

    var textures = textureDescs.Select(desc => device.CreateTexture(desc) as DX12Texture).ToArray();

    var texture1Data = CreateCheckerboardPattern(256, 256);
    var texture2Data = CreateGradientPattern(512, 512);

    device.BatchUploadResources(uploader =>
    {
      uploader.UploadTexture(textures[0], texture1Data, 0, 0);
      uploader.UploadTexture(textures[1], texture2Data, 0, 0);

      Console.WriteLine("  ✅ Uploaded textures in batch operation");
    });

    foreach(var texture in textures)
    {
      texture.Dispose();
    }

    Console.WriteLine("✅ Batch upload demonstration completed!");
  }

  /// <summary>
  /// Демонстрация readback операций
  /// </summary>
  public void DemonstrateReadback()
  {
    Console.WriteLine("\n📥 Demonstrating readback capabilities...");

    var testData = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

    var bufferDesc = new BufferDescription
    {
      Name = "ReadbackTestBuffer",
      Size = (ulong)(testData.Length * sizeof(float)),
      BufferUsage = BufferUsage.Structured,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default,
      Stride = sizeof(float)
    };

    var buffer = device.CreateBuffer(bufferDesc) as DX12Buffer;

    buffer.SetData(testData);
    Console.WriteLine($"  📤 Uploaded data: [{string.Join(", ", testData)}]");

    var readbackData = buffer.GetData<float>(0, -1);
    Console.WriteLine($"  📥 Readback data: [{string.Join(", ", readbackData)}]");

    bool dataMatches = testData.SequenceEqual(readbackData);
    Console.WriteLine($"  ✅ Data integrity: {(dataMatches ? "PASSED" : "FAILED")}");

    buffer.Dispose();
    Console.WriteLine("✅ Readback demonstration completed!");
  }

  private byte[] CreateCheckerboardPattern(uint width, uint height)
  {
    var data = new byte[width * height * 4];
    var index = 0;

    for(uint y = 0; y < height; y++)
    {
      for(uint x = 0; x < width; x++)
      {
        bool isWhite = ((x / 32) + (y / 32)) % 2 == 0;
        byte color = (byte)(isWhite ? 255 : 0);

        data[index++] = color;
        data[index++] = color;
        data[index++] = color;
        data[index++] = 255;
      }
    }

    return data;
  }

  private byte[] CreateGradientPattern(uint width, uint height)
  {
    var data = new byte[width * height * 4];
    var index = 0;

    for(uint y = 0; y < height; y++)
    {
      for(uint x = 0; x < width; x++)
      {
        data[index++] = (byte)(x * 255 / width);
        data[index++] = (byte)(y * 255 / height);
        data[index++] = 128;
        data[index++] = 255;
      }
    }

    return data;
  }
}