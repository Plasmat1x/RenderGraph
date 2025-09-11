using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl.Parts;
/// <summary>
/// Утилиты для отладки SwapChain
/// </summary>
public unsafe static class DX12SwapChainDebugger
{
  /// <summary>
  /// Выводит подробную информацию о SwapChain в консоль
  /// </summary>
  public static void LogSwapChainInfo(IDXGISwapChain3* _swapChain, string _prefix = "[SwapChain]")
  {
    SwapChainDesc1 desc;
    HResult hr = _swapChain->GetDesc1(&desc);

    if(hr.IsSuccess)
    {
      Console.WriteLine($"{_prefix} SwapChain Information:");
      Console.WriteLine($"{_prefix}   Size: {desc.Width}x{desc.Height}");
      Console.WriteLine($"{_prefix}   Format: {desc.Format}");
      Console.WriteLine($"{_prefix}   BufferCount: {desc.BufferCount}");
      Console.WriteLine($"{_prefix}   SwapEffect: {desc.SwapEffect}");
      Console.WriteLine($"{_prefix}   SampleCount: {desc.SampleDesc.Count}");
      Console.WriteLine($"{_prefix}   SampleQuality: {desc.SampleDesc.Quality}");
      Console.WriteLine($"{_prefix}   Scaling: {desc.Scaling}");
      Console.WriteLine($"{_prefix}   AlphaMode: {desc.AlphaMode}");
      Console.WriteLine($"{_prefix}   Flags: {desc.Flags}");
    }

    int isFullscreen;
    IDXGIOutput* output;
    hr = _swapChain->GetFullscreenState(&isFullscreen, &output);

    if(hr.IsSuccess)
    {
      Console.WriteLine($"{_prefix}   Fullscreen: {isFullscreen != 0}");

      if(output != null)
      {
        OutputDesc outputDesc;
        hr = output->GetDesc(&outputDesc);
        if(hr.IsSuccess)
        {
          Console.WriteLine($"{_prefix} Output: {new string (outputDesc.DeviceName)}");
        }
        output->Release();
      }
    }

    var currentIndex = _swapChain->GetCurrentBackBufferIndex();
    Console.WriteLine($"{_prefix}   Current BackBuffer Index: {currentIndex}");

    for(uint i = 0; i < desc.BufferCount; i++)
    {
      ID3D12Resource* backBuffer;
      var riid = ID3D12Resource.Guid;
      hr = _swapChain->GetBuffer(i, &riid, (void**)&backBuffer);

      if(hr.IsSuccess)
      {
        var resourceDesc = backBuffer->GetDesc();
        Console.WriteLine($"{_prefix}   BackBuffer {i}: {resourceDesc.Width}x{resourceDesc.Height}, Format: {resourceDesc.Format}");
        backBuffer->Release();
      }
    }
  }

  /// <summary>
  /// Проверяет корректность настроек SwapChain
  /// </summary>
  public static bool ValidateSwapChainSettings(SwapChainDesc1* _desc, out List<string> _issues)
  {
    _issues = new List<string>();

    // Проверка размеров
    if(_desc->Width == 0 || _desc->Height == 0)
    {
      _issues.Add("SwapChain size cannot be zero");
    }

    if(_desc->Width > 16384 || _desc->Height > 16384)
    {
      _issues.Add("SwapChain size exceeds reasonable limits (16384x16384)");
    }

    if(_desc->BufferCount < 2)
    {
      _issues.Add("BufferCount should be at least 2 for modern swap effects");
    }

    if(_desc->BufferCount > 16)
    {
      _issues.Add("BufferCount exceeds reasonable limits (16)");
    }

    if((_desc->SwapEffect == Silk.NET.DXGI.SwapEffect.FlipSequential ||
         _desc->SwapEffect == Silk.NET.DXGI.SwapEffect.FlipDiscard) &&
        _desc->BufferCount < 2)
    {
      _issues.Add("Flip swap effects require at least 2 buffers");
    }

    if(_desc->Format == Format.FormatUnknown)
    {
      _issues.Add("SwapChain format cannot be unknown");
    }

    if(_desc->SampleDesc.Count > 1 &&
        (_desc->SwapEffect == Silk.NET.DXGI.SwapEffect.FlipSequential ||
         _desc->SwapEffect == Silk.NET.DXGI.SwapEffect.FlipDiscard))
    {
      _issues.Add("Flip swap effects do not support MSAA");
    }

    return _issues.Count == 0;
  }

  /// <summary>
  /// Рекомендует оптимальные настройки SwapChain
  /// </summary>
  public static SwapChainDesc1 GetRecommendedSettings(uint _width, uint _height, bool _enableVSync = true)
  {
    return new SwapChainDesc1
    {
      Width = _width,
      Height = _height,
      Format = Format.FormatR8G8B8A8Unorm,
      Stereo = false,
      SampleDesc = new SampleDesc { Count = 1, Quality = 0 },
      BufferUsage = DXGI.UsageRenderTargetOutput,
      BufferCount = _enableVSync ? 2u : 3u,
      Scaling = Scaling.None,
      SwapEffect = Silk.NET.DXGI.SwapEffect.FlipDiscard,
      AlphaMode = Silk.NET.DXGI.AlphaMode.Ignore,
      Flags = _enableVSync ? 0u : (uint)Silk.NET.DXGI.SwapChainFlag.AllowTearing
    };
  }
}
