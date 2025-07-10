using Core;

using Resources.Enums;

using System.Numerics;

namespace Passes;

public class RenderableObject
{
  public bool Visible { get; set; } = true;
  public ResourceHandle VertexBuffer { get; set; }
  public ResourceHandle IndexBuffer { get; set; }
  public uint VertexCount { get; set; }
  public uint IndexCount { get; set; }
  public uint InstanceCount { get; set; } = 1;
  public uint VertexStride { get; set; }
  public IndexFormat IndexFormat { get; set; } = IndexFormat.UInt32;
  public Material Material { get; set; }
  public Matrix4x4 WorldMatrix { get; set; } = Matrix4x4.Identity;
}
