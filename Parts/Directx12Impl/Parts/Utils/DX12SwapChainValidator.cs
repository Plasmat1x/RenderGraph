using GraphicsAPI.Descriptions;

using Silk.NET.DXGI;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl.Parts.Utils;

/// <summary>
/// Валидатор настроек SwapChain
/// </summary>
public unsafe static class DX12SwapChainValidator
{
  /// <summary>
  /// Проверить совместимость настроек SwapChain с устройством
  /// </summary>
  public static ValidationResult ValidateDescription(SwapChainDescription _description, IDXGIAdapter* _adapter = null)
  {
    var result = new ValidationResult();

    ValidateBasicParameters(_description, result);
    ValidateParameterCompatibility(_description, result);

    if(_adapter != null)
    {
      ValidateFormatSupport(_description, _adapter, result);
    }

    return result;
  }

  private static void ValidateBasicParameters(SwapChainDescription _description, ValidationResult _result)
  {
    if(_description.Width == 0 || _description.Height == 0)
    {
      _result.AddError("SwapChain dimensions cannot be zero");
    }

    if(_description.Width > 16384 || _description.Height > 16384)
    {
      _result.AddWarning("SwapChain dimensions are very large and may not be supported on all hardware");
    }

    if(_description.BufferCount < 2)
    {
      _result.AddError("BufferCount must be at least 2");
    }

    if(_description.BufferCount > 16)
    {
      _result.AddWarning("BufferCount is very high and may impact performance");
    }

    if(_description.SampleCount > 1 && (_description.SampleCount & (_description.SampleCount - 1)) != 0)
    {
      _result.AddError("SampleCount must be a power of 2");
    }
  }

  private static void ValidateParameterCompatibility(SwapChainDescription _description, ValidationResult _result)
  {
    if(_description.SampleCount > 1 &&
        (_description.SwapEffect == GraphicsAPI.Enums.SwapEffect.FlipSequential ||
         _description.SwapEffect == GraphicsAPI.Enums.SwapEffect.FlipDiscard))
    {
      _result.AddError("Flip swap effects do not support multisampling");
    }

    if((_description.SwapEffect == GraphicsAPI.Enums.SwapEffect.FlipSequential ||
         _description.SwapEffect == GraphicsAPI.Enums.SwapEffect.FlipDiscard) &&
        _description.BufferCount < 2)
    {
      _result.AddError("Flip swap effects require at least 2 buffers");
    }

    if((_description.Flags & GraphicsAPI.Enums.SwapChainFlags.AllowTearing) != 0 &&
        _description.SwapEffect != GraphicsAPI.Enums.SwapEffect.FlipDiscard)
    {
      _result.AddWarning("AllowTearing flag is most effective with FlipDiscard swap effect");
    }
  }

  private static unsafe void ValidateFormatSupport(SwapChainDescription _description, IDXGIAdapter* _adapter, ValidationResult _result)
  {
    // TODO: Проверить поддержку формата через CheckFormatSupport

    var commonFormats = new[]
    {
            Resources.Enums.TextureFormat.R8G8B8A8_UNORM,
            Resources.Enums.TextureFormat.B8G8R8A8_UNORM,
            Resources.Enums.TextureFormat.R10G10B10A2_UNORM,
            Resources.Enums.TextureFormat.R16G16B16A16_FLOAT
        };

    if(!commonFormats.Contains(_description.Format))
    {
      _result.AddWarning($"Format {_description.Format} may not be supported on all hardware");
    }
  }
}
