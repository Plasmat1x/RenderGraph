using Directx12Impl.Builders;

using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts.Utils;

public static class DX12RootSignatureLayouts
{
  /// <summary>
  /// Простой макет для базового рендеринга без текстур
  /// </summary>
  public static RootSignatureDesc1 CreateBasicGraphics0()
  {
    using var builder = new DX12RootSignatureDescBuilder()
        .AllowInputAssemblerInputLayout()
        // b0 - View/Projection constants
        .AddRootCBV(0, 0, ShaderVisibility.Vertex)
        // b1 - Per-object constants  
        .AddRootCBV(1, 0, ShaderVisibility.All);

    return builder.Build();
  }

  /// <summary>
  /// Простой макет для базового рендеринга
  /// </summary>
  public static RootSignatureDesc1 CreateBasicGraphics()
  {
    using var builder = new DX12RootSignatureDescBuilder()
        .AllowInputAssemblerInputLayout()
        // b0 - View/Projection constants
        .AddRootCBV(0, 0, ShaderVisibility.Vertex)
        // b1 - Per-object constants  
        .AddRootCBV(1, 0, ShaderVisibility.All)
        // t0-t7 - Textures
        .AddDescriptorTableSRV(0, 8, 0, ShaderVisibility.Pixel)
        // s0-s7 - Samplers
        .AddDescriptorTableSamplers(0, 8, 0, ShaderVisibility.Pixel);

    return builder.Build();
  }

  /// <summary>
  /// Макет для compute шейдеров
  /// </summary>
  public static RootSignatureDesc1 CreateBasicCompute()
  {
    using var builder = new DX12RootSignatureDescBuilder()
        // b0 - Compute constants
        .AddRootCBV(0, 0)
        // t0-t7 - Input textures/buffers
        .AddDescriptorTableSRV(0, 8)
        // u0-u7 - Output textures/buffers
        .AddDescriptorTableUAV(0, 8);

    return builder.Build();
  }

  /// <summary>
  /// Макет для постобработки (fullscreen effects)
  /// </summary>
  public static RootSignatureDesc1 CreatePostProcess()
  {
    using var builder = new DX12RootSignatureDescBuilder()
        // 16 32-bit constants directly in root signature
        .AddRoot32BitConstants(16, 0, 0, ShaderVisibility.Pixel)
        // t0 - Input texture
        .AddDescriptorTableSRV(0, 1, 0, ShaderVisibility.Pixel)
        // Static sampler for point sampling
        .AddStaticSampler(0, Filter.MinMagMipPoint)
        // Static sampler for linear sampling
        .AddStaticSampler(1, Filter.MinMagMipLinear);

    return builder.Build();
  }
}