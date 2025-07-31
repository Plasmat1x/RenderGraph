using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using MockImpl;

using Resources;
using Resources.Enums;

using System.Numerics;
/// <summary>
/// Демонстрация использования командного буфера
/// </summary>
public static class CommandBufferExample
{
  public static void RunExample()
  {
    Console.WriteLine("=== Command Buffer Example ===\n");

    // Создаем mock устройство
    var device = new MockGraphicsDevice();

    // Создаем командный буфер
    using var commandBuffer = new ExampleCommandBuffer(device, CommandBufferType.Direct);

    // Демонстрируем базовое использование
    BasicUsageExample(commandBuffer, device);

    Console.WriteLine();

    // Демонстрируем продвинутые функции
    AdvancedFeaturesExample(commandBuffer, device);

    Console.WriteLine();

    // Демонстрируем оптимизацию
    OptimizationExample(commandBuffer, device);
  }

  private static void BasicUsageExample(ExampleCommandBuffer _commandBuffer, MockGraphicsDevice _device)
  {
    Console.WriteLine("--- Basic Usage Example ---");

    // Создаем ресурсы
    var colorTexture = _device.CreateTexture(new TextureDescription
    {
      Width = 1920,
      Height = 1080,
      Format = TextureFormat.R8G8B8A8_UNORM,
      TextureUsage = TextureUsage.RenderTarget,
      BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
      Usage = ResourceUsage.Default,
      Name = "ColorBuffer"
    });

    var depthTexture = _device.CreateTexture(new TextureDescription
    {
      Width = 1920,
      Height = 1080,
      Format = TextureFormat.D24_UNORM_S8_UINT,
      TextureUsage = TextureUsage.DepthStencil,
      BindFlags = BindFlags.DepthStencil,
      Usage = ResourceUsage.Default,
      Name = "DepthBuffer"
    });

    var vertexShader = _device.CreateShader(new ShaderDescription
    {
      Stage = ShaderStage.Vertex,
      Name = "VertexShader",
      EntryPoint = "main"
    });

    var pixelShader = _device.CreateShader(new ShaderDescription
    {
      Stage = ShaderStage.Pixel,
      Name = "PixelShader",
      EntryPoint = "main"
    });

    // Начинаем запись команд
    _commandBuffer.Begin();

    try
    {
      // Настройка render targets
      var colorView = colorTexture.GetDefaultRenderTargetView();
      var depthView = depthTexture.GetDefaultDepthStencilView();
      _commandBuffer.SetRenderTarget(colorView, depthView);

      // Настройка viewport
      _commandBuffer.SetViewport(new Viewport
      {
        X = 0, Y = 0,
        Width = 1920, Height = 1080,
        MinDepth = 0.0f, MaxDepth = 1.0f
      });

      // Очистка буферов
      _commandBuffer.ClearRenderTarget(colorView, new Vector4(0.2f, 0.3f, 0.8f, 1.0f)); // Синий фон
      _commandBuffer.ClearDepthStencil(depthView);

      // Настройка шейдеров
      _commandBuffer.SetVertexShader(vertexShader);
      _commandBuffer.SetPixelShader(pixelShader);

      // Настройка topology
      _commandBuffer.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

      // Рисование
      _commandBuffer.Draw(3, 1); // Fullscreen triangle

      Console.WriteLine($"Commands recorded: {_commandBuffer.CommandCount}");
    }
    finally
    {
      // Завершаем запись
      _commandBuffer.End();
    }

    // Выполняем команды
    _commandBuffer.Execute();

    // Получаем статистику
    var stats = _commandBuffer.GetStats();
    Console.WriteLine($"Stats: {stats}");
  }

  private static void AdvancedFeaturesExample(ExampleCommandBuffer _commandBuffer, MockGraphicsDevice _device)
  {
    Console.WriteLine("--- Advanced Features Example ---");

    // Создаем compute shader и ресурсы для compute
    var computeShader = _device.CreateShader(new ShaderDescription
    {
      Stage = ShaderStage.Compute,
      Name = "BlurComputeShader",
      EntryPoint = "main"
    });

    var inputTexture = _device.CreateTexture(new TextureDescription
    {
      Width = 512,
      Height = 512,
      Format = TextureFormat.R8G8B8A8_UNORM,
      TextureUsage = TextureUsage.ShaderResource | TextureUsage.UnorderedAccess,
      BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
      Usage = ResourceUsage.Default,
      Name = "InputTexture"
    });

    var outputTexture = _device.CreateTexture(new TextureDescription
    {
      Width = 512,
      Height = 512,
      Format = TextureFormat.R8G8B8A8_UNORM,
      TextureUsage = TextureUsage.ShaderResource | TextureUsage.UnorderedAccess,
      BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
      Usage = ResourceUsage.Default,
      Name = "OutputTexture"
    });

    var constantBuffer = _device.CreateBuffer(new BufferDescription
    {
      Size = 256,
      BufferUsage = BufferUsage.Constant,
      BindFlags = BindFlags.ConstantBuffer,
      Usage = ResourceUsage.Dynamic,
      CPUAccessFlags = CPUAccessFlags.Write,
      Name = "BlurParams"
    });

    _commandBuffer.Begin();

    try
    {
      // Используем debug scope
      using var debugScope = _commandBuffer.BeginDebugScope("Blur Pass");

      // Transition resources
      _commandBuffer.TransitionResource(inputTexture, ResourceState.ShaderResource);
      _commandBuffer.TransitionResource(outputTexture, ResourceState.UnorderedAccess);

      // Setup compute pipeline
      _commandBuffer.SetComputeShader(computeShader);

      // Bind resources
      var inputView = inputTexture.GetDefaultShaderResourceView();
      var outputView = outputTexture.GetDefaultUnorderedAccessView();
      var constantView = constantBuffer.GetDefaultShaderResourceView();

      _commandBuffer.SetShaderResource(ShaderStage.Compute, 0, inputView);
      _commandBuffer.SetUnorderedAccess(ShaderStage.Compute, 0, outputView);
      _commandBuffer.SetConstantBuffer(ShaderStage.Compute, 0, constantView);

      // Dispatch compute work
      var groupsX = (inputTexture.Description.Width + 7) / 8;
      var groupsY = (inputTexture.Description.Height + 7) / 8;
      _commandBuffer.Dispatch(groupsX, groupsY, 1);

      // UAV Barrier
      _commandBuffer.UAVBarrier(outputTexture);

      // Copy result back for further processing
      _commandBuffer.TransitionResource(outputTexture, ResourceState.CopySource);
      _commandBuffer.TransitionResource(inputTexture, ResourceState.CopyDest);
      _commandBuffer.CopyTexture(outputTexture, inputTexture);

      Console.WriteLine($"Advanced commands recorded: {_commandBuffer.CommandCount}");
    }
    finally
    {
      _commandBuffer.End();
    }

    // Execute and show stats
    _commandBuffer.Execute();
    var stats = _commandBuffer.GetStats();
    Console.WriteLine($"Advanced Stats: {stats}");
  }

  private static void OptimizationExample(ExampleCommandBuffer _commandBuffer, MockGraphicsDevice _device)
  {
    Console.WriteLine("--- Optimization Example ---");

    var shader1 = _device.CreateShader(new ShaderDescription { Stage = ShaderStage.Vertex, Name = "Shader1" });
    var shader2 = _device.CreateShader(new ShaderDescription { Stage = ShaderStage.Vertex, Name = "Shader2" });

    _commandBuffer.Begin();

    try
    {
      // Записываем много избыточных state changes
      _commandBuffer.SetVertexShader(shader1);
      _commandBuffer.SetVertexShader(shader2);  // Overrides previous
      _commandBuffer.SetVertexShader(shader1);  // Overrides again

      _commandBuffer.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
      _commandBuffer.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);  // Overrides
      _commandBuffer.SetPrimitiveTopology(PrimitiveTopology.TriangleList);   // Back to original

      // Actual draw call
      _commandBuffer.Draw(3, 1);

      // More redundant state changes
      _commandBuffer.SetVertexShader(shader2);
      _commandBuffer.SetVertexShader(shader1);  // Will be optimized out if no draw follows

      Console.WriteLine($"Commands before optimization: {_commandBuffer.CommandCount}");
    }
    finally
    {
      _commandBuffer.End();
    }

    // Show commands before optimization
    Console.WriteLine("Commands before optimization:");
    for(int i = 0; i < _commandBuffer.Commands.Count; i++)
    {
      var cmd = _commandBuffer.Commands[i];
      Console.WriteLine($"  {i}: {cmd.Type} - {cmd}");
    }

    // Optimize
    _commandBuffer.Optimize();

    Console.WriteLine($"\nCommands after optimization: {_commandBuffer.CommandCount}");
    Console.WriteLine("Commands after optimization:");
    for(int i = 0; i < _commandBuffer.Commands.Count; i++)
    {
      var cmd = _commandBuffer.Commands[i];
      Console.WriteLine($"  {i}: {cmd.Type} - {cmd}");
    }

    // Execute optimized commands
    _commandBuffer.Execute();
  }

  /// <summary>
  /// Демонстрация использования методов-удобностей из базового класса
  /// </summary>
  public static void ConvenienceMethodsExample()
  {
    Console.WriteLine("\n--- Convenience Methods Example ---");

    var device = new MockGraphicsDevice();
    using var commandBuffer = new MockCommandBuffer(CommandBufferType.Direct);

    var colorTexture = device.CreateTexture(new TextureDescription
    {
      Width = 800,
      Height = 600,
      Format = TextureFormat.R8G8B8A8_UNORM,
      TextureUsage = TextureUsage.RenderTarget,
      BindFlags = BindFlags.RenderTarget,
      Usage = ResourceUsage.Default,
      Name = "MainRenderTarget"
    });

    var depthTexture = device.CreateTexture(new TextureDescription
    {
      Width = 800,
      Height = 600,
      Format = TextureFormat.D24_UNORM_S8_UINT,
      TextureUsage = TextureUsage.DepthStencil,
      BindFlags = BindFlags.DepthStencil,
      Usage = ResourceUsage.Default,
      Name = "MainDepthBuffer"
    });

    commandBuffer.Begin();

    try
    {
      // Используем convenience methods
      commandBuffer.SetRenderTarget(colorTexture, depthTexture);  // Из текстур напрямую
      commandBuffer.SetViewportFullTexture(colorTexture);         // Viewport на всю текстуру
      commandBuffer.SetScissorRectFullTexture(colorTexture);      // Scissor на всю текстуру

      // Очистка стандартными значениями
      var colorView = colorTexture.GetDefaultRenderTargetView();
      var depthView = depthTexture.GetDefaultDepthStencilView();

      commandBuffer.ClearRenderTarget(colorView);  // Черный цвет по умолчанию
      commandBuffer.ClearDepthStencil(depthView);  // 1.0f depth, 0 stencil

      // Рисование fullscreen quad
      commandBuffer.DrawFullscreenQuad();

      // Batch операции
      var texture1 = device.CreateTexture(new TextureDescription
      {
        Width = 256, Height = 256,
        Format = TextureFormat.R8G8B8A8_UNORM,
        TextureUsage = TextureUsage.ShaderResource,
        BindFlags = BindFlags.ShaderResource,
        Usage = ResourceUsage.Default,
        Name = "Texture1"
      });
      var texture2 = device.CreateTexture(new TextureDescription
      {
        Width = 256, Height = 256,
        Format = TextureFormat.R8G8B8A8_UNORM,
        TextureUsage = TextureUsage.ShaderResource,
        BindFlags = BindFlags.ShaderResource,
        Usage = ResourceUsage.Default,
        Name = "Texture2"
      });

      commandBuffer.SetShaderResourceBatch(ShaderStage.Pixel, 0,
        texture1.GetDefaultShaderResourceView(),
        texture2.GetDefaultShaderResourceView());

      Console.WriteLine("Convenience methods demo completed");
    }
    finally
    {
      commandBuffer.End();
    }
  }

  /// <summary>
  /// Демонстрация работы с Mock реализацией
  /// </summary>
  public static void MockImplementationExample()
  {
    Console.WriteLine("\n--- Mock Implementation Example ---");

    var device = new MockGraphicsDevice();
    using var commandBuffer = device.CreateCommandBuffer(CommandBufferType.Direct);

    // Создаем простые ресурсы
    var vertexBuffer = device.CreateBuffer(new BufferDescription
    {
      Size = 1024,
      BufferUsage = BufferUsage.Vertex,
      BindFlags = BindFlags.VertexBuffer,
      Usage = ResourceUsage.Default,
      Name = "VertexBuffer"
    });

    var indexBuffer = device.CreateBuffer(new BufferDescription
    {
      Size = 512,
      BufferUsage = BufferUsage.Index,
      BindFlags = BindFlags.IndexBuffer,
      Usage = ResourceUsage.Default,
      Name = "IndexBuffer"
    });

    commandBuffer.Begin();

    try
    {
      // Настройка buffers
      var vertexView = vertexBuffer.GetDefaultShaderResourceView();
      var indexView = indexBuffer.GetDefaultShaderResourceView();

      commandBuffer.SetVertexBuffer(vertexView, 0);
      commandBuffer.SetIndexBuffer(indexView, IndexFormat.UInt16);

      // Множественные draw calls с разными параметрами
      commandBuffer.DrawIndexed(36, 1, 0, 0, 0);    // Куб
      commandBuffer.DrawIndexed(6, 1, 36, 0, 0);    // Квад
      commandBuffer.Draw(3, 1, 0, 0);               // Треугольник

      // Compute dispatch
      commandBuffer.Dispatch(8, 8, 1);

      Console.WriteLine("Mock implementation demo completed");
    }
    finally
    {
      commandBuffer.End();
    }

    // Выполняем через device
    device.ExecuteCommandBuffer(commandBuffer);
  }
}
