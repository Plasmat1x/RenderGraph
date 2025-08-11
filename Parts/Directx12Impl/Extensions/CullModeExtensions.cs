namespace Directx12Impl.Extensions;

public static class CullModeExtensions
{
  public static Silk.NET.Direct3D12.CullMode Convert(this GraphicsAPI.Enums.CullMode _mode) => _mode switch
  {
    GraphicsAPI.Enums.CullMode.None => Silk.NET.Direct3D12.CullMode.None,
    GraphicsAPI.Enums.CullMode.Front => Silk.NET.Direct3D12.CullMode.Front,
    GraphicsAPI.Enums.CullMode.Back => Silk.NET.Direct3D12.CullMode.Back,
    _ => Silk.NET.Direct3D12.CullMode.Back
  };
}