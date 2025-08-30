using Directx12Impl;

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
  private DX12GraphicsDevice p_device;
  private DX12SwapChain p_swapChain;
  private DX12CommandBuffer p_commandBuffer;
  private DX12RenderState p_simpleRenderState;
  private DX12Buffer p_vertexBuffer;
  private DX12Buffer p_indexBuffer;
  private DX12Buffer p_constantBuffer;
  private DX12Texture p_depthTexture;
  private DX12TextureView p_depthStencilView;

  private struct Vertex
  {
    public Vector3 Position;
    public Vector4 Color;

    public Vertex(Vector3 _position, Vector4 _color)
    {
      Position = _position;
      Color = _color;
    }
  }

  public void Initialize(IntPtr _windowHandle, uint _width, uint _height)
  {
    p_device = new DX12GraphicsDevice(true);

    var swapChainDesc = new SwapChainDescription
    {
      Width = _width,
      Height = _height,
      Format = TextureFormat.R8G8B8A8_UNORM,
      BufferCount = 2,
      SampleCount = 1,
      SwapEffect = SwapEffect.FlipDiscard
    };

    p_swapChain = p_device.CreateSwapChain(swapChainDesc, _windowHandle) as DX12SwapChain;

    CreatePipelineState();

    p_commandBuffer = p_device.CreateCommandBuffer(
        CommandBufferType.Direct,
        CommandBufferExecutionMode.Immediate) as DX12CommandBuffer;

    CreateAndUploadResources();

    Console.WriteLine("✅ DX12 Rendering Example initialized successfully!");
    Console.WriteLine($"   Device: {p_device.Name}");
    Console.WriteLine($"   SwapChain: {_width}x{_height}, Format: {swapChainDesc.Format}");
    Console.WriteLine($"   Vertex Buffer: {p_vertexBuffer.Size} bytes");
    Console.WriteLine($"   Index Buffer: {p_indexBuffer.Size} bytes");
    Console.WriteLine($"   Constant Buffer: {p_constantBuffer.Size} bytes");
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

    var vertexShader = p_device.CreateShader(vsDesc);
    var pixelShader = p_device.CreateShader(psDesc);

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

    p_simpleRenderState = p_device.CreateRenderState(renderStateDesc, pipelineStateDesc) as DX12RenderState;
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

    p_vertexBuffer = p_device.CreateBuffer(vbDesc) as DX12Buffer;

    p_vertexBuffer.SetData(vertices);
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

    p_indexBuffer = p_device.CreateBuffer(ibDesc) as DX12Buffer;

    p_indexBuffer.SetData(indices);
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

    p_constantBuffer = p_device.CreateBuffer(cbDesc) as DX12Buffer;

    var mvpMatrix = Matrix4x4.Identity;
    UpdateConstantBuffer(mvpMatrix);
    Console.WriteLine("✅ Initialized constant buffer with identity matrix");

    var depthDesc = new TextureDescription
    {
      Name = "DepthBuffer",
      Width = p_swapChain.Description.Width,
      Height = p_swapChain.Description.Height,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Format = TextureFormat.D24_UNORM_S8_UINT,
      SampleCount = 1,
      TextureUsage = TextureUsage.DepthStencil,
      BindFlags = BindFlags.DepthStencil,
      Usage = ResourceUsage.Default
    };

    p_depthTexture = p_device.CreateTexture(depthDesc) as DX12Texture;
    p_depthStencilView = p_depthTexture.CreateView(
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

      p_device.BeginFrame();

      var backBuffer = p_swapChain.GetCurrentBackBuffer();
      var renderTargetView = backBuffer.GetDefaultRenderTargetView();

      p_commandBuffer.Begin();

      try
      {
        using var debugScope = p_commandBuffer.BeginDebugScope("Triangle Render Pass");

        p_commandBuffer.SetRenderTarget(renderTargetView, p_depthStencilView);

        var viewport = new Resources.Viewport
        {
          X = 0,
          Y = 0,
          Width = p_swapChain.Description.Width,
          Height = p_swapChain.Description.Height,
          MinDepth = 0.0f,
          MaxDepth = 1.0f
        };
        p_commandBuffer.SetViewport(viewport);

        p_commandBuffer.ClearRenderTarget(renderTargetView, new Vector4(0.2f, 0.3f, 0.4f, 1.0f));
        p_commandBuffer.ClearDepthStencil(p_depthStencilView, ClearFlags.Depth, 1.0f, 0);

        p_commandBuffer.SetRenderState(p_simpleRenderState);

        var vertexView = p_vertexBuffer.GetDefaultShaderResourceView();
        var indexView = p_indexBuffer.GetDefaultShaderResourceView();
        var constantView = p_constantBuffer.GetDefaultShaderResourceView();

        p_commandBuffer.SetVertexBuffer(vertexView, 0);
        p_commandBuffer.SetIndexBuffer(indexView, IndexFormat.UInt16);
        p_commandBuffer.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
        p_commandBuffer.SetConstantBuffer(ShaderStage.Vertex, 0, constantView);

        p_commandBuffer.DrawIndexed(3, 1, 0, 0, 0);

        if(p_commandBuffer is DX12CommandBuffer dx12Buffer && backBuffer is DX12Texture dx12BackBuffer)
        {
          dx12Buffer.TransitionBackBufferForPresent(dx12BackBuffer);
        }

        Console.WriteLine("🎨 Drew triangle with 3 indices");
      }
      finally
      {
        p_commandBuffer.End();
      }

      p_device.Submit(p_commandBuffer);
      p_swapChain.Present();
      p_device.EndFrame();
    }
    catch(TimeoutException ex)
    {
      Console.WriteLine($"⚠️ GPU Timeout: {ex.Message}");
      try
      {
        p_device.WaitForGPU();
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

  private void UpdateConstantBuffer(Matrix4x4 _worldViewProj)
  {
    var mappedPtr = p_device.MapBuffer(p_constantBuffer, MapMode.WriteDiscard);

    try
    {
      unsafe
      {
        var matrixPtr = (Matrix4x4*)mappedPtr.ToPointer();
        *matrixPtr = _worldViewProj;
      }
    }
    finally
    {
      p_device.UnmapBuffer(p_constantBuffer);
    }
  }

  public void Cleanup()
  {
    try
    {
      Console.WriteLine("🧹 Starting cleanup...");

      p_device?.WaitForGPU();

      p_depthStencilView?.Dispose();
      p_depthTexture?.Dispose();
      p_constantBuffer?.Dispose();
      p_indexBuffer?.Dispose();
      p_vertexBuffer?.Dispose();
      p_simpleRenderState?.Dispose();
      p_commandBuffer?.Dispose();
      p_swapChain?.Dispose();
      p_device?.Dispose();

      Console.WriteLine("✅ Cleanup completed successfully!");
    }
    catch(Exception ex)
    {
      Console.WriteLine($"⚠️ Error during cleanup: {ex.Message}");
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

    var textures = textureDescs.Select(_desc => p_device.CreateTexture(_desc) as DX12Texture).ToArray();

    var texture1Data = CreateCheckerboardPattern(256, 256);
    var texture2Data = CreateGradientPattern(512, 512);

    p_device.BatchUploadResources(_uploader => {
      _uploader.UploadTexture(textures[0], texture1Data, 0, 0);
      _uploader.UploadTexture(textures[1], texture2Data, 0, 0);

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

    var buffer = p_device.CreateBuffer(bufferDesc) as DX12Buffer;

    buffer.SetData(testData);
    Console.WriteLine($"  📤 Uploaded data: [{string.Join(", ", testData)}]");

    var readbackData = buffer.GetData<float>(0, -1);
    Console.WriteLine($"  📥 Readback data: [{string.Join(", ", readbackData)}]");

    bool dataMatches = testData.SequenceEqual(readbackData);
    Console.WriteLine($"  ✅ Data integrity: {(dataMatches ? "PASSED" : "FAILED")}");

    buffer.Dispose();
    Console.WriteLine("✅ Readback demonstration completed!");
  }

  private byte[] CreateCheckerboardPattern(uint _width, uint _height)
  {
    var data = new byte[_width * _height * 4];
    var index = 0;

    for(uint y = 0; y < _height; y++)
    {
      for(uint x = 0; x < _width; x++)
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

  private byte[] CreateGradientPattern(uint _width, uint _height)
  {
    var data = new byte[_width * _height * 4];
    var index = 0;

    for(uint y = 0; y < _height; y++)
    {
      for(uint x = 0; x < _width; x++)
      {
        data[index++] = (byte)(x * 255 / _width);
        data[index++] = (byte)(y * 255 / _height);
        data[index++] = 128;
        data[index++] = 255;
      }
    }

    return data;
  }
}