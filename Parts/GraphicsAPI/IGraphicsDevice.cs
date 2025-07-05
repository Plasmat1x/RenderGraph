using Resources;

namespace GraphicsAPI;

public interface IGraphicsDevice : IDisposable
{
  ICommandBuffer CreateCommandBuffer();
  ITexture CreateTexture(TextureDescription _desc);
  IBuffer CreateBuffer(BufferDescription _desc);
  void ExecuteCommandBuffer(ICommandBuffer _commandBuffer);
  void Present();
  void WaitForGpu();
}