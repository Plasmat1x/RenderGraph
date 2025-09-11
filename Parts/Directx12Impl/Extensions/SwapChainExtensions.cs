using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl.Extensions;
/// <summary>
/// Расширения для работы с SwapChain
/// </summary>
public static class SwapChainExtensions
{
  /// <summary>
  /// Получить соотношение сторон SwapChain
  /// </summary>
  public static float GetAspectRatio(this ISwapChain _swapChain)
  {
    var desc = _swapChain.Description;
    return (float)desc.Width / desc.Height;
  }

  /// <summary>
  /// Проверить, поддерживает ли SwapChain VRR (Variable Refresh Rate)
  /// </summary>
  public static bool SupportsVariableRefreshRate(this DX12SwapChain _swapChain)
  {
    return (_swapChain.Description.Flags & GraphicsAPI.Enums.SwapChainFlags.AllowTearing) != 0;
  }

  /// <summary>
  /// Получить рекомендуемое количество буферов для производительности
  /// </summary>
  public static uint GetRecommendedBufferCount(this SwapChainDescription _description, bool _enableVSync = true)
  {
    if(_enableVSync)
      return 2;

    return 3;
  }

  /// <summary>
  /// Создать оптимальное описание SwapChain для игр
  /// </summary>
  public static SwapChainDescription CreateGameOptimized(uint _width, uint _height, bool _enableHDR = false)
  {
    return new SwapChainDescription
    {
      Width = _width,
      Height = _height,
      Format = _enableHDR
            ? Resources.Enums.TextureFormat.R10G10B10A2_UNORM
            : Resources.Enums.TextureFormat.R8G8B8A8_UNORM,
      BufferCount = 3,
      SampleCount = 1,
      SampleQuality = 0,
      SwapEffect = GraphicsAPI.Enums.SwapEffect.FlipDiscard,
      Flags = GraphicsAPI.Enums.SwapChainFlags.AllowTearing,
      Scaling = GraphicsAPI.Enums.ScalingMode.None,
      AlphaMode = GraphicsAPI.Enums.AlphaMode.Ignore
    };
  }

  /// <summary>
  /// Создать описание SwapChain для приложений (не игр)
  /// </summary>
  public static SwapChainDescription CreateApplicationOptimized(uint _width, uint _height)
  {
    return new SwapChainDescription
    {
      Width = _width,
      Height = _height,
      Format = Resources.Enums.TextureFormat.R8G8B8A8_UNORM,
      BufferCount = 2,
      SampleCount = 1,
      SampleQuality = 0,
      SwapEffect = GraphicsAPI.Enums.SwapEffect.FlipDiscard,
      Flags = GraphicsAPI.Enums.SwapChainFlags.None,
      Scaling = GraphicsAPI.Enums.ScalingMode.None,
      AlphaMode = GraphicsAPI.Enums.AlphaMode.Ignore
    };
  }
}
