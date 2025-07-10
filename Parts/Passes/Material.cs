using Core;

using GraphicsAPI.Interfaces;

namespace Passes;

public class Material
{
  public IShader VertexShader { get; set; }
  public IShader PixelShader { get; set; }
  public List<ResourceHandle> Textures { get; set; } = new();
  public List<ISampler> Samplers { get; set; } = new();
  public ResourceHandle ConstantBuffer { get; set; }
  public string Name { get; set; } = string.Empty;
}