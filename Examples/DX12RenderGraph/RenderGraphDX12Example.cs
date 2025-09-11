using Core;

using Directx12Impl;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Passes;
using Passes.Enums;

using Resources.Enums;

using Silk.NET.Maths;
using Silk.NET.Windowing;

using System.Numerics;
/// <summary>
/// Полный пример использования RenderGraph с DirectX12Impl через абстракции.
/// Демонстрирует использование готовых пассов из пакета Passes и создание 
/// комплексного render pipeline через абстракции GraphicsAPI.
/// </summary>
public unsafe class RenderGraphDX12Example: IDisposable
{
  private DX12GraphicsDevice _device;
  private RenderGraph _renderGraph;
  private DX12SwapChain _swapChain;
  private IWindow _window;

  private GeometryPass _geometryPass;
  private BlurPass _blurPass;
  private ClearPass _clearPass;
  private SimpleTrianglePass _trianglePass;

  private ToneMappingPass _toneMappingPass;

  private bool _isRunning = true;
  private readonly uint _windowWidth = 1280;
  private readonly uint _windowHeight = 720;

  private ResourceHandle _backBufferHandle;

  public RenderGraphDX12Example()
  {
    InitializeWindow();
    InitializeGraphicsDevice();
    InitializeRenderGraph();
  }

  private void InitializeWindow()
  {
    _window = Window.Create(new WindowOptions
    {
      API = Silk.NET.Windowing.GraphicsAPI.Default,
      Size = new Vector2D<int>((int)_windowWidth, (int)_windowHeight),
      Title = "RenderGraph + DirectX12 Example",
      IsVisible = true,
      Position = new Vector2D<int>(100, 100),
    });

    _window.Load += OnWindowLoad;
    _window.Render += OnWindowRender;
    _window.Closing += OnWindowClosing;
    _window.Resize += OnWindowResize;
  }

  private void InitializeGraphicsDevice()
  {
    Console.WriteLine("🚀 Initializing DirectX12 Graphics Device...");

    _device = new DX12GraphicsDevice(true);

    Console.WriteLine($"✅ Graphics Device Created:");
    Console.WriteLine($"   Name: {_device.Name}");
    Console.WriteLine($"   API: {_device.API}");
    Console.WriteLine($"   Max Texture Size: {_device.Capabilities.MaxTexture2DSize}");
  }

  private void InitializeRenderGraph()
  {
    Console.WriteLine("📊 Creating Render Graph...");

    _renderGraph = new RenderGraph(_device);

    Console.WriteLine("✅ Render Graph created successfully!");
  }

  /// <summary>
  /// Создает placeholder для back buffer, который будет заменен реальным во время выполнения
  /// </summary>
  private void CreateBackBufferPlaceholder()
  {
    // Создаем временную текстуру которая будет служить placeholder для back buffer
    var builder = new RenderGraphBuilder(_renderGraph.ResourceManager);
    _backBufferHandle = builder.CreateColorTarget(
        "BackBufferPlaceholder",
        _windowWidth,
        _windowHeight,
        TextureFormat.R8G8B8A8_UNORM
    );

    Console.WriteLine($"✅ Created back buffer placeholder: {_backBufferHandle}");
  }

  /// <summary>
  /// Создает и настраивает все render passes
  /// </summary>
  private void CreateRenderPasses()
  {
    Console.WriteLine("🎨 Creating Render Passes...");

    CreateBackBufferPlaceholder();

    _clearPass = new ClearPass("ScreenClear");
    _clearPass.SetRenderTarget(_backBufferHandle);


    _geometryPass = new GeometryPass
    {
      ViewportWidth = _windowWidth,
      ViewportHeight = _windowHeight,
      ClearColor = true,
      ClearDepth = true,
      ClearColorValue = new Vector4(0.1f, 0.2f, 0.4f, 1.0f),
      ClearDepthValue = 1.0f
    };

    _trianglePass = new SimpleTrianglePass("RenderTriangle");
    _trianglePass.SetRenderTarget(_backBufferHandle);

    _blurPass = new BlurPass
    {
      BlurRadius = 8.0f,
      BlurSigma = 2.5f,
      Quality = BlurQuality.High,
      BlurDirection = BlurDirection.Both
    };

    _toneMappingPass = new ToneMappingPass("ToneMapping")
    {
      Exposure = 1.0f,
      Gamma = 2.2f,
      TonemappingMode = TonemappingMode.ACES
    };

    _toneMappingPass.SetOutputTarget(_backBufferHandle);

    Console.WriteLine($"✅ Created {5} render passes");
  }

  /// <summary>
  /// Настраивает зависимости между пассами и связывает ресурсы
  /// </summary>
  private void SetupPassDependenciesAndResources()
  {
    Console.WriteLine("🔗 Setting up pass dependencies and resource connections...");

    _trianglePass.AddDependency(_clearPass);
    _geometryPass.AddDependency(_trianglePass);
    _blurPass.AddDependency(_geometryPass);
    _toneMappingPass.AddDependency(_blurPass);

    SetupResourceConnections();

    Console.WriteLine("✅ Dependencies and resource connections configured");
  }

  /// <summary>
  /// Настраивает связи ресурсов между пассами через коллбэки
  /// </summary>
  private void SetupResourceConnections()
  {
    _geometryPass.OnPassSetup += (pass) =>
    {
      Console.WriteLine("🔗 Connecting GeometryPass.ColorTarget -> BlurPass.InputTexture");
      _blurPass.InputTexture = _geometryPass.ColorTarget;
    };

    _blurPass.OnPassSetup += (pass) =>
    {
      Console.WriteLine("🔗 Connecting BlurPass.OutputTexture -> ToneMappingPass.InputTexture");
      _toneMappingPass.InputTexture = _blurPass.OutputTexture;
    };
  }

  /// <summary>
  /// Добавляет все пассы в render graph
  /// </summary>
  private void AddPassesToRenderGraph()
  {
    Console.WriteLine("📋 Adding passes to render graph...");

    _renderGraph.AddPass(_clearPass);
    _renderGraph.AddPass(_geometryPass);
    _renderGraph.AddPass(_trianglePass);
    _renderGraph.AddPass(_blurPass);
    _renderGraph.AddPass(_toneMappingPass);

    var passCount = _renderGraph.Passes.Count;
    Console.WriteLine($"✅ Added {passCount} passes to render graph");

    foreach(var pass in _renderGraph.Passes)
    {
      Console.WriteLine($"   • {pass.Name} (Category: {pass.Category}, Priority: {pass.Priority})");
    }
  }

  private void OnWindowLoad()
  {
    Console.WriteLine("🏗️ Window loaded, setting up render pipeline...");

    try
    {
      CreateSwapChain();

      CreateRenderPasses();
      SetupPassDependenciesAndResources();
      AddPassesToRenderGraph();

      CompileRenderGraph();

      Console.WriteLine("🎉 Render pipeline setup completed successfully!");
      Console.WriteLine("🚀 Starting render loop...");
    }
    catch(Exception ex)
    {
      Console.WriteLine($"❌ Error during initialization: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
      _isRunning = false;
    }
  }

  private void OnWindowRender(double deltaTime)
  {
    if(!_isRunning)
      return;

    try
    {
      ExecuteRenderFrame(deltaTime);
    }
    catch(Exception ex)
    {
      Console.WriteLine($"❌ Render error: {ex.Message}");
      // В production коде здесь можно добавить recovery logic
    }
  }

  private void OnWindowClosing()
  {
    Console.WriteLine("👋 Shutting down application...");
    _isRunning = false;
    Dispose();
  }

  private void OnWindowResize(Vector2D<int> newSize)
  {
    if(newSize.X <= 0 || newSize.Y <= 0)
      return;

    Console.WriteLine($"🔄 Window resized to {newSize.X}x{newSize.Y}");

    // В реальном приложении здесь нужно пересоздать SwapChain и обновить пассы
    // Для простоты примера пропускаем эту логику
  }

  private void CreateSwapChain()
  {
    Console.WriteLine("🖼️ Creating SwapChain...");

    var swapChainDesc = new SwapChainDescription
    {
      Width = _windowWidth,
      Height = _windowHeight,
      Format = TextureFormat.R8G8B8A8_UNORM,
      BufferCount = 3,
      SampleCount = 1,
      SwapEffect = SwapEffect.FlipDiscard
    };

    _swapChain = _device.CreateSwapChain(swapChainDesc, _window.Native.DXHandle.GetValueOrDefault()) as DX12SwapChain;

    Console.WriteLine("✅ SwapChain created successfully");
  }

  private void CompileRenderGraph()
  {
    Console.WriteLine("⚙️ Compiling render graph...");

    _renderGraph.Compile();

    Console.WriteLine("✅ Render graph compiled successfully!");

    DisplayExecutionOrder();
  }

  private void DisplayExecutionOrder()
  {
    var executionOrder = _renderGraph.ExecutionOrder;
    Console.WriteLine($"📝 Execution order ({executionOrder.Count} passes):");

    for(int i = 0; i < executionOrder.Count; i++)
    {
      var pass = executionOrder[i];
      Console.WriteLine($"   {i + 1}. {pass.Name}");
    }
  }

  /// <summary>
  /// Выполняет рендеринг одного кадра через RenderGraph
  /// </summary>
  private void ExecuteRenderFrame(double deltaTime)
  {
    var backBuffer = _swapChain.GetCurrentBackBuffer();

    var builder = new RenderGraphBuilder(_renderGraph.ResourceManager);
    var backBufferHandle = builder.ImportTexture("BackBuffer", backBuffer);

    SetupBackBufferTargets(backBufferHandle);

    UpdateFrameData(deltaTime);

    using var commandBuffer = _device.CreateCommandBuffer(
        CommandBufferType.Direct,
        CommandBufferExecutionMode.Immediate
    );

    _renderGraph.Execute(commandBuffer);

    _swapChain.Present(1);
  }

  /// <summary>
  /// Устанавливает back buffer как render target для соответствующих пассов
  /// </summary>
  private void SetupBackBufferTargets(ResourceHandle backBufferHandle)
  {
    _clearPass.SetRenderTarget(backBufferHandle);

    _trianglePass.SetRenderTarget(backBufferHandle);

    _toneMappingPass.SetOutputTarget(backBufferHandle);

    Console.WriteLine($"🔗 Updated render targets with BackBuffer: {backBufferHandle}");

  }

  /// <summary>
  /// Обновляет данные кадра для всех пассов
  /// </summary>
  private void UpdateFrameData(double deltaTime)
  {
    var frameData = _renderGraph.FrameData;

    frameData.DeltaTime = (float)deltaTime;
    frameData.TotalTime += (float)deltaTime;
    frameData.FrameIndex++;

    frameData.ScreenWidth = _windowWidth;
    frameData.ScreenHeight = _windowHeight;

    UpdateCameraData(frameData);
  }

  /// <summary>
  /// Обновляет данные камеры
  /// </summary>
  private void UpdateCameraData(FrameData frameData)
  {
    var time = frameData.TotalTime;
    var cameraDistance = 5.0f;

    var cameraPos = new Vector3(
        MathF.Sin(time * 0.5f) * cameraDistance,
        2.0f,
        MathF.Cos(time * 0.5f) * cameraDistance
    );

    var viewMatrix = Matrix4x4.CreateLookAt(cameraPos, Vector3.Zero, Vector3.UnitY);
    var projMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
        MathF.PI / 4.0f,
        (float)_windowWidth / _windowHeight,
        0.1f,
        100.0f
    );

    frameData.ViewMatrix = viewMatrix;
    frameData.ProjectionMatrix = projMatrix;
  }

  /// <summary>
  /// Запускает приложение
  /// </summary>
  public void Run()
  {
    Console.WriteLine("🎮 Starting RenderGraph + DirectX12 Example Application...");
    _window.Run();
  }

  /// <summary>
  /// Останавливает приложение
  /// </summary>
  public void Stop()
  {
    _isRunning = false;
    _window?.Close();
  }

  public void Dispose()
  {
    Console.WriteLine("🧹 Disposing resources...");

    _renderGraph?.Dispose();
    _swapChain?.Dispose();
    _device?.Dispose();
    _window?.Dispose();

    Console.WriteLine("✅ All resources disposed successfully");
  }
}