using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using Resources.Enums;

namespace VulkanImpl;

public class VKRenderState : IRenderState
{
  private readonly RenderStateDescription p_description;
  private bool p_disposed;

  public VKRenderState(RenderStateDescription _description)
  {
    p_description = _description;
  }

  public RenderStateDescription Description => p_description;
  public string Name { get; set; } = "VKRenderState";
  public ResourceType ResourceType => ResourceType.RenderState;
  public bool IsDisposed => p_disposed;
  public ulong GetMemorySize() => 1024;

  public void SetName(string _name)
  {
    Console.WriteLine($"  [Vulkan] SetName render state {_name}");
  }

  public IntPtr GetNativeHandle() => IntPtr.Zero;

  public void Dispose()
  {
    if(p_disposed)
      return;
    Console.WriteLine($"  [Vulkan] Dispose render state");
    p_disposed = true;
  }
}
