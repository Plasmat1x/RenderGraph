using GraphicsAPI;
using GraphicsAPI.Commands;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Direct3D12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;


/// <summary>
/// DX12 реализация командного буфера, наследующаяся от GenericCommandBuffer
/// Переопределяет ExecuteCommand для прямого выполнения на DX12
/// </summary>
public unsafe class DX12CommandBuffer: GenericCommandBuffer
{
  private readonly D3D12 p_d3d12;
  private readonly ID3D12Device* p_device;
  private readonly ID3D12GraphicsCommandList* p_commandList;
  private readonly ID3D12CommandAllocator* p_commandAllocator;
  private readonly DX12ResourceStateTracker p_stateTracker;

  // DX12-specific state
  private ID3D12PipelineState* p_currentPipelineState;
  private ID3D12RootSignature* p_currentRootSignature;
  private readonly CpuDescriptorHandle[] p_currentRenderTargets = new CpuDescriptorHandle[8];
  private CpuDescriptorHandle? p_currentDepthStencil;
  private uint p_renderTargetCount;

  // Performance mode flag
  private readonly bool p_immediateMode;

  public DX12CommandBuffer(ID3D12Device* _device, D3D12 _d3d12, CommandBufferType _type, bool _immediateMode = true)
    : base(_type)
  {
    p_d3d12 = _d3d12;
    p_device = _device;
    p_stateTracker = new DX12ResourceStateTracker();
    p_immediateMode = _immediateMode;

    CreateCommandListAndAllocator();
  }

  /// <summary>
  /// Режим работы: true = прямое выполнение, false = запись команд
  /// </summary>
  public bool ImmediateMode => p_immediateMode;

  /// <summary>
  /// Получить нативный command list для продвинутых операций
  /// </summary>
  public ID3D12GraphicsCommandList* GetNativeCommandList() => p_commandList;

  // === Переопределяем методы для прямого выполнения в immediate mode ===

  protected override void ExecuteCommand(ICommand _command)
  {
    // В immediate mode выполняем команды напрямую через DX12 API
    if(p_immediateMode)
    {
      ExecuteCommandDirectly(_command);
    }
    else
    {
      // В deferred mode можем использовать базовую логику или свою
      ExecuteCommandDeferred(_command);
    }
  }

  protected override void OnBegin()
  {
    base.OnBegin();

    // Reset command list
    var hr = p_commandList->Reset(p_commandAllocator, null);
    DX12Helpers.ThrowIfFailed(hr, "Failed to reset command list");

    p_stateTracker.Reset();
    ResetDX12State();
  }

  protected override void OnEnd()
  {
    if(p_immediateMode)
    {
      // Flush pending barriers and close
      p_stateTracker.FlushResourceBarriers(p_commandList);
      var hr = p_commandList->Close();
      DX12Helpers.ThrowIfFailed(hr, "Failed to close command list");
    }

    base.OnEnd();
  }

  // === Можем переопределить критичные методы для производительности ===

  public override void Draw(uint _vertexCount, uint _instanceCount = 1, uint _startVertex = 0, uint _startInstance = 0)
  {
    if(p_immediateMode)
    {
      // Прямое выполнение без создания команды
      ValidateRecording();
      p_stateTracker.FlushResourceBarriers(p_commandList);
      p_commandList->DrawInstanced(_vertexCount, _instanceCount, _startVertex, _startInstance);
    }
    else
    {
      // Используем базовую логику записи команд
      base.Draw(_vertexCount, _instanceCount, _startVertex, _startInstance);
    }
  }

  public override void DrawIndexed(uint _indexCount, uint _instanceCount = 1, uint _startIndex = 0, int _baseVertex = 0, uint _startInstance = 0)
  {
    if(p_immediateMode)
    {
      ValidateRecording();
      p_stateTracker.FlushResourceBarriers(p_commandList);
      p_commandList->DrawIndexedInstanced(_indexCount, _instanceCount, _startIndex, _baseVertex, _startInstance);
    }
    else
    {
      base.DrawIndexed(_indexCount, _instanceCount, _startIndex, _baseVertex, _startInstance);
    }
  }

  public override void SetRenderTargets(ITextureView[] _colorTargets, ITextureView _depthTarget)
  {
    if(p_immediateMode)
    {
      ValidateRecording();
      SetRenderTargetsDirectly(_colorTargets, _depthTarget);
    }
    else
    {
      base.SetRenderTargets(_colorTargets, _depthTarget);
    }
  }

  public override void ClearRenderTarget(ITextureView _target, Vector4 _color)
  {
    if(p_immediateMode)
    {
      ValidateRecording();
      ClearRenderTargetDirectly(_target, _color);
    }
    else
    {
      base.ClearRenderTarget(_target, _color);
    }
  }

  // === Методы прямого выполнения ===

  private void ExecuteCommandDirectly(ICommand _command)
  {
    switch(_command)
    {
      case DrawCommand draw:
        p_stateTracker.FlushResourceBarriers(p_commandList);
        p_commandList->DrawInstanced(draw.VertexCount, draw.InstanceCount, draw.StartVertex, draw.StartInstance);
        break;

      case DrawIndexedCommand drawIndexed:
        p_stateTracker.FlushResourceBarriers(p_commandList);
        p_commandList->DrawIndexedInstanced(drawIndexed.IndexCount, drawIndexed.InstanceCount,
                                           drawIndexed.StartIndex, drawIndexed.BaseVertex, drawIndexed.StartInstance);
        break;

      case SetRenderTargetsCommand renderTargets:
        SetRenderTargetsDirectly(renderTargets.ColorTargets, renderTargets.DepthTarget);
        break;

      case ClearRenderTargetCommand clearRT:
        ClearRenderTargetDirectly(clearRT.Target, clearRT.Color);
        break;

      case ClearDepthStencilCommand clearDS:
        ClearDepthStencilDirectly(clearDS.Target, clearDS.Flags, clearDS.Depth, clearDS.Stencil);
        break;

      case SetViewportCommand viewport:
        SetViewportDirectly(viewport.Viewport);
        break;

      case SetVertexBufferCommand vertexBuffer:
        SetVertexBufferDirectly(vertexBuffer.Buffer, vertexBuffer.Slot);
        break;

      case SetIndexBufferCommand indexBuffer:
        SetIndexBufferDirectly(indexBuffer.Buffer, indexBuffer.Format);
        break;

      case TransitionResourceCommand transition:
        if(transition.Resource is DX12Resource dx12Resource)
        {
          p_stateTracker.TransitionResource(dx12Resource.GetResource(), DX12Helpers.ConvertResourceState(transition.NewState));
        }
        break;

      case DispatchCommand dispatch:
        p_stateTracker.FlushResourceBarriers(p_commandList);
        p_commandList->Dispatch(dispatch.GroupCountX, dispatch.GroupCountY, dispatch.GroupCountZ);
        break;

      // Можно добавить больше команд для оптимизации
      default:
        // Для неоптимизированных команд используем общую логику
        ExecuteCommandGeneric(_command);
        break;
    }
  }

  private void ExecuteCommandDeferred(ICommand _command)
  {
    // В deferred режиме можем использовать более сложную логику
    // Например, группировку команд, дополнительную оптимизацию и т.д.
    ExecuteCommandDirectly(_command);
  }

  private void ExecuteCommandGeneric(ICommand _command)
  {
    // Fallback для команд, которые не оптимизированы
    Console.WriteLine($"[DX12] Executing generic command: {_command.Type}");
  }

  // === DX12-специфичные методы ===

  private void SetRenderTargetsDirectly(ITextureView[] _colorTargets, ITextureView _depthTarget)
  {
    p_renderTargetCount = 0;

    if(_colorTargets != null)
    {
      for(int i = 0; i < Math.Min(_colorTargets.Length, p_currentRenderTargets.Length); i++)
      {
        if(_colorTargets[i] is _DX12TextureView dx12View)
        {
          p_currentRenderTargets[i] = dx12View.GetRenderTargetView();
          p_renderTargetCount++;
        }
      }
    }

    if(_depthTarget is _DX12TextureView depthView)
    {
      p_currentDepthStencil = depthView.GetDepthStencilView();
    }
    else
    {
      p_currentDepthStencil = null;
    }

    ApplyRenderTargets();
  }

  private void ClearRenderTargetDirectly(ITextureView _target, Vector4 _color)
  {
    if(_target is not _DX12TextureView dx12View)
      throw new ArgumentException("Invalid texture view type");

    var clearColor = stackalloc float[4] { _color.X, _color.Y, _color.Z, _color.W };
    var rtvHandle = dx12View.GetRenderTargetView();

    p_commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, null);
  }

  private void ClearDepthStencilDirectly(ITextureView _target, GraphicsAPI.Enums.ClearFlags _flags, float _depth, byte _stencil)
  {
    if(_target is not _DX12TextureView dx12View)
      throw new ArgumentException("Invalid texture view type");

    var d3d12Flags = ConvertClearFlags(_flags);
    var dsvHandle = dx12View.GetDepthStencilView();

    p_commandList->ClearDepthStencilView(dsvHandle.Value, d3d12Flags, _depth, _stencil, 0, null);
  }

  private void SetViewportDirectly(Resources.Viewport _viewport)
  {
    var d3d12Viewport = new Silk.NET.Direct3D12.Viewport
    {
      TopLeftX = _viewport.X,
      TopLeftY = _viewport.Y,
      Width = _viewport.Width,
      Height = _viewport.Height,
      MinDepth = _viewport.MinDepth,
      MaxDepth = _viewport.MaxDepth
    };

    p_commandList->RSSetViewports(1, &d3d12Viewport);
  }

  private void SetVertexBufferDirectly(IBufferView _buffer, uint _slot)
  {
    if(_buffer is not DX12BufferView dx12View)
      throw new ArgumentException("Invalid buffer view type");

    var vbView = dx12View.GetVertexBufferView();
    p_commandList->IASetVertexBuffers(_slot, 1, &vbView);
  }

  private void SetIndexBufferDirectly(IBufferView _buffer, IndexFormat _format)
  {
    if(_buffer is not DX12BufferView dx12View)
      throw new ArgumentException("Invalid buffer view type");

    var ibView = dx12View.GetIndexBufferView(_format);
    p_commandList->IASetIndexBuffer(&ibView);
  }

  private void ApplyRenderTargets()
  {
    var dsHandle = p_currentDepthStencil.HasValue ? p_currentDepthStencil.Value : default;
    CpuDescriptorHandle rtHandle = p_renderTargetCount > 0 ? p_currentRenderTargets[0] : default;

    p_commandList->OMSetRenderTargets(
      p_renderTargetCount,
      p_renderTargetCount > 0 ? &rtHandle : null,
      false,
      p_currentDepthStencil.HasValue ? &dsHandle : null);
  }

  private void ResetDX12State()
  {
    p_currentPipelineState = null;
    p_currentRootSignature = null;
    p_renderTargetCount = 0;
    p_currentDepthStencil = null;
    Array.Clear(p_currentRenderTargets);
  }

  private void CreateCommandListAndAllocator()
  {
    // Создание command allocator и command list
    // Упрощенная версия - в реальности нужна полная инициализация
  }

  // === Conversion helpers ===
  private static Silk.NET.Direct3D12.ClearFlags ConvertClearFlags(GraphicsAPI.Enums.ClearFlags _flags) => _flags switch
  {
    GraphicsAPI.Enums.ClearFlags.Depth => Silk.NET.Direct3D12.ClearFlags.Depth,
    GraphicsAPI.Enums.ClearFlags.Stencil => Silk.NET.Direct3D12.ClearFlags.Stencil,
    GraphicsAPI.Enums.ClearFlags.DepthAndStencil => Silk.NET.Direct3D12.ClearFlags.Depth | Silk.NET.Direct3D12.ClearFlags.Stencil,
    _ => throw new ArgumentOutOfRangeException("Not supported index format")
  };

  // === Additional features ===

  /// <summary>
  /// Переключить режим выполнения
  /// </summary>
  public void SwitchToImmediateMode()
  {
    if(!p_immediateMode)
      throw new InvalidOperationException("Cannot switch to immediate mode - buffer was created in deferred mode");
    // Логика переключения
  }

  /// <summary>
  /// Выполнить все накопленные команды (для deferred режима)
  /// </summary>
  public override void Execute()
  {
    if(p_immediateMode)
    {
      // В immediate режиме команды уже выполнены
      return;
    }

    // В deferred режиме выполняем накопленные команды
    base.Execute();
  }

  /// <summary>
  /// Получить статистику с DX12-специфичной информацией
  /// </summary>
  public DX12CommandBufferStats GetDX12Stats()
  {
    var baseStats = GetStats();

    return new DX12CommandBufferStats
    {
      BaseStats = baseStats,
      StateTransitions = p_stateTracker.GetTransitionCount(),
      BarrierFlushes = p_stateTracker.GetBarrierFlushCount(),
      ImmediateMode = p_immediateMode,
      NativeCommandListPtr = (IntPtr)p_commandList
    };
  }

  public override void Dispose()
  {
    p_stateTracker?.Dispose();

    if(p_commandList != null)
    {
      p_commandList->Release();
    }

    if(p_commandAllocator != null)
    {
      p_commandAllocator->Release();
    }

    base.Dispose();
  }
}

/// <summary>
/// DX12-специфичная статистика
/// </summary>
public struct DX12CommandBufferStats
{
  public CommandBufferStats BaseStats;
  public int StateTransitions;
  public int BarrierFlushes;
  public bool ImmediateMode;
  public IntPtr NativeCommandListPtr;

  public override readonly string ToString()
  {
    return $"{BaseStats}, DX12: Transitions={StateTransitions}, Flushes={BarrierFlushes}, Mode={ImmediateMode}";
  }
}

