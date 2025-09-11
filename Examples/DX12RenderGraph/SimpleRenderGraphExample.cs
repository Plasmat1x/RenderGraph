using Core;

using Directx12Impl;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Resources.Enums;

using Silk.NET.Maths;
using Silk.NET.Windowing;
/// <summary>
/// Простой пример использования RenderGraph с DirectX12
/// </summary>
public unsafe class SimpleRenderGraphExample: IDisposable
{
  private DX12GraphicsDevice _device;
  private RenderGraph _renderGraph;
  private DX12SwapChain _swapChain;
  private IWindow _window;

  private SimpleTrianglePass _trianglePass;
  private ClearPass _clearPass;

  public SimpleRenderGraphExample()
  {
    _window = Window.Create(new WindowOptions
    {
      API = Silk.NET.Windowing.GraphicsAPI.Default,
      Size = new Vector2D<int>(800, 600),
      Title = "Simple RenderGraph + DirectX12",
      IsVisible = true,
      Position = new Vector2D<int>(200, 200)
    });

    _device = new DX12GraphicsDevice(true);
  }

  private void CreateRenderPasses()
  {
    _clearPass = new ClearPass("ClearScreen");
    _trianglePass = new SimpleTrianglePass("RenderTriangle");

    _trianglePass.AddDependency(_clearPass);

    _renderGraph.AddPass(_clearPass);
    _renderGraph.AddPass(_trianglePass);
  }

  public void Run()
  {
    _window.Load += OnLoad;
    _window.Render += OnRender;
    _window.Closing += OnClosing;

    _window.Run();
  }

  private void OnLoad()
  {

    // Создаем swap chain
    var swapChainDesc = new SwapChainDescription
    {
      Width = 800,
      Height = 600,
      Format = TextureFormat.R8G8B8A8_UNORM,
      BufferCount = 2,
      SampleCount = 1,
      SwapEffect = SwapEffect.FlipDiscard
    };

    _swapChain = _device.CreateSwapChain(swapChainDesc, _window.Native.DXHandle.GetValueOrDefault()) as DX12SwapChain;

    _renderGraph = new RenderGraph(_device);

    CreateRenderPasses();

    _renderGraph.Compile();

    Console.WriteLine("✅ Simple RenderGraph example initialized!");

    Console.WriteLine("🚀 Starting render loop...");
  }

  private void OnRender(double deltaTime)
  {
    try
    {
      var backBuffer = _swapChain.GetCurrentBackBuffer();

      var builder = new RenderGraphBuilder(_renderGraph.ResourceManager);
      var backBufferHandle = builder.ImportTexture("BackBuffer", backBuffer);

      _clearPass.SetRenderTarget(backBufferHandle);
      _trianglePass.SetRenderTarget(backBufferHandle);

      using var commandBuffer = _device.CreateCommandBuffer(
          CommandBufferType.Direct,
          CommandBufferExecutionMode.Immediate
      );

      _renderGraph.Execute(commandBuffer);
      _swapChain.Present(1);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Render error: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
  }

  private void OnClosing()
  {
    Console.WriteLine("👋 Shutting down...");
    Dispose();
  }

  public void Dispose()
  {
    _renderGraph?.Dispose();
    _swapChain?.Dispose();
    _device?.Dispose();
    _window?.Dispose();
  }
}
