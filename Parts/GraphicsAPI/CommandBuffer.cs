using Core.Interfaces;

using Resources;
using Resources.Enums;

namespace GraphicsAPI;

public abstract class CommandBuffer
{
  public bool IsRecording { get; }
  public abstract void Begin();
  public abstract void End();
  public abstract void SetViewport(Viewport _viewport);
  public abstract void SetScissorRect(Rectangle _rect);
  public abstract void DrawIndexed(uint _indexCount, uint _instanceCount);
  public abstract void Dispatch(uint _groupCountX, uint _groupCountY, uint _groupCountZ);
  public abstract void CopyTexture(ITexture _src, ITexture _dst);
  public abstract void TrasinitionResource(IResource _resource, ResourceState _state);
}
