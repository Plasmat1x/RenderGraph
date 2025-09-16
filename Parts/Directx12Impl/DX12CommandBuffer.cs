

using Directx12Impl.Extensions;
using Directx12Impl.Parts.Data;
using Directx12Impl.Parts.Managers;
using Directx12Impl.Parts.Utils;

using GraphicsAPI;
using GraphicsAPI.Commands;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Silk.NET.Maths;

using System.Numerics;

namespace Directx12Impl;

/// <summary>
/// DX12 реализация командного буфера, наследующаяся от GenericCommandBuffer
/// Переопределяет ExecuteCommand для прямого выполнения на DX12
/// </summary>
public unsafe class DX12CommandBuffer: GenericCommandBuffer
{
  private readonly D3D12 p_d3d12;
  private readonly ID3D12Device* p_device;
  private ID3D12GraphicsCommandList* p_commandList;
  private ID3D12CommandAllocator* p_commandAllocator;
  private readonly DX12ResourceStateTracker p_stateTracker = new();
  private readonly DX12GraphicsDevice p_parentDevice;

  private readonly CpuDescriptorHandle[] p_currentRenderTargets = new CpuDescriptorHandle[8];
  private CpuDescriptorHandle? p_currentDepthStencil;
  private uint p_renderTargetCount;
  private CommandBufferExecutionMode p_executionMode;

  private DX12RenderState p_currentRenderState;

  public DX12CommandBuffer(DX12GraphicsDevice _parentDevice, ID3D12Device* _device, D3D12 _d3d12, CommandBufferType _type, CommandBufferExecutionMode _executionMode)
    : base(_type)
  {
    p_d3d12 = _d3d12;
    p_device = _device;
    p_stateTracker = new DX12ResourceStateTracker();
    p_executionMode = _executionMode;
    p_parentDevice = _parentDevice;

    CreateCommandListAndAllocator();
  }

  /// <summary>
  /// Режим работы
  /// </summary>
  public CommandBufferExecutionMode ImmediateMode => p_executionMode;

  /// <summary>
  /// Получить нативный command list для продвинутых операций
  /// </summary>
  public ID3D12GraphicsCommandList* GetNativeCommandList() => p_commandList;


  /// <summary>
  /// Установить режим выполнения команд
  /// </summary>
  public void SetExecutionMode(CommandBufferExecutionMode _mode)
  {
    p_executionMode = _mode;
  }

  // === Переопределяем методы для прямого выполнения в immediate mode ===

  /// <summary>
  /// Основной метод выполнения команды - переопределяем из GenericCommandBuffer
  /// </summary>
  protected override void ExecuteCommand(ICommand _command)
  {
    if(_command == null)
      return;

    switch(p_executionMode)
    {
      case CommandBufferExecutionMode.Immediate:
        ExecuteCommandImmediate(_command);
        break;
      case CommandBufferExecutionMode.Deferred:
        ExecuteCommandDeferred(_command);
        break;
      default:
        ExecuteCommandGeneric(_command);
        break;
    }
  }

  protected override void OnBegin()
  {
    base.OnBegin();

    var hr = p_commandList->Reset(p_commandAllocator, null);
    DX12Helpers.ThrowIfFailed(hr, "Failed to reset command list");

    p_stateTracker.Reset();
    ResetDX12State();

    SetupDescriptorHeaps();
  }

  protected override void OnEnd()
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      p_stateTracker.FlushResourceBarriers(p_commandList);
      var hr = p_commandList->Close();
      DX12Helpers.ThrowIfFailed(hr, "Failed to close command list");
    }

    base.OnEnd();
  }

  // === Можем переопределить критичные методы для производительности ===

  public override void Draw(uint _vertexCount, uint _instanceCount = 1, uint _startVertex = 0, uint _startInstance = 0)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      p_stateTracker.FlushResourceBarriers(p_commandList);
      p_commandList->DrawInstanced(_vertexCount, _instanceCount, _startVertex, _startInstance);
    }
    else
    {
      base.Draw(_vertexCount, _instanceCount, _startVertex, _startInstance);
    }
  }

  public override void DrawIndexed(uint _indexCount, uint _instanceCount = 1, uint _startIndex = 0, int _baseVertex = 0, uint _startInstance = 0)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();

      Console.WriteLine($"\n[CommandBuffer] === DrawIndexed START ===");
      Console.WriteLine($"[CommandBuffer] IndexCount: {_indexCount}");
      Console.WriteLine($"[CommandBuffer] InstanceCount: {_instanceCount}");
      Console.WriteLine($"[CommandBuffer] StartIndex: {_startIndex}");
      Console.WriteLine($"[CommandBuffer] BaseVertex: {_baseVertex}");
      Console.WriteLine($"[CommandBuffer] StartInstance: {_startInstance}");

      Console.WriteLine($"[CommandBuffer] Current render state: {(p_currentRenderState != null ? "SET" : "NULL")}");

      p_stateTracker.FlushResourceBarriers(p_commandList);
      Console.WriteLine($"[CommandBuffer] Calling DrawIndexedInstanced...");

      p_commandList->DrawIndexedInstanced(_indexCount, _instanceCount, _startIndex, _baseVertex, _startInstance);

      Console.WriteLine($"[CommandBuffer] DrawIndexedInstanced completed");
      Console.WriteLine($"[CommandBuffer] === DrawIndexed END ===\n");
    }
    else
    {
      base.DrawIndexed(_indexCount, _instanceCount, _startIndex, _baseVertex, _startInstance);
    }
  }

  public override void SetRenderTargets(ITextureView[] _colorTargets, ITextureView _depthTarget)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
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
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      ClearRenderTargetDirectly(_target, _color);
    }
    else
    {
      base.ClearRenderTarget(_target, _color);
    }
  }

  public override void SetRenderState(IRenderState _renderState)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      SetRenderStateDirectly(_renderState);
    }
    else
    {
      base.SetRenderState(_renderState);
    }
  }

  public override void SetConstantBuffer(ShaderStage _stage, uint _slot, IBufferView _buffer)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      var cmd = new SetConstantBufferCommand(_stage, _slot, _buffer);
      SetConstantBufferDirectly(cmd);
    }
    else
    {
      base.SetConstantBuffer(_stage, _slot, _buffer);
    }
  }

  public override void SetViewport(Resources.Viewport _viewport)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      SetViewportDirectly(_viewport);
    }
    else
    {
      base.SetViewport(_viewport);
    }
  }

  public override void SetScissorRect(Resources.Rectangle _rect)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      SetScissorRectDirectly(_rect);
    }
    else
    {
      base.SetScissorRect(_rect);
    }
  }

  public override void SetVertexBuffer(IBufferView _vertices, uint _slot)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      SetVertexBufferDirectly(_vertices, _slot);
    }
    else
    {
      base.SetVertexBuffer(_vertices);
    }
  }

  public override void SetIndexBuffer(IBufferView _indices, IndexFormat _format)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      SetIndexBufferDirectly(_indices, _format);
    }
    else
    {
      base.SetIndexBuffer(_indices, _format);
    }
  }

  public override void ClearDepthStencil(ITextureView _target, GraphicsAPI.Enums.ClearFlags _flags, float _depth, byte _stencil)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      ClearDepthStencilDirectly(_target, _flags, _depth, _stencil);
    }
    else
    {
      base.ClearDepthStencil(_target, _flags, _depth, _stencil);
    }
  }

  public void TransitionBackBufferForPresent(DX12Texture _backBuffer)
  {
    var barrier = new ResourceBarrier
    {
      Type = ResourceBarrierType.Transition,
      Transition = new ResourceTransitionBarrier
      {
        PResource = _backBuffer.GetResource(),
        StateBefore = ResourceStates.RenderTarget,
        StateAfter = ResourceStates.Present,
        Subresource = 0
      }
    };

    p_commandList->ResourceBarrier(1, &barrier);
    _backBuffer.SetCurrentState(ResourceStates.Present);
  }


  // === Методы прямого выполнения ===

  /// <summary>
  /// Немедленное выполнение команды (оптимизированное)
  /// </summary>
  private void ExecuteCommandImmediate(ICommand _command)
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
        Console.WriteLine($"[CommandBuffer] Processing SetViewportCommand: {viewport.Viewport.Width}x{viewport.Viewport.Height}");
        SetViewportDirectly(viewport.Viewport);
        break;

      case SetScissorRectCommand rectangle:
        Console.WriteLine($"[CommandBuffer] Processing SetScissorRectCommand: ({rectangle.Rect.X},{rectangle.Rect.Y},{rectangle.Rect.Width},{rectangle.Rect.Height})");
        SetScissorRectDirectly(rectangle.Rect);
        break;

      case SetVertexBufferCommand vertexBuffer:
        Console.WriteLine($"[CommandBuffer] Processing SetVertexBufferCommand: Slot={vertexBuffer.Slot}");
        SetVertexBufferDirectly(vertexBuffer.Buffer, vertexBuffer.Slot);
        break;

      case SetIndexBufferCommand indexBuffer:
        Console.WriteLine($"[CommandBuffer] Processing SetIndexBufferCommand: Format={indexBuffer.Format}");
        SetIndexBufferDirectly(indexBuffer.Buffer, indexBuffer.Format);
        break;

      case SetConstantBufferCommand constantBuffer:
        Console.WriteLine($"[CommandBuffer] Processing SetConstantBufferCommand: Stage={constantBuffer.Stage}, Slot={constantBuffer.Slot}");
        SetConstantBufferDirectly(constantBuffer);
        break;

      case SetShaderResourceCommand shaderResource:
        Console.WriteLine($"[CommandBuffer] Processing SetShaderResourceCommand: Stage={shaderResource.Stage}, Slot={shaderResource.Slot}");
        SetShaderResourceDirectly(shaderResource);
        break;

      case SetSamplerCommand sampler:
        Console.WriteLine($"[CommandBuffer] Processing SetSamplerCommand: Stage={sampler.Stage}, Slot={sampler.Slot}");
        SetSamplerDirectly(sampler);
        break;

      case SetUnorderedAccessCommand uav:
        Console.WriteLine($"[CommandBuffer] Processing SetUnorderedAccessCommand: Stage={uav.Stage}, Slot={uav.Slot}");
        SetUnorderedAccessDirectly(uav);
        break;

      case TransitionResourceCommand transition:
        if(transition.Resource is DX12Resource dx12Resource)
        {
          p_stateTracker.TransitionResource(dx12Resource.GetResource(),
              transition.NewState.Convert());
        }
        break;

      case DispatchCommand dispatch:
        p_stateTracker.FlushResourceBarriers(p_commandList);
        p_commandList->Dispatch(dispatch.GroupCountX, dispatch.GroupCountY, dispatch.GroupCountZ);
        break;

      case SetComputeShaderCommand computeShader:
        SetComputeShaderDirectly(computeShader.Shader);
        break;

      case SetVertexShaderCommand vertexShader:
        SetVertexShaderDirectly(vertexShader.Shader);
        break;

      case SetPixelShaderCommand pixelShader:
        SetPixelShaderDirectly(pixelShader.Shader);
        break;

      case SetRenderStateCommand renderState:
        Console.WriteLine($"[CommandBuffer] Processing SetRenderStateCommand");
        SetRenderStateDirectly(renderState.RenderState);
        break;

      case SetPrimitiveTopologyCommand primitiveTopology:
        Console.WriteLine($"[CommandBuffer] Processing SetPrimitiveTopologyCommand: Topology={primitiveTopology.Topology}");
        SetPrimitiveTopologyDirectly(primitiveTopology.Topology);
        break;

      case BeginEventCommand beginEvent:
        BeginEvent(beginEvent.Name);
        break;

      case EndEventCommand:
        EndEvent();
        break;

      case SetMarkerCommand marker:
        SetMarker(marker.Name);
        break;

      default:
        Console.WriteLine($"[CommandBuffer] ⚠️ WARNING: Unsupported command type {_command.GetType().Name} in immediate mode");
        ExecuteCommandGeneric(_command);
        break;
    }
  }

  /// <summary>
  /// Отложенное выполнение команды (с возможностью группировки и оптимизации)
  /// </summary>
  private void ExecuteCommandDeferred(ICommand _command)
  {
    //TODO: impl deferred render

    // В deferred режиме можем использовать более сложную логику
    // Например, группировку команд, дополнительную оптимизацию и т.д.

    // Пока что используем простое выполнение, но здесь можно добавить:
    // - Группировку state changes
    // - Batch обработку ресурсов
    // - Оптимизацию переходов состояний
    ExecuteCommandImmediate(_command);
  }

  private void SetScissorRectDirectly(Resources.Rectangle _rect)
  {
    Console.WriteLine($"\n[CommandBuffer] === SetScissorRectDirectly START ===");
    Console.WriteLine($"[CommandBuffer] ScissorRect X: {_rect.X}");
    Console.WriteLine($"[CommandBuffer] ScissorRect Y: {_rect.Y}");
    Console.WriteLine($"[CommandBuffer] ScissorRect Width: {_rect.Width}");
    Console.WriteLine($"[CommandBuffer] ScissorRect Height: {_rect.Height}");

    var d3d12Rect = new Box2D<int>
    {
      Min = new Vector2D<int>(_rect.X, _rect.Y),
      Max = new Vector2D<int>(_rect.X + _rect.Width, _rect.Y + _rect.Height)
    };

    Console.WriteLine($"[CommandBuffer] D3D12 Rect Left: {d3d12Rect.Min.X}");
    Console.WriteLine($"[CommandBuffer] D3D12 Rect Top: {d3d12Rect.Min.Y}");
    Console.WriteLine($"[CommandBuffer] D3D12 Rect Right: {d3d12Rect.Max.X}");
    Console.WriteLine($"[CommandBuffer] D3D12 Rect Bottom: {d3d12Rect.Max.Y}");

    p_commandList->RSSetScissorRects(1, &d3d12Rect);
    Console.WriteLine($"[CommandBuffer] Called RSSetScissorRects");
    Console.WriteLine($"[CommandBuffer] === SetScissorRectDirectly END ===\n");
  }

  /// <summary>
  /// Общее выполнение команды через переопределенные методы базового класса
  /// </summary>
  private void ExecuteCommandGeneric(ICommand _command)
  {
    switch(_command)
    {
      case BeginEventCommand beginEvent:
        BeginEvent(beginEvent.Name);
        break;

      case EndEventCommand:
        EndEvent();
        break;

      case SetMarkerCommand marker:
        SetMarker(marker.Name);
        break;

      default:
        Console.WriteLine($"Warning: Unsupported command type {_command.GetType().Name}");
        break;
    }
  }

  // === DX12-специфичные методы ===
  #region Immediate

  private void SetRenderTargetDirectly(ITextureView _colorTarget, ITextureView _depthTarget)
  {
    Console.WriteLine("\n[CommandBuffer] === SetRenderTargetDirectly START ===");

    p_renderTargetCount = 0;

    var transitionsNeeded = new List<(DX12Texture texture, ResourceStates targetState)>();

    if(_colorTarget is DX12TextureView colorView)
    {
      var colorTexture = colorView.Texture as DX12Texture;
      if(colorTexture != null)
      {
        var currentState = colorTexture.GetCurrentState();
        Console.WriteLine($"[CommandBuffer] Color target ({colorTexture.Name}) current state: {currentState}");

        if(currentState != ResourceStates.RenderTarget)
        {
          transitionsNeeded.Add((colorTexture, ResourceStates.RenderTarget));
          Console.WriteLine($"[CommandBuffer] Will transition color: {currentState} → RenderTarget");
        }
      }
      p_currentRenderTargets[0] = colorView.GetRenderTargetView();
      p_renderTargetCount = 1;
    }

    if(_depthTarget is DX12TextureView depthView)
    {
      var depthTexture = depthView.Texture as DX12Texture;
      if(depthTexture != null)
      {
        var currentState = depthTexture.GetCurrentState();
        Console.WriteLine($"[CommandBuffer] Depth target ({depthTexture.Name}) current state: {currentState}");

        if(currentState != ResourceStates.DepthWrite)
        {
          transitionsNeeded.Add((depthTexture, ResourceStates.DepthWrite));
          Console.WriteLine($"[CommandBuffer] Will transition depth: {currentState} → DepthWrite");
        }
      }
      p_currentDepthStencil = depthView.GetDepthStencilView();
    }
    else
    {
      p_currentDepthStencil = null;
    }

    if(transitionsNeeded.Count > 0)
    {
      Console.WriteLine($"[CommandBuffer] Executing {transitionsNeeded.Count} transitions...");

      foreach(var (texture, targetState) in transitionsNeeded)
      {
        p_stateTracker.TransitionResource(
            texture.GetResource(),
            targetState,
            0);

        texture.SetCurrentState(targetState);
      }

      Console.WriteLine("[CommandBuffer] Flushing all resource barriers...");
      p_stateTracker.FlushResourceBarriers(p_commandList);
    }
    else
    {
      Console.WriteLine("[CommandBuffer] No state transitions needed");
    }

    Console.WriteLine("[CommandBuffer] Setting render targets on command list...");
    ApplyRenderTargets();

    Console.WriteLine("[CommandBuffer] === SetRenderTargetDirectly END ===\n");
  }

  private void SetRenderTargetsDirectly(ITextureView[] _colorTargets, ITextureView _depthTarget)
  {
    p_renderTargetCount = 0;

    if(_colorTargets != null)
    {
      for(int i = 0; i < Math.Min(_colorTargets.Length, p_currentRenderTargets.Length); i++)
      {
        if(_colorTargets[i] is DX12TextureView dx12View)
        {
          var colorTexture = dx12View.Texture as DX12Texture;
          if(colorTexture != null)
          {
            var currentState = colorTexture.GetCurrentState();
            if(currentState != ResourceStates.RenderTarget)
            {
              p_stateTracker.TransitionResource(
                  colorTexture.GetResource(),
                  ResourceStates.RenderTarget,
                  0);

              Console.WriteLine($"[CommandBuffer] Transitioning color target {i}: {currentState} → RenderTarget");
            }
          }

          p_currentRenderTargets[i] = dx12View.GetRenderTargetView();
          p_renderTargetCount++;
        }
      }
    }

    if(_depthTarget is DX12TextureView depthView)
    {
      var depthTexture = depthView.Texture as DX12Texture;
      if(depthTexture != null)
      {
        var currentState = depthTexture.GetCurrentState();
        if(currentState != ResourceStates.DepthWrite)
        {
          p_stateTracker.TransitionResource(
              depthTexture.GetResource(),
              ResourceStates.DepthWrite,
              0);

          Console.WriteLine($"[CommandBuffer] Transitioning depth target: {currentState} → DepthWrite");
        }
      }

      p_currentDepthStencil = depthView.GetDepthStencilView();
    }
    else
    {
      p_currentDepthStencil = null;
    }

    p_stateTracker.FlushResourceBarriers(p_commandList);

    Console.WriteLine($"[CommandBuffer] Flushed resource barriers before setting render targets");

    ApplyRenderTargets();
  }

  private void ClearRenderTargetDirectly(ITextureView _target, Vector4 _color)
  {
    Console.WriteLine("[CommandBuffer] === ClearRenderTargetDirectly START ===");

    if(_target is not DX12TextureView dx12View)
      throw new ArgumentException("Invalid texture view type");

    var texture = dx12View.Texture as DX12Texture;
    if(texture != null)
    {
      var currentState = texture.GetCurrentState();
      Console.WriteLine($"[CommandBuffer] Clear target ({texture.Name}) current state: {currentState}");

      if(currentState != ResourceStates.RenderTarget)
      {
        Console.WriteLine($"[CommandBuffer] Transitioning for clear: {currentState} → RenderTarget");

        p_stateTracker.TransitionResource(
            texture.GetResource(),
            ResourceStates.RenderTarget,
            0);

        texture.SetCurrentState(ResourceStates.RenderTarget);

        Console.WriteLine("[CommandBuffer] Flushing barriers for clear...");
        p_stateTracker.FlushResourceBarriers(p_commandList);
      }
      else
      {
        Console.WriteLine("[CommandBuffer] Clear target already in RenderTarget state");
      }
    }

    var rtvHandle = dx12View.GetRenderTargetView();
    var colorArray = stackalloc float[4] { _color.X, _color.Y, _color.Z, _color.W };

    Console.WriteLine($"[CommandBuffer] Clearing render target to [{_color.X:F2}, {_color.Y:F2}, {_color.Z:F2}, {_color.W:F2}]");
    p_commandList->ClearRenderTargetView(rtvHandle, colorArray, 0, null);

    Console.WriteLine("[CommandBuffer] === ClearRenderTargetDirectly END ===\n");
  }

  private void ClearDepthStencilDirectly(ITextureView _target, GraphicsAPI.Enums.ClearFlags _flags, float _depth, byte _stencil)
  {
    if(_target is not DX12TextureView dx12View)
      throw new ArgumentException("Invalid texture view type");

    var d3d12Flags = ConvertClearFlags(_flags);
    var dsvHandle = dx12View.GetDepthStencilView();

    p_commandList->ClearDepthStencilView(dsvHandle, d3d12Flags, _depth, _stencil, 0, null);
  }

  private void SetViewportDirectly(Resources.Viewport _viewport)
  {
    Console.WriteLine($"\n[CommandBuffer] === SetViewportDirectly START ===");
    Console.WriteLine($"[CommandBuffer] Viewport X: {_viewport.X}");
    Console.WriteLine($"[CommandBuffer] Viewport Y: {_viewport.Y}");
    Console.WriteLine($"[CommandBuffer] Viewport Width: {_viewport.Width}");
    Console.WriteLine($"[CommandBuffer] Viewport Height: {_viewport.Height}");
    Console.WriteLine($"[CommandBuffer] Viewport MinDepth: {_viewport.MinDepth}");
    Console.WriteLine($"[CommandBuffer] Viewport MaxDepth: {_viewport.MaxDepth}");

    var d3d12Viewport = new Silk.NET.Direct3D12.Viewport
    {
      TopLeftX = _viewport.X,
      TopLeftY = _viewport.Y,
      Width = _viewport.Width,
      Height = _viewport.Height,
      MinDepth = _viewport.MinDepth,
      MaxDepth = _viewport.MaxDepth
    };

    Console.WriteLine($"[CommandBuffer] D3D12 Viewport TopLeftX: {d3d12Viewport.TopLeftX}");
    Console.WriteLine($"[CommandBuffer] D3D12 Viewport TopLeftY: {d3d12Viewport.TopLeftY}");
    Console.WriteLine($"[CommandBuffer] D3D12 Viewport Width: {d3d12Viewport.Width}");
    Console.WriteLine($"[CommandBuffer] D3D12 Viewport Height: {d3d12Viewport.Height}");

    p_commandList->RSSetViewports(1, &d3d12Viewport);
    Console.WriteLine($"[CommandBuffer] Called RSSetViewports");
    Console.WriteLine($"[CommandBuffer] === SetViewportDirectly END ===\n");
  }
  private void SetVertexBufferDirectly(IBufferView _buffer, uint _slot)
  {
    Console.WriteLine($"\n[CommandBuffer] === SetVertexBufferDirectly START ===");
    Console.WriteLine($"[CommandBuffer] Slot: {_slot}");
    Console.WriteLine($"[CommandBuffer] Buffer: {_buffer?.GetType().Name}");

    if(_buffer is not DX12BufferView dx12View)
    {
      Console.WriteLine($"[CommandBuffer] ERROR: Invalid buffer view type - expected DX12BufferView, got {_buffer?.GetType().Name}");
      throw new ArgumentException("Invalid buffer view type");
    }

    var buffer = dx12View.Buffer as DX12Buffer;
    Console.WriteLine($"[CommandBuffer] Buffer name: {buffer?.Name}");
    Console.WriteLine($"[CommandBuffer] Buffer size: {buffer?.Size} bytes");
    Console.WriteLine($"[CommandBuffer] Buffer current state: {buffer?.GetCurrentState()}");

    if(buffer != null)
    {
      var currentState = buffer.GetCurrentState();
      var resourcePtr = buffer.GetNativeHandle();

      if(currentState != ResourceStates.GenericRead &&
          currentState != ResourceStates.VertexAndConstantBuffer)
      {
        Console.WriteLine($"[CommandBuffer] WARNING: Vertex buffer in wrong state for IA: {currentState}");
        Console.WriteLine($"[CommandBuffer] Transitioning vertex buffer: {currentState} → GenericRead");

        p_stateTracker.TransitionResource(buffer.GetResource(), ResourceStates.GenericRead);
        buffer.SetCurrentState(ResourceStates.GenericRead);
      }
      else
      {
        Console.WriteLine($"[CommandBuffer] Vertex buffer in correct state: {currentState}");
      }
    }

    var vbView = dx12View.GetVertexBufferView();
    Console.WriteLine($"[CommandBuffer] VBView BufferLocation: 0x{vbView.BufferLocation:X16}");
    Console.WriteLine($"[CommandBuffer] VBView SizeInBytes: {vbView.SizeInBytes}");
    Console.WriteLine($"[CommandBuffer] VBView StrideInBytes: {vbView.StrideInBytes}");

    p_commandList->IASetVertexBuffers(_slot, 1, &vbView);
    Console.WriteLine($"[CommandBuffer] Called IASetVertexBuffers for slot {_slot}");
    Console.WriteLine($"[CommandBuffer] === SetVertexBufferDirectly END ===\n");
  }

  private void SetIndexBufferDirectly(IBufferView _buffer, IndexFormat _format)
  {
    Console.WriteLine($"\n[CommandBuffer] === SetIndexBufferDirectly START ===");
    Console.WriteLine($"[CommandBuffer] Format: {_format}");
    Console.WriteLine($"[CommandBuffer] Buffer: {_buffer?.GetType().Name}");

    if(_buffer is not DX12BufferView dx12View)
    {
      Console.WriteLine($"[CommandBuffer] ERROR: Invalid buffer view type - expected DX12BufferView, got {_buffer?.GetType().Name}");
      throw new ArgumentException("Invalid buffer view type");
    }

    var buffer = dx12View.Buffer as DX12Buffer;
    Console.WriteLine($"[CommandBuffer] Buffer name: {buffer?.Name}");
    Console.WriteLine($"[CommandBuffer] Buffer size: {buffer?.Size} bytes");
    Console.WriteLine($"[CommandBuffer] Buffer current state: {buffer?.GetCurrentState()}");

    if(buffer != null)
    {
      var currentState = buffer.GetCurrentState();

      if(currentState != ResourceStates.GenericRead &&
          currentState != ResourceStates.IndexBuffer)
      {
        Console.WriteLine($"[CommandBuffer] WARNING: Index buffer in wrong state for IA: {currentState}");
        Console.WriteLine($"[CommandBuffer] Transitioning index buffer: {currentState} → GenericRead");

        p_stateTracker.TransitionResource(buffer.GetResource(), ResourceStates.GenericRead);
        buffer.SetCurrentState(ResourceStates.GenericRead);
      }
      else
      {
        Console.WriteLine($"[CommandBuffer] Index buffer in correct state: {currentState}");
      }
    }

    var ibView = dx12View.GetIndexBufferView(_format);
    Console.WriteLine($"[CommandBuffer] IBView BufferLocation: 0x{ibView.BufferLocation:X16}");
    Console.WriteLine($"[CommandBuffer] IBView SizeInBytes: {ibView.SizeInBytes}");
    Console.WriteLine($"[CommandBuffer] IBView Format: {ibView.Format}");

    p_commandList->IASetIndexBuffer(&ibView);
    Console.WriteLine($"[CommandBuffer] Called IASetIndexBuffer");
    Console.WriteLine($"[CommandBuffer] === SetIndexBufferDirectly END ===\n");
  }

  private void SetComputeShaderDirectly(IShader _shader)
  {
    if(_shader is DX12Shader dx12Shader)
    {
      p_commandList->SetComputeRootSignature(p_currentRenderState.GetRootSignature());
      p_commandList->SetPipelineState(p_currentRenderState.GetPipelineState());
    }
  }

  private void SetVertexShaderDirectly(IShader _shader)
  {
    p_currentShaders[(int)ShaderStage.Vertex] = _shader;
    UpdateGraphicsPipelineState();
  }

  private void SetPixelShaderDirectly(IShader _shader)
  {
    p_currentShaders[(int)ShaderStage.Pixel] = _shader;
    UpdateGraphicsPipelineState();
  }

  private void SetConstantBufferDirectly(SetConstantBufferCommand _cmd)
  {

    Console.WriteLine($"\n[CommandBuffer] === SetConstantBufferDirectly START ===");
    Console.WriteLine($"[CommandBuffer] Stage: {_cmd.Stage}, Slot: {_cmd.Slot}");

    if(_cmd.Buffer is not DX12BufferView dx12View)
      throw new ArgumentException("Invalid buffer view type");

    if(p_currentRenderState == null)
      throw new InvalidOperationException("RenderState must be set before setting constant buffers");

    uint rootParamIndex = GetRootParameterIndex(_cmd.Stage, _cmd.Slot, ResourceType.ConstantBuffer);

    var gpuVirtualAddress = dx12View.GetResource()->GetGPUVirtualAddress();

    Console.WriteLine($"[CommandBuffer] Setting CBV at root parameter {rootParamIndex}");
    Console.WriteLine($"[CommandBuffer] GPU Virtual Address: 0x{gpuVirtualAddress:X16}");

    if(_cmd.Stage == ShaderStage.Compute)
    {
      Console.WriteLine($"[CommandBuffer] Using SetComputeRootConstantBufferView");
      p_commandList->SetComputeRootConstantBufferView(rootParamIndex, gpuVirtualAddress);
    }
    else
    {
      Console.WriteLine($"[CommandBuffer] Using SetGraphicsRootConstantBufferView");
      p_commandList->SetGraphicsRootConstantBufferView(rootParamIndex, gpuVirtualAddress);
    }

    Console.WriteLine($"[CommandBuffer] === SetConstantBufferDirectly END ===\n");
  }

  private void SetRenderStateDirectly(IRenderState _renderState)
  {
    Console.WriteLine($"\n[CommandBuffer] === SetRenderStateDirectly START ===");

    if(_renderState is not DX12RenderState dx12RenderState)
      throw new ArgumentException("Invalid render state type");

    p_currentRenderState = dx12RenderState;

    var pipelineState = dx12RenderState.GetPipelineState();
    var rootSignature = dx12RenderState.GetRootSignature();

    Console.WriteLine($"[CommandBuffer] Setting pipeline state: 0x{((IntPtr)pipelineState.Handle):X16}");
    p_commandList->SetPipelineState(pipelineState);

    if(Type == CommandBufferType.Compute)
    {
      Console.WriteLine($"[CommandBuffer] Setting compute root signature: 0x{((IntPtr)rootSignature.Handle):X16}");
      p_commandList->SetComputeRootSignature(rootSignature);
    }
    else
    {
      Console.WriteLine($"[CommandBuffer] Setting graphics root signature: 0x{((IntPtr)rootSignature.Handle):X16}");
      p_commandList->SetGraphicsRootSignature(rootSignature);
    }

    Console.WriteLine($"[CommandBuffer] === SetRenderStateDirectly END ===\n");
  }

  private void SetShaderResourceDirectly(SetShaderResourceCommand _cmd)
  {
    if(p_currentRenderState == null)
      throw new InvalidOperationException("RenderState must be set before setting shader resources");

    uint rootParamIndex = GetRootParameterIndex(_cmd.Stage, _cmd.Slot, ResourceType.ShaderResource);

    if(_cmd.Resource is DX12TextureView texView)
    {
      var srvHandle = texView.GetShaderResourceView();
    }
    else if(_cmd.Resource is DX12BufferView bufView)
    {
      var srvHandle = bufView.GetShaderResourceView();
    }
  }

  private void SetSamplerDirectly(SetSamplerCommand _cmd)
  {
    if(p_currentRenderState == null)
      throw new InvalidOperationException("RenderState must be set before setting samplers");

    if(_cmd.Sampler is not DX12Sampler dx12Sampler)
      throw new ArgumentException("Invalid sampler type");

    uint rootParamIndex = GetRootParameterIndex(_cmd.Stage, _cmd.Slot, ResourceType.Sampler);

    var samplerHandle = dx12Sampler.GetDescriptorHandle();
  }

  private void SetUnorderedAccessDirectly(SetUnorderedAccessCommand _cmd)
  {
    if(p_currentRenderState == null)
      throw new InvalidOperationException("RenderState must be set before setting UAVs");

    uint rootParamIndex = GetRootParameterIndex(_cmd.Stage, _cmd.Slot, ResourceType.UnorderedAccess);
  }

  #endregion

  private uint GetRootParameterIndex(ShaderStage _stage, uint _slot, ResourceType _resourceType)
  {

    // Root Parameter 0: Root CBV b0 (Vertex visibility)  
    // Root Parameter 1: Root CBV b1 (All visibility)

    Console.WriteLine($"[CommandBuffer] GetRootParameterIndex: Stage={_stage}, Slot={_slot}, Type={_resourceType}");

    if(_resourceType == ResourceType.ConstantBuffer)
    {
      // Для константных буферов используем прямое отображение slot → root parameter
      // b0 → Root Parameter 0
      // b1 → Root Parameter 1
      uint rootParamIndex = _slot;

      Console.WriteLine($"[CommandBuffer] Mapped CB slot {_slot} to root parameter {rootParamIndex}");
      return rootParamIndex;
    }

    switch(_resourceType)
    {
      case ResourceType.ShaderResource:
        Console.WriteLine($"[CommandBuffer] SRV resources not supported in basic layout");
        throw new NotSupportedException("SRV resources require CreateBasicGraphics layout");

      case ResourceType.UnorderedAccess:
        Console.WriteLine($"[CommandBuffer] UAV resources not supported in basic layout");
        throw new NotSupportedException("UAV resources require compute layout");

      case ResourceType.Sampler:
        Console.WriteLine($"[CommandBuffer] Sampler resources not supported in basic layout");
        throw new NotSupportedException("Sampler resources require CreateBasicGraphics layout");

      default:
        Console.WriteLine($"[CommandBuffer] Unsupported resource type: {_resourceType}");
        throw new ArgumentException($"Unsupported resource type: {_resourceType}");
    }
  }

  private void UpdateGraphicsPipelineState()
  {
    // Создание/обновление graphics pipeline state на основе текущих шейдеров
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
    p_currentRenderState = null;
    p_renderTargetCount = 0;
    p_currentDepthStencil = null;
    Array.Clear(p_currentRenderTargets);
  }

  private void CreateCommandListAndAllocator()
  {
    var listType = Type switch
    {
      CommandBufferType.Direct => CommandListType.Direct,
      CommandBufferType.Compute => CommandListType.Compute,
      CommandBufferType.Copy => CommandListType.Copy,
      CommandBufferType.Bundle => CommandListType.Bundle,
      _ => CommandListType.Direct
    };

    ID3D12CommandAllocator* allocator;
    HResult hr = p_device->CreateCommandAllocator(
        listType,
        SilkMarshal.GuidPtrOf<ID3D12CommandAllocator>(),
        (void**)&allocator);

    DX12Helpers.ThrowIfFailed(hr, "Failed to create command allocator");

    p_commandAllocator = allocator;

    ID3D12GraphicsCommandList* commandList;
    hr = p_device->CreateCommandList(
        0, // Node mask
        listType,
        p_commandAllocator,
        null, // Initial pipeline state
        SilkMarshal.GuidPtrOf<ID3D12GraphicsCommandList>(),
        (void**)&commandList);

    DX12Helpers.ThrowIfFailed(hr, "Failed to create command list");
    p_commandList = commandList;

    p_commandList->Close();
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
    if(!(p_executionMode == CommandBufferExecutionMode.Immediate))
      throw new InvalidOperationException("Cannot switch to immediate mode - buffer was created in deferred mode");
  }

  /// <summary>
  /// Выполнить все накопленные команды (для deferred режима)
  /// </summary>
  public override void Execute()
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      return;
    }
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
      ExecutionMode = p_executionMode,
      NativeCommandListPtr = (IntPtr)p_commandList
    };
  }

  public override void SetPrimitiveTopology(PrimitiveTopology _topology)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      SetPrimitiveTopologyDirectly(_topology);
    }
    else
    {
      base.SetPrimitiveTopology(_topology);
    }
  }

  private void SetPrimitiveTopologyDirectly(PrimitiveTopology _topology)
  {
    Console.WriteLine($"\n[CommandBuffer] === SetPrimitiveTopologyDirectly START ===");
    Console.WriteLine($"[CommandBuffer] Topology: {_topology}");

    var d3d12Topology = _topology.ConvertToCmd();
    Console.WriteLine($"[CommandBuffer] D3D12 Topology: {d3d12Topology}");

    p_commandList->IASetPrimitiveTopology(d3d12Topology);
    Console.WriteLine($"[CommandBuffer] Called IASetPrimitiveTopology");
    Console.WriteLine($"[CommandBuffer] === SetPrimitiveTopologyDirectly END ===\n");
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

  private DX12DescriptorHeapManager GetDescriptorManager() => p_parentDevice.GetDescriptorManager();

  private unsafe void SetupDescriptorHeaps()
  {
    var descriptorManager = GetDescriptorManager();
    if(descriptorManager == null)
      return;

    var heaps = stackalloc ID3D12DescriptorHeap*[2];
    uint heapCount = 0;

    var cbvSrvUavHeap = descriptorManager.GetGPUCBVSRVUAVHeap();
    if(cbvSrvUavHeap != null)
    {
      heaps[heapCount++] = cbvSrvUavHeap;
    }

    var samplerHeap = descriptorManager.GetGPUSamplerHeap();
    if(samplerHeap != null)
    {
      heaps[heapCount++] = samplerHeap;
    }

    if(heapCount > 0)
    {
      p_commandList->SetDescriptorHeaps(heapCount, heaps);
    }
  }

  #region Event/Marker Methods

  private void BeginEvent(string _name)
  {
    // Можно реализовать через PIX events или другие профайлеры
    Console.WriteLine($"[DX12] Begin Event: {_name}");
  }

  private void EndEvent()
  {
    Console.WriteLine("[DX12] End Event");
  }

  private void SetMarker(string _name)
  {
    Console.WriteLine($"[DX12] Marker: {_name}");
  }

  #endregion
}

