using Resources;

namespace GraphicsAPI.Interfaces;

public interface IGraphicsDevice : IDisposable
{
  CommandBuffer CreateCommandBuffer();
  ITexture CreateTexture(TextureDescription _desc);
  IBuffer CreateBuffer(BufferDescription _desc);
  void ExecuteCommandBuffer(CommandBuffer _commandBuffer);
  void Present();
  void WaitForGpu();
}