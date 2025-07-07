using Resources;
using Resources.Enums;

namespace Core.Extensions;

public static class RenderGraphBuilderExtensions
{
  public static ResourceHandle CreateColorTarget(this RenderGraphBuilder _builder, string _name, uint _width, uint _height, TextureFormat _format = TextureFormat.R8G8B8A8_UNORM)
  {
    var desc = new TextureDescription
    {
      Name = _name,
      Width = _width,
      Height = _height,
      Format = _format,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Usage = TextureUsage.ResolveTarget,
      BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource
    };

    return _builder.CreateTexture(_name, desc);
  }

  public static ResourceHandle CreateDepthTarget(this RenderGraphBuilder _builder, string _name, uint _width, uint _height, TextureFormat _format = TextureFormat.D32_FLOAT)
  {
    var desc = new TextureDescription
    {
      Name = _name,
      Width = _width,
      Height = _height,
      Format = _format,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Usage = TextureUsage.DepthStencil,
      BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource
    };

    return _builder.CreateTexture(_name, desc);
  }

  public static ResourceHandle CreateVertexBuffer(this RenderGraphBuilder _builder, string _name, ulong _size, uint _stride)
  {
    var desc = new BufferDescription
    {
      Name = _name,
      Size = _size,
      Stride = _stride,
      Usage = BufferUsage.Vertex,
      BindFlags = BindFlags.VertexBuffer,
    };

    return _builder.CreateBuffer(_name, desc);
  }

  public static ResourceHandle CreateIndexBuffer(this RenderGraphBuilder _builder, string _name, ulong _size)
  {
    var desc = new BufferDescription
    {
      Name = _name,
      Size = _size,
      Stride = sizeof(uint),
      Usage = BufferUsage.Index,
      BindFlags = BindFlags.IndexBuffer,
    };

    return _builder.CreateBuffer(_name, desc);
  }

  public static ResourceHandle CreateConstantBuffer(this RenderGraphBuilder _builder, string _name, ulong _size)
  {
    var desc = new BufferDescription
    {
      Name = _name,
      Size = _size,
      Stride = 0,
      Usage = BufferUsage.Constant,
      BindFlags = BindFlags.ConstantBuffer,
    };

    return _builder.CreateBuffer(_name, desc);
  }
}