

using Directx12Impl.Extensions;
using Directx12Impl.Parts.Data;
using Directx12Impl.Parts.Managers;
using Directx12Impl.Parts.Utils;

using GraphicsAPI;
using GraphicsAPI.Commands;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

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

    // Reset command list
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
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
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
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
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

  public void TransitionBackBufferForPresent(DX12Texture _backBuffer)
  {
    p_stateTracker.TransitionResource(
      _backBuffer.GetResource(),
      ResourceStates.Present,
      0);

    p_stateTracker.FlushResourceBarriers(p_commandList);
  }


  // === Методы прямого выполнения ===

  /// <summary>
  /// Немедленное выполнение команды (оптимизированное)
  /// </summary>
  private void ExecuteCommandImmediate(ICommand _command)
  {
    // Оптимизированные пути для часто используемых команд
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

      default:
        ExecuteCommandGeneric(_command);
        break;
    }
  }

  /// <summary>
  /// Отложенное выполнение команды (с возможностью группировки и оптимизации)
  /// </summary>
  private void ExecuteCommandDeferred(ICommand _command)
  {
    // В deferred режиме можем использовать более сложную логику
    // Например, группировку команд, дополнительную оптимизацию и т.д.

    // Пока что используем простое выполнение, но здесь можно добавить:
    // - Группировку state changes
    // - Batch обработку ресурсов
    // - Оптимизацию переходов состояний
    ExecuteCommandImmediate(_command);
  }

  /// <summary>
  /// Общее выполнение команды через переопределенные методы базового класса
  /// </summary>
  private void ExecuteCommandGeneric(ICommand _command)
  {
    // Используем методы базового GenericCommandBuffer для неоптимизированных команд
    // Эти методы автоматически создают соответствующие команды и добавляют их в список
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
    p_renderTargetCount = 0;

    if(_colorTarget is DX12TextureView colorView)
    {
      var colorTexture = colorView.Texture as DX12Texture;

      // ДОБАВИТЬ: Resource barrier для color target
      if(colorTexture != null)
      {
        p_stateTracker.TransitionResource(
          colorTexture.GetResource(),
          ResourceStates.RenderTarget,
          0);
      }

      p_currentRenderTargets[0] = colorView.GetRenderTargetView();
      p_renderTargetCount = 1;
    }

    if(_depthTarget is DX12TextureView depthView)
    {
      var depthTexture = depthView.Texture as DX12Texture;

      // ДОБАВИТЬ: Resource barrier для depth target
      if(depthTexture != null)
      {
        p_stateTracker.TransitionResource(
          depthTexture.GetResource(),
          ResourceStates.DepthWrite,
          0);
      }

      p_currentDepthStencil = depthView.GetDepthStencilView();
    }
    else
    {
      p_currentDepthStencil = null;
    }

    // ВАЖНО: Flush barriers перед установкой render targets
    p_stateTracker.FlushResourceBarriers(p_commandList);

    // Теперь устанавливаем render targets
    ApplyRenderTargets();
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
          p_currentRenderTargets[i] = dx12View.GetRenderTargetView();
          p_renderTargetCount++;
        }
      }
    }

    if(_depthTarget is DX12TextureView depthView)
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
    if(_target is not DX12TextureView dx12View)
      throw new ArgumentException("Invalid texture view type");

    var texture = dx12View.Texture as DX12Texture;
    if(texture != null)
    {
      p_stateTracker.TransitionResource(
        texture.GetResource(),
        ResourceStates.RenderTarget,
        0);

      p_stateTracker.FlushResourceBarriers(p_commandList);
    }

    var rtvHandle = dx12View.GetRenderTargetView();
    var colorArray = stackalloc float[4] { _color.X, _color.Y, _color.Z, _color.W };

    p_commandList->ClearRenderTargetView(rtvHandle, colorArray, 0, null);
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
    // Vertex shader устанавливается через pipeline state
    // Здесь мы сохраняем ссылку для последующего создания PSO
    p_currentShaders[(int)ShaderStage.Vertex] = _shader;
    UpdateGraphicsPipelineState();
  }

  private void SetPixelShaderDirectly(IShader _shader)
  {
    // Pixel shader устанавливается через pipeline state
    p_currentShaders[(int)ShaderStage.Pixel] = _shader;
    UpdateGraphicsPipelineState();
  }

  private void SetConstantBufferDirectly(SetConstantBufferCommand _cmd)
  {
    if(p_currentRenderState == null)
      throw new InvalidOperationException("RenderState must be set before setting constant buffers");

    if(_cmd.Buffer is not DX12BufferView dx12View)
      throw new ArgumentException("Invalid buffer view type");

    // Используем информацию из RenderState для правильной установки CBV
    var rootSignature = p_currentRenderState.GetRootSignature();
    var cbvHandle = dx12View.GetConstantBufferView();

    // Определяем root parameter index на основе stage и slot
    uint rootParamIndex = GetRootParameterIndex(_cmd.Stage, _cmd.Slot, ResourceType.ConstantBuffer);

    if(_cmd.Stage == ShaderStage.Compute)
      p_commandList->SetComputeRootConstantBufferView(rootParamIndex, dx12View.GetResource()->GetGPUVirtualAddress());
    else
      p_commandList->SetGraphicsRootConstantBufferView(rootParamIndex, dx12View.GetResource()->GetGPUVirtualAddress());
  }

  private void SetRenderStateDirectly(IRenderState _renderState)
  {
    if(_renderState is not DX12RenderState dx12RenderState)
      throw new ArgumentException("Invalid render state type");

    // Сохраняем текущий RenderState
    p_currentRenderState = dx12RenderState;

    // Применяем Pipeline State и Root Signature из RenderState
    var pipelineState = dx12RenderState.GetPipelineState();
    var rootSignature = dx12RenderState.GetRootSignature();

    p_commandList->SetPipelineState(pipelineState);

    if(Type == CommandBufferType.Compute)
      p_commandList->SetComputeRootSignature(rootSignature);
    else
      p_commandList->SetGraphicsRootSignature(rootSignature);
  }

  private void SetShaderResourceDirectly(SetShaderResourceCommand _cmd)
  {
    if(p_currentRenderState == null)
      throw new InvalidOperationException("RenderState must be set before setting shader resources");

    // Определяем root parameter index
    uint rootParamIndex = GetRootParameterIndex(_cmd.Stage, _cmd.Slot, ResourceType.ShaderResource);

    if(_cmd.Resource is DX12TextureView texView)
    {
      var srvHandle = texView.GetShaderResourceView();
      // Здесь нужно использовать descriptor table
      // Это требует GPU-visible descriptor heap
    }
    else if(_cmd.Resource is DX12BufferView bufView)
    {
      var srvHandle = bufView.GetShaderResourceView();
      // Аналогично
    }
  }

  private void SetSamplerDirectly(SetSamplerCommand _cmd)
  {
    if(p_currentRenderState == null)
      throw new InvalidOperationException("RenderState must be set before setting samplers");

    if(_cmd.Sampler is not DX12Sampler dx12Sampler)
      throw new ArgumentException("Invalid sampler type");

    uint rootParamIndex = GetRootParameterIndex(_cmd.Stage, _cmd.Slot, ResourceType.Sampler);

    // Samplers также требуют descriptor table
    var samplerHandle = dx12Sampler.GetDescriptorHandle();
    // Установка через descriptor table
  }

  private void SetUnorderedAccessDirectly(SetUnorderedAccessCommand _cmd)
  {
    if(p_currentRenderState == null)
      throw new InvalidOperationException("RenderState must be set before setting UAVs");

    uint rootParamIndex = GetRootParameterIndex(_cmd.Stage, _cmd.Slot, ResourceType.UnorderedAccess);

    // UAV также через descriptor table
  }

  #endregion

  private uint GetRootParameterIndex(ShaderStage _stage, uint _slot, ResourceType _resourceType)
  {
    // Здесь должна быть логика маппинга на основе root signature layout
    // Пока используем простую схему:
    // 0-3: Constant Buffers
    // 4-7: Shader Resources  
    // 8-11: UAVs
    // 12-15: Samplers

    uint baseIndex = _resourceType switch
    {
      ResourceType.ConstantBuffer => 0,
      ResourceType.ShaderResource => 4,
      ResourceType.UnorderedAccess => 8,
      ResourceType.Sampler => 12,
      _ => throw new ArgumentException($"Unsupported resource type: {_resourceType}")
    };

    return baseIndex + _slot;
  }

  private void UpdateGraphicsPipelineState()
  {
    // Создание/обновление graphics pipeline state на основе текущих шейдеров
    // Это более сложная логика, которая требует PSO cache
    // Пока что оставляем заглушку
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
    // Логика переключения
  }

  /// <summary>
  /// Выполнить все накопленные команды (для deferred режима)
  /// </summary>
  public override void Execute()
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
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

  public override void SetIndexBuffer(IBufferView _buffer, IndexFormat _format)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      SetIndexBufferDirectly(_buffer, _format);
    }
    else
    {
      base.SetIndexBuffer(_buffer, _format);
    }
  }

  public override void SetVertexBuffer(IBufferView _buffer, uint _slot)
  {
    if(p_executionMode == CommandBufferExecutionMode.Immediate)
    {
      ValidateRecording();
      SetVertexBufferDirectly(_buffer, _slot);
    }
    else
    {
      base.SetVertexBuffer(_buffer, _slot);
    }
  }

  // Добавить private implementation методы:
  private void SetPrimitiveTopologyDirectly(PrimitiveTopology _topology)
  {
    var d3d12Topology = _topology.ConvertToCmd();
    p_commandList->IASetPrimitiveTopology(d3d12Topology);
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
    // Получаем descriptor manager из parent device
    var descriptorManager = GetDescriptorManager();
    if(descriptorManager == null)
      return;

    // Устанавливаем descriptor heaps для CBV/SRV/UAV и Samplers
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
    // Пока что простая заглушка
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

