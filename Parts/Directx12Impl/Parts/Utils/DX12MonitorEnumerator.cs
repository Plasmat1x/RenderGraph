using GraphicsAPI.Interfaces;

using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl.Parts.Utils;

/// <summary>
/// Утилита для перечисления мониторов в DX12
/// </summary>
public unsafe static class DX12MonitorEnumerator
{
  /// <summary>
  /// Получить все доступные мониторы
  /// </summary>
  public static List<IMonitor> EnumerateMonitors(IDXGIFactory4* _factory)
  {
    var monitors = new List<IMonitor>();

    uint adapterIndex = 0;
    IDXGIAdapter1* adapter;

    while(_factory->EnumAdapters1(adapterIndex, &adapter) == 0)
    {
      uint outputIndex = 0;
      IDXGIOutput* output;

      while(adapter->EnumOutputs(outputIndex, &output) == 0)
      {
        monitors.Add(new DX12Monitor(output));
        outputIndex++;
      }

      adapter->Release();
      adapterIndex++;
    }

    return monitors;
  }

  /// <summary>
  /// Получить основной монитор
  /// </summary>
  public static IMonitor GetPrimaryMonitor(IDXGIFactory4* _factory)
  {
    IDXGIAdapter1* adapter;
    if(_factory->EnumAdapters1(0, &adapter) == 0)
    {
      IDXGIOutput* output;
      if(adapter->EnumOutputs(0, &output) == 0)
      {
        adapter->Release();
        return new DX12Monitor(output);
      }
      adapter->Release();
    }

    return null;
  }

  /// <summary>
  /// Найти монитор по хэндлу окна
  /// </summary>
  public static IMonitor FindMonitorForWindow(IDXGIFactory4* _factory, IntPtr _windowHandle)
  {
    // TODO: Реализовать поиск монитора по координатам окна
    // Пока возвращаем основной монитор
    return GetPrimaryMonitor(_factory);
  }
}