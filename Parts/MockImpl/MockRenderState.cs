using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockRenderState: IRenderState
{
  public MockRenderState(RenderStateDescription _description)
  {
    Description = _description;
  }

  public string Name { get; set; } = "MockRenderState";
  public ResourceType ResourceType => ResourceType.Buffer;
  public bool IsDisposed { get; private set; }
  public RenderStateDescription Description { get; }

  public IntPtr GetNativeHandle() => new IntPtr(54321);
  public ulong GetMemorySize() => 128;

  public void Dispose()
  {
    if(IsDisposed)
      return;

    Console.WriteLine($"    [Resource] Disposed render state");
    IsDisposed = true;

  }
}