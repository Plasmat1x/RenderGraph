using Directx12Impl;

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

  // Vertex структура для нашего треугольника
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
    // 1. Создаем устройство с debug layer
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

    // 3. Создаем Pipeline State
    CreatePipelineState();

    // 4. Создаем Command Buffer
    commandBuffer = device.CreateCommandBuffer(
        CommandBufferType.Direct,
        CommandBufferExecutionMode.Immediate) as DX12CommandBuffer;

    // 5. Создаем ресурсы и загружаем данные
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
    // Создаем простые шейдеры (заглушки для примера)
    var vsDesc = new ShaderDescription
    {
      Name = "SimpleVertexShader",
      Stage = ShaderStage.Vertex,
      ByteCode = CreateMockVertexShaderBytecode(),
      ShaderModel = "5_0",
      EntryPoint = "VSMain"
    };

    var psDesc = new ShaderDescription
    {
      Name = "SimplePixelShader",
      Stage = ShaderStage.Pixel,
      ByteCode = CreateMockPixelShaderBytecode(),
      ShaderModel = "5_0",
      EntryPoint = "PSMain"
    };

    var vertexShader = device.CreateShader(vsDesc);
    var pixelShader = device.CreateShader(psDesc);

    // Создаем render state с pipeline state
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
      DepthStencilFormat = TextureFormat.D32_FLOAT,
      SampleCount = 1,
      SampleQuality = 0
    };

    simpleRenderState = device.CreateRenderState(renderStateDesc, pipelineStateDesc) as DX12RenderState;
  }

  private void CreateAndUploadResources()
  {
    // === Создаем Vertex Buffer ===
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

    // ✅ ТЕПЕРЬ РАБОТАЕТ! Загружаем данные вершин
    vertexBuffer.SetData(vertices);
    Console.WriteLine($"✅ Uploaded {vertices.Length} vertices to vertex buffer");

    // === Создаем Index Buffer ===
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

    // ✅ ТЕПЕРЬ РАБОТАЕТ! Загружаем индексы
    indexBuffer.SetData(indices);
    Console.WriteLine($"✅ Uploaded {indices.Length} indices to index buffer");

    // === Создаем Constant Buffer ===
    var cbDesc = new BufferDescription
    {
      Name = "ConstantBuffer",
      Size = 256, // Выравнено по 256 байт (требование DX12)
      BufferUsage = BufferUsage.Constant,
      BindFlags = BindFlags.ConstantBuffer,
      Usage = ResourceUsage.Dynamic, // Для частых обновлений
      CPUAccessFlags = CPUAccessFlags.Write
    };

    constantBuffer = device.CreateBuffer(cbDesc) as DX12Buffer;

    // ✅ ТЕПЕРЬ РАБОТАЕТ! Инициализируем constant buffer
    var mvpMatrix = Matrix4x4.Identity;
    UpdateConstantBuffer(mvpMatrix);
    Console.WriteLine("✅ Initialized constant buffer with identity matrix");

    // === Создаем Depth Buffer ===
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
    depthStencilView = depthTexture.CreateView(
        TextureViewDescription.CreateDSV(TextureFormat.D32_FLOAT)) as DX12TextureView;

    Console.WriteLine($"✅ Created depth buffer: {depthDesc.Width}x{depthDesc.Height}");
  }

  public void Render()
  {
    // Обновляем матрицы
    var time = (float)(DateTime.Now.TimeOfDay.TotalSeconds);
    var rotationMatrix = Matrix4x4.CreateRotationZ(time * 0.5f);
    UpdateConstantBuffer(rotationMatrix);

    // Начинаем кадр
    device.BeginFrame();

    // Получаем текущий back buffer
    var backBuffer = swapChain.GetCurrentBackBuffer();
    var renderTargetView = backBuffer.GetDefaultRenderTargetView();

    commandBuffer.Begin();

    try
    {
      using var debugScope = commandBuffer.BeginDebugScope("Triangle Render Pass");

      // Устанавливаем render targets
      commandBuffer.SetRenderTarget(renderTargetView, depthStencilView);

      // Устанавливаем viewport
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

      // Очищаем буферы
      commandBuffer.ClearRenderTarget(renderTargetView, new Vector4(0.2f, 0.3f, 0.4f, 1.0f));
      commandBuffer.ClearDepthStencil(depthStencilView, ClearFlags.Depth, 1.0f, 0);

      // Устанавливаем render state
      commandBuffer.SetRenderState(simpleRenderState);

      // Устанавливаем буферы
      var vertexView = vertexBuffer.GetDefaultShaderResourceView();
      var indexView = indexBuffer.GetDefaultShaderResourceView();
      var constantView = constantBuffer.GetDefaultShaderResourceView();

      commandBuffer.SetVertexBuffer(vertexView, 0);
      commandBuffer.SetIndexBuffer(indexView, IndexFormat.R16_UINT);
      commandBuffer.SetConstantBuffer(ShaderStage.Vertex, 0, constantView);

      // Рисуем треугольник
      commandBuffer.DrawIndexed(3, 1, 0, 0, 0);

      Console.WriteLine("🎨 Drew triangle with 3 indices");
    }
    finally
    {
      commandBuffer.End();
    }

    // Выполняем команды
    device.Submit(commandBuffer);

    // Презентуем кадр
    swapChain.Present();

    // Завершаем кадр
    device.EndFrame();
  }

  private void UpdateConstantBuffer(Matrix4x4 worldViewProj)
  {
    // ✅ ТЕПЕРЬ РАБОТАЕТ! Map/Unmap для dynamic constant buffer
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
    // Ждем завершения GPU операций
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

    Console.WriteLine("🧹 Cleanup completed successfully!");
  }

  // === Вспомогательные методы ===

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
                    AlignedByteOffset = 12, // sizeof(Vector3)
                    InputSlotClass = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                }
            }
    };
  }

  private byte[] CreateMockVertexShaderBytecode()
  {
    // В реальной реализации здесь был бы скомпилированный HLSL
    // Для примера используем заглушку
    return new byte[]
    {
            0x44, 0x58, 0x42, 0x43, // DXBC signature
            0x00, 0x00, 0x00, 0x00, // Checksum
            0x01, 0x00, 0x00, 0x00, // Version
            0x08, 0x00, 0x00, 0x00, // Size
                                    // Minimal valid shader bytecode would go here
                                    // For demo purposes, this is just a placeholder
    };
  }

  private byte[] CreateMockPixelShaderBytecode()
  {
    // В реальной реализации здесь был бы скомпилированный HLSL
    return new byte[]
    {
            0x44, 0x58, 0x42, 0x43, // DXBC signature
            0x00, 0x00, 0x00, 0x00, // Checksum
            0x01, 0x00, 0x00, 0x00, // Version
            0x08, 0x00, 0x00, 0x00, // Size
                                    // Minimal valid shader bytecode would go here
    };
  }

  // === Дополнительные demo методы ===

  /// <summary>
  /// Демонстрация пакетной загрузки данных
  /// </summary>
  public void DemonstrateBatchUpload()
  {
    Console.WriteLine("\n🚀 Demonstrating batch upload capabilities...");

    // Создаем несколько текстур для демонстрации
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

    // Создаем тестовые данные
    var texture1Data = CreateCheckerboardPattern(256, 256);
    var texture2Data = CreateGradientPattern(512, 512);

    // Демонстрируем пакетную загрузку
    device.BatchUploadResources(uploader =>
    {
      uploader.UploadTexture(textures[0], texture1Data, 0, 0);
      uploader.UploadTexture(textures[1], texture2Data, 0, 0);

      Console.WriteLine("  ✅ Uploaded textures in batch operation");
    });

    // Cleanup
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

    // Создаем небольшой буфер с тестовыми данными
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

    // Загружаем данные
    buffer.SetData(testData);
    Console.WriteLine($"  📤 Uploaded data: [{string.Join(", ", testData)}]");

    // Читаем данные обратно
    var readbackData = buffer.GetData<float>(0, -1);
    Console.WriteLine($"  📥 Readback data: [{string.Join(", ", readbackData)}]");

    // Проверяем корректность
    bool dataMatches = testData.SequenceEqual(readbackData);
    Console.WriteLine($"  ✅ Data integrity: {(dataMatches ? "PASSED" : "FAILED")}");

    buffer.Dispose();
    Console.WriteLine("✅ Readback demonstration completed!");
  }

  // === Вспомогательные методы для создания тестовых данных ===

  private byte[] CreateCheckerboardPattern(uint width, uint height)
  {
    var data = new byte[width * height * 4]; // RGBA
    var index = 0;

    for(uint y = 0; y < height; y++)
    {
      for(uint x = 0; x < width; x++)
      {
        bool isWhite = ((x / 32) + (y / 32)) % 2 == 0;
        byte color = (byte)(isWhite ? 255 : 0);

        data[index++] = color; // R
        data[index++] = color; // G
        data[index++] = color; // B
        data[index++] = 255;   // A
      }
    }

    return data;
  }

  private byte[] CreateGradientPattern(uint width, uint height)
  {
    var data = new byte[width * height * 4]; // RGBA
    var index = 0;

    for(uint y = 0; y < height; y++)
    {
      for(uint x = 0; x < width; x++)
      {
        data[index++] = (byte)(x * 255 / width);    // R gradient
        data[index++] = (byte)(y * 255 / height);   // G gradient
        data[index++] = 128;                        // B constant
        data[index++] = 255;                        // A opaque
      }
    }

    return data;
  }
}