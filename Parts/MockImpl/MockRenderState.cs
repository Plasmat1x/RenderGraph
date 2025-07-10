using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockRenderState: IRenderState
{
  public string Name { get; set; } = "MockRenderState";
  public ResourceType ResourceType => ResourceType.Buffer;
  public bool IsDisposed { get; private set; }
  public RenderStateDescription Description { get; }

  public MockRenderState(RenderStateDescription description)
  {
    Description = description;
  }

  public IntPtr GetNativeHandle() => new IntPtr(54321);
  public ulong GetMemorySize() => 128;

  public void Dispose()
  {
    if(!IsDisposed)
    {
      Console.WriteLine($"    [Resource] Disposed render state");
      IsDisposed = true;
    }
  }
}