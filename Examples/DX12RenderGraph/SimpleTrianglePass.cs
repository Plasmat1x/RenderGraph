using Core;
using Core.Enums;

using Directx12Impl;
using Directx12Impl.Extensions;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Direct3D12;

using System.Numerics;
/// <summary>
/// Простой пасс для рендеринга треугольника
/// </summary>
public class SimpleTrianglePass: RenderPass
{
  private ResourceHandle _renderTarget;
  private ResourceHandle _vertexBuffer;
  private ResourceHandle _indexBuffer;

  public SimpleTrianglePass(string name) : base(name)
  {
    Category = PassCategory.Rendering;
    Priority = PassPriority.Normal;
    AlwaysExecute = true;
  }

  public void SetRenderTarget(ResourceHandle renderTarget)
  {
    _renderTarget = renderTarget;
  }

  public unsafe override void Setup(RenderGraphBuilder builder)
  {
    Console.WriteLine($"[{Name}] Setup called");

    if(_renderTarget.IsValid())
    {
      builder.WriteTexture(_renderTarget);
    }

    var vertexBufferSize = (ulong)(3 * sizeof(SimpleVertex));
    _vertexBuffer = builder.CreateVertexBuffer("TriangleVertices", vertexBufferSize, (uint)sizeof(SimpleVertex));
    builder.ReadBuffer(_vertexBuffer);

    var indexBufferSize = (ulong)(3 * sizeof(uint));
    _indexBuffer = builder.CreateIndexBuffer("TriangleIndices", indexBufferSize);
    builder.ReadBuffer(_indexBuffer);
  }

  public override void Execute(RenderPassContext context)
  {
    Console.WriteLine($"[{Name}] Execute called");

    if(!_renderTarget.IsValid())
      return;

    var commandBuffer = context.CommandBuffer as DX12CommandBuffer;
    var renderTarget = context.GetTexture(_renderTarget);
    var vertexBuffer = (DX12Buffer)context.GetBuffer(_vertexBuffer);
    var indexBuffer = (DX12Buffer)context.GetBuffer(_indexBuffer);

    if(renderTarget != null && vertexBuffer != null && indexBuffer != null)
    {
      FillBuffers(vertexBuffer, indexBuffer);

      var rtv = renderTarget.GetDefaultRenderTargetView();
      commandBuffer.SetRenderTarget(rtv);

      commandBuffer.SetViewport(new Resources.Viewport
      {
        X = 0, Y = 0,
        Width = renderTarget.Width,
        Height = renderTarget.Height,
        MinDepth = 0.0f,
        MaxDepth = 1.0f
      });

      commandBuffer.SetVertexBuffer(CreateVertexBufferView(vertexBuffer), 0);
      commandBuffer.SetIndexBuffer(CreateIndexBufferView(indexBuffer), IndexFormat.UInt32);

      commandBuffer.DrawIndexed(3, 1, 0, 0, 0);

      Console.WriteLine($"[{Name}] Triangle rendered");
    }
  }

  private IBufferView CreateVertexBufferView(DX12Buffer buffer)
  {
    var desc = new BufferViewDescription
    {
      ViewType = BufferViewType.VertexBuffer,
      FirstElement = 0,
      NumElements = buffer.Size / buffer.Stride,
      StructureByteStride = buffer.Stride
    };

    return buffer.CreateView(desc);
  }

  private IBufferView CreateIndexBufferView(DX12Buffer buffer)
  {
    var desc = new BufferViewDescription
    {
      ViewType = BufferViewType.IndexBuffer,
      FirstElement = 0,
      NumElements = buffer.Size / buffer.Stride,
      StructureByteStride = buffer.Stride
    };

    return buffer.CreateView(desc);
  }

  private void FillBuffers(IBuffer vertexBuffer, IBuffer indexBuffer)
  {
    var vertices = new SimpleVertex[]
    {
            new() { Position = new Vector3(0.0f, 0.5f, 0.0f), Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) },  // Верх - красный
            new() { Position = new Vector3(-0.5f, -0.5f, 0.0f), Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f) }, // Лево - зеленый  
            new() { Position = new Vector3(0.5f, -0.5f, 0.0f), Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f) }   // Право - синий
    };

    var indices = new uint[] { 0, 1, 2 };

    if(vertexBuffer is DX12Buffer dx12VertexBuffer)
    {
      dx12VertexBuffer.SetData(vertices);
    }

    if(indexBuffer is DX12Buffer dx12IndexBuffer)
    {
      dx12IndexBuffer.SetData(indices);
    }
  }
}
