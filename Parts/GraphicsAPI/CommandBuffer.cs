using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using Resources;
using Resources.Enums;
using System.Numerics;

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
  public abstract void SetRenderTargets(ITexture[] _colorTargets, ITexture _depthTarget);
  public abstract void ClearRenderTarget(ITexture _target, Vector4 _color);
  public abstract void ClearDepthStencil(ITexture _target, float _depth, byte _stencil);
  public abstract void SetVertexBuffer(IBuffer _buffer, uint _stride);
  public abstract void SetIndexBuffer(IBuffer _buffer, IndexFormat _format);
  public abstract void SetVertexShader(IShader _shader);
  public abstract void SetPixelShader(IShader _shader);
  public abstract void SetShaderResource(int _slot, ITexture _texture);
  public abstract void SetConstantBuffer(int _slot, IBuffer _buffer);
  public abstract void Draw(uint _vertexCount, uint _instanceCount);
  public abstract void DrawFullscreenQuad();
}
