using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Silk.NET.Core.Native;
using Silk.NET.DXGI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;
public unsafe class DX12Monitor: IMonitor
{
  private readonly IDXGIOutput* p_output;
  private readonly string p_name;
  private readonly int p_width;
  private readonly int p_height;
  private readonly int p_refreshRate;

  public DX12Monitor(ComPtr<IDXGIOutput> _output)
  {
    p_output = _output;
    OutputDesc desc;

    HResult hr = p_output->GetDesc(&desc);

    if(hr.IsSuccess)
    {
      p_name = desc.DeviceName != null 
        ? new string(desc.DeviceName) 
        : "Unknown Monitor";

      p_width = desc.DesktopCoordinates.Max.X - desc.DesktopCoordinates.Min.X;
      p_height = desc.DesktopCoordinates.Max.Y - desc.DesktopCoordinates.Min.Y;
    }

    uint numModes = 0;

    p_output->GetDisplayModeList(Format.FormatR8G8B8A8Unorm, (uint)EnumModes.Interlaced, &numModes, null);

    if(numModes > 0)
    {
      var modes = stackalloc ModeDesc[(int)numModes];
      p_output->GetDisplayModeList(Format.FormatR8G8B8A8Unorm, (uint)EnumModes.Interlaced, &numModes, modes);

      if(numModes > 0)
      {
        p_refreshRate = (int)(modes[0].RefreshRate.Numerator / modes[0].RefreshRate.Denominator);
      }
    }
    else
    {
      p_name = "Unknown Monitor";
      p_width = 1280;
      p_height = 720;
      p_refreshRate = 30;
    }
  }

  public string Name => p_name;

  public int Width => p_width;

  public int Height => p_height;

  public int RefreshRate => p_refreshRate;

  public nint Handle => (IntPtr)p_output;

  public ComPtr<IDXGIOutput> GetNativeOutput() => p_output;
}
