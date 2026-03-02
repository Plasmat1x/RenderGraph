using GraphicsAPI;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace VulkanImpl;

public class VKCommandBuffer : GenericCommandBuffer
{
  private bool p_disposed;

  public VKCommandBuffer(CommandBufferType _type) : base(_type)
  {
  }

  protected override void ExecuteCommand(ICommand _command)
  {
    Console.WriteLine($"  [Vulkan] ExecuteCommand {_command.GetType().Name}");
  }

  public void Begin()
  {
    Console.WriteLine($"  [Vulkan] Begin command buffer");
    p_isRecording = true;
  }

  public void End()
  {
    Console.WriteLine($"  [Vulkan] End command buffer");
    p_isRecording = false;
  }

  public void Reset()
  {
    Console.WriteLine($"  [Vulkan] Reset command buffer");
    p_isRecording = false;
    p_commands.Clear();
  }

  public void SetRenderTargets(ITextureView[] _colorTargets, ITextureView _depthTarget)
  {
    Console.WriteLine($"  [Vulkan] SetRenderTargets");
  }

  public void SetShader(IShader _shader)
  {
    Console.WriteLine($"  [Vulkan] SetShader");
  }

  public void SetVertexBuffer(IBuffer _buffer, uint _slot = 0, ulong _offset = 0)
  {
    Console.WriteLine($"  [Vulkan] SetVertexBuffer");
  }

  public void SetIndexBuffer(IBuffer _buffer, IndexFormat _format, ulong _offset = 0)
  {
    Console.WriteLine($"  [Vulkan] SetIndexBuffer");
  }

  public void Draw(uint _vertexCount, uint _instanceCount = 1, uint _firstVertex = 0, uint _firstInstance = 0)
  {
    Console.WriteLine($"  [Vulkan] Draw {_vertexCount} vertices");
  }

  public void DrawIndexed(uint _indexCount, uint _instanceCount = 1, uint _firstIndex = 0, int _vertexOffset = 0, uint _firstInstance = 0)
  {
    Console.WriteLine($"  [Vulkan] DrawIndexed {_indexCount} indices");
  }

  public void SetViewport(Viewport _viewport)
  {
    Console.WriteLine($"  [Vulkan] SetViewport");
  }

  public void SetScissorRect(Rectangle _rect)
  {
    Console.WriteLine($"  [Vulkan] SetScissorRect");
  }

  public void ClearRenderTarget(ITextureView _target, Vector4 _color)
  {
    Console.WriteLine($"  [Vulkan] ClearRenderTarget");
  }

  public void ClearDepthStencil(ITextureView _target, float _depth = 1.0f, byte _stencil = 0)
  {
    Console.WriteLine($"  [Vulkan] ClearDepthStencil");
  }

  public void CopyBuffer(IBuffer _src, IBuffer _dst, ulong _size, ulong _srcOffset = 0, ulong _dstOffset = 0)
  {
    Console.WriteLine($"  [Vulkan] CopyBuffer");
  }

  public void CopyTexture(ITexture _src, ITexture _dst)
  {
    Console.WriteLine($"  [Vulkan] CopyTexture");
  }

  public void Dispatch(uint _x, uint _y, uint _z)
  {
    Console.WriteLine($"  [Vulkan] Dispatch {_x},{_y},{_z}");
  }

  public void SetPushConstant<T>(T _data, ShaderStage _stages) where T : unmanaged
  {
    Console.WriteLine($"  [Vulkan] SetPushConstant");
  }

  public void BeginDebugGroup(string _name)
  {
    Console.WriteLine($"  [Vulkan] BeginDebugGroup {_name}");
  }

  public void EndDebugGroup()
  {
    Console.WriteLine($"  [Vulkan] EndDebugGroup");
  }

  public void InsertDebugMarker(string _name)
  {
    Console.WriteLine($"  [Vulkan] InsertDebugMarker {_name}");
  }

  public bool IsRecording => p_isRecording;
  public int CommandCount => p_commands.Count;

  public IntPtr GetNativeHandle() => IntPtr.Zero;

  public void Dispose()
  {
    if(p_disposed)
      return;
    Console.WriteLine($"  [Vulkan] Dispose command buffer");
    p_disposed = true;
  }
}
