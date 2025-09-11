using Directx12Impl.Extensions;
using Directx12Impl.Parts.Data;
using Directx12Impl.Parts.Utils;

using GraphicsAPI.Descriptions;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

using System.Runtime.InteropServices;

namespace Directx12Impl.Parts.Structures;
public unsafe class DX12PipelineStateCache: IDisposable
{
  private readonly Dictionary<PSOCacheKey, ComPtr<ID3D12PipelineState>> p_graphicsCache = [];
  private readonly Dictionary<ComputePSOCacheKey, ComPtr<ID3D12PipelineState>> p_computeCache = [];
  private readonly ComPtr<ID3D12Device> p_device;
  private readonly object p_cacheLock = new();
  private bool p_disposed;

  public DX12PipelineStateCache(ComPtr<ID3D12Device> _device)
  {
    p_device = _device;
  }

  public unsafe ID3D12PipelineState* GetOrCreatePSO(PSOCacheKey _key)
  {
    lock(p_cacheLock)
    {
      if(p_graphicsCache.TryGetValue(_key, out var pso))
        return pso;

      pso = CreateGraphicsPSO(_key);
      p_graphicsCache[_key] = pso;
      return pso;
    }
  }

  public unsafe ID3D12PipelineState* GetOrCreateComputePSO(ComputePSOCacheKey _key)
  {
    lock(p_cacheLock)
    {
      if(p_computeCache.TryGetValue(_key, out var pso))
        return pso;

      pso = CreateComputePSO(_key);
      p_computeCache[_key] = pso;
      return pso;
    }
  }

  private unsafe ID3D12PipelineState* CreateGraphicsPSO(PSOCacheKey _key)
  {
    if(_key.VertexShader == null)
      throw new ArgumentException("Vertex shader is required");

    var vs = _key.VertexShader;
    var ps = _key.PixelShader;
    var ds = _key.DomainShader;
    var hs = _key.HullShader;
    var gs = _key.GeometryShader;

    DX12ShaderValidator.ValidatePipelineShaders(vs, ps, gs, hs, ds);

    List<IntPtr> stringPointers = null;
    InputElementDesc[] inputElements = null;

    try
    {
      if(_key.PipelineStateDescription.InputLayout != null)
      {
        (inputElements, stringPointers) = _key.PipelineStateDescription.InputLayout.ConvertWithMemory();
      }
      else
      {
        var vsReflection = vs.GetReflection();
        var autoLayout = InputLayoutDescription.FromReflection(vsReflection);
        (inputElements, stringPointers) = autoLayout.ConvertWithMemory();
      }

      var psoDesc = new GraphicsPipelineStateDesc
      {
        PRootSignature = _key.RootSignature.Handle,
        VS = vs.GetD3D12Bytecode(),
        PS = ps?.GetD3D12Bytecode() ?? default,
        DS = ds?.GetD3D12Bytecode() ?? default,
        HS = hs?.GetD3D12Bytecode() ?? default,
        GS = gs?.GetD3D12Bytecode() ?? default,
        StreamOutput = default,
        BlendState = _key.RenderStateDescription.BlendState.Convert(),
        SampleMask = _key.PipelineStateDescription.SampleMask,
        RasterizerState = _key.RenderStateDescription.RasterizerState.Convert(),
        DepthStencilState = _key.RenderStateDescription.DepthStencilState.Convert(),
        InputLayout = default,
        IBStripCutValue = IndexBufferStripCutValue.ValueDisabled,
        PrimitiveTopologyType = _key.PipelineStateDescription.PrimitiveTopology.Convert(),
        NumRenderTargets = _key.PipelineStateDescription.RenderTargetCount,
        DSVFormat = _key.PipelineStateDescription.DepthStencilFormat.Convert(),
        SampleDesc = new SampleDesc
        {
          Count = _key.PipelineStateDescription.SampleCount,
          Quality = _key.PipelineStateDescription.SampleQuality
        },
        NodeMask = 0,
        Flags = PipelineStateFlags.None
      };

      for(var i = 0; i < 8; i++)
      {
        if(_key.PipelineStateDescription.RenderTargetFormats != null &&
            i < _key.PipelineStateDescription.RenderTargetFormats.Length)
        {
          psoDesc.RTVFormats[i] = _key.PipelineStateDescription.RenderTargetFormats[i].Convert();
        }
        else
        {
          psoDesc.RTVFormats[i] = Format.FormatUnknown;
        }
      }

      if(inputElements != null && inputElements.Length > 0)
      {
        fixed(InputElementDesc* pElements = inputElements)
        {
          psoDesc.InputLayout = new InputLayoutDesc
          {
            PInputElementDescs = pElements,
            NumElements = (uint)inputElements.Length
          };

          ID3D12PipelineState* pipelineState;
          fixed(Guid* pGuid = &ID3D12PipelineState.Guid)
          {
            HResult hr = p_device.CreateGraphicsPipelineState(&psoDesc, pGuid, (void**)&pipelineState);

            if(hr.IsFailure)
            {
              string errorDetails = GetPSOCreationErrorDetails(hr);
              throw new InvalidOperationException(
                  $"Failed to create graphics pipeline state: 0x{hr.Value:X8}\n{errorDetails}");
            }

            Console.WriteLine("=== INPUT LAYOUT DEBUG ===");
            if(inputElements != null)
            {
              Console.WriteLine($"Input Layout has {inputElements.Length} elements:");
              for(int i = 0; i < inputElements.Length; i++)
              {
                var element = inputElements[i];
                Console.WriteLine($"  Element {i}:");
                Console.WriteLine($"    SemanticName: {Marshal.PtrToStringAnsi((IntPtr)element.SemanticName)}");
                Console.WriteLine($"    SemanticIndex: {element.SemanticIndex}");
                Console.WriteLine($"    Format: {element.Format}");
                Console.WriteLine($"    InputSlot: {element.InputSlot}");
                Console.WriteLine($"    AlignedByteOffset: {element.AlignedByteOffset}");
                Console.WriteLine($"    InputSlotClass: {element.InputSlotClass}");
              }
            }
            else
            {
              Console.WriteLine("Input Layout is NULL!");
            }
            Console.WriteLine("=== END INPUT LAYOUT DEBUG ===");
          }

          return pipelineState;
        }
      }
      else
      {
        psoDesc.InputLayout = new InputLayoutDesc
        {
          PInputElementDescs = null,
          NumElements = 0
        };

        ID3D12PipelineState* pipelineState;
        fixed(Guid* pGuid = &ID3D12PipelineState.Guid)
        {
          var hr = p_device.CreateGraphicsPipelineState(&psoDesc, pGuid, (void**)&pipelineState);
          DX12Helpers.ThrowIfFailed(hr, "Failed to create graphics pipeline state");
        }

        return pipelineState;
      }
    }
    finally
    {
      if(stringPointers != null)
      {
        InputLayoutDescriptionExtensions.FreeStringPointers(stringPointers);
      }
    }
  }

  private unsafe ID3D12PipelineState* CreateComputePSO(ComputePSOCacheKey _key)
  {
    if(_key.ComputeShader == null)
      throw new ArgumentException("Compute shader is required");

    var psoDesc = new ComputePipelineStateDesc
    {
      PRootSignature = _key.RootSignature,
      CS = _key.ComputeShader.GetD3D12Bytecode(),
      NodeMask = 0,
      CachedPSO = default,
      Flags = PipelineStateFlags.None
    };

    ID3D12PipelineState* pipelineState;
    fixed(Guid* pGuid = &ID3D12PipelineState.Guid)
    {
      HResult hr = p_device.CreateComputePipelineState(&psoDesc, pGuid, (void**)&pipelineState);

      if(hr.IsFailure)
      {
        throw new InvalidOperationException($"Failed to create compute PSO: 0x{hr.Value:X8}");
      }
    }
    return pipelineState;
  }

  public void Clear()
  {
    lock(p_cacheLock)
    {
      foreach(var pso in p_graphicsCache.Values)
        pso.Dispose();

      p_graphicsCache.Clear();

      foreach(var pso in p_computeCache.Values)
        pso.Dispose();

      p_computeCache.Clear();
    }
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    Clear();

    p_disposed = true;
  }

  private string GetPSOCreationErrorDetails(HResult _hr)
  {
    var errorMessage = new System.Text.StringBuilder();
    errorMessage.AppendLine($"HRESULT: 0x{_hr.Value:X8}");

    switch((uint)_hr.Value)
    {
      case 0x80070057: // E_INVALIDARG
        errorMessage.AppendLine("Invalid argument. Check:");
        errorMessage.AppendLine("- All shader stages are compatible");
        errorMessage.AppendLine("- Input layout matches vertex shader");
        errorMessage.AppendLine("- Render target formats are valid");
        errorMessage.AppendLine("- Root signature matches shaders");
        break;

      case 0x887A0005: // DXGI_ERROR_DEVICE_REMOVED
        errorMessage.AppendLine("Device was removed. GPU crash or TDR.");
        break;

      case 0x887A0001: // DXGI_ERROR_INVALID_CALL
        errorMessage.AppendLine("Invalid call. Check PSO description parameters.");
        break;

      default:
        errorMessage.AppendLine("Unknown error. Enable D3D12 debug layer for more info.");
        break;
    }

    return errorMessage.ToString();
  }
}
