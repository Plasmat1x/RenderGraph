using GraphicsAPI.Commands;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using System.Numerics;


namespace GraphicsAPI;
/// <summary>
/// Универсальная реализация командного буфера, которая может служить основой для различных API
/// </summary>
public abstract class GenericCommandBuffer: CommandBuffer
{
  protected readonly CommandBufferType p_type;
  protected bool p_isRecording;
  protected readonly List<ICommand> p_commands;
  protected readonly Dictionary<string, object> p_state;

  // Current pipeline state tracking
  protected ITextureView[] p_currentRenderTargets;
  protected ITextureView p_currentDepthTarget;
  protected Viewport p_currentViewport;
  protected Rectangle p_currentScissorRect;
  protected PrimitiveTopology p_currentTopology;
  protected readonly IShader[] p_currentShaders; // VS, PS, GS, HS, DS, CS

  protected GenericCommandBuffer(CommandBufferType _type)
  {
    p_type = _type;
    p_commands = new List<ICommand>();
    p_state = new Dictionary<string, object>();
    p_currentShaders = new IShader[6];
    p_currentRenderTargets = Array.Empty<ITextureView>();
  }

  public override bool IsRecording { get; protected set; }
  public override CommandBufferType Type => p_type;

  /// <summary>
  /// Получить количество записанных команд
  /// </summary>
  public int CommandCount => p_commands.Count;

  /// <summary>
  /// Получить все записанные команды (для отладки)
  /// </summary>
  public IReadOnlyList<ICommand> Commands => p_commands.AsReadOnly();

  // === Основные операции ===
  public override void Begin()
  {
    if(IsRecording)
      throw new InvalidOperationException("Command buffer is already recording");

    IsRecording = true;
    p_commands.Clear();
    p_state.Clear();
    ResetState();

    OnBegin();
  }

  public override void End()
  {
    if(!IsRecording)
      throw new InvalidOperationException("Command buffer is not recording");

    IsRecording = false;
    OnEnd();
  }

  public override void Reset()
  {
    if(IsRecording)
      throw new InvalidOperationException("Cannot reset while recording");

    p_commands.Clear();
    p_state.Clear();
    ResetState();
    OnReset();
  }

  // === Protected virtual methods для переопределения в наследниках ===
  protected virtual void OnBegin() { }
  protected virtual void OnEnd() { }
  protected virtual void OnReset() { }
  protected abstract void ExecuteCommand(ICommand _command);

  /// <summary>
  /// Выполнить все записанные команды
  /// </summary>
  public virtual void Execute()
  {
    if(IsRecording)
      throw new InvalidOperationException("Cannot execute while recording");

    foreach(var command in p_commands)
    {
      ExecuteCommand(command);
    }
  }

  // === Command Recording ===
  protected void RecordCommand(ICommand _command)
  {
    if(!IsRecording)
      throw new InvalidOperationException("Command buffer is not recording");

    p_commands.Add(_command);
  }

  protected void ValidateRecording()
  {
    if(!IsRecording)
      throw new InvalidOperationException("Command buffer is not recording");
  }

  protected void ResetState()
  {
    p_currentRenderTargets = Array.Empty<ITextureView>();
    p_currentDepthTarget = null;
    p_currentTopology = PrimitiveTopology.Undefined;
    Array.Clear(p_currentShaders);
  }

  // === Render Targets ===
  public override void SetRenderTargets(ITextureView[] _colorTargets, ITextureView _depthTarget)
  {
    ValidateRecording();

    p_currentRenderTargets = _colorTargets ?? Array.Empty<ITextureView>();
    p_currentDepthTarget = _depthTarget;

    RecordCommand(new SetRenderTargetsCommand(_colorTargets, _depthTarget));
  }

  public override void SetRenderTarget(ITextureView _colorTarget, ITextureView _depthTarget = null)
  {
    SetRenderTargets(_colorTarget != null ? new[] { _colorTarget } : null, _depthTarget);
  }

  // === Viewport и Scissor ===
  public override void SetViewport(Viewport _viewport)
  {
    ValidateRecording();
    p_currentViewport = _viewport;
    RecordCommand(new SetViewportCommand(_viewport));
  }

  public override void SetViewports(Viewport[] _viewports)
  {
    ValidateRecording();
    RecordCommand(new SetViewportsCommand(_viewports));
  }

  public override void SetScissorRect(Rectangle _rect)
  {
    ValidateRecording();
    p_currentScissorRect = _rect;
    RecordCommand(new SetScissorRectCommand(_rect));
  }

  public override void SetScissorRects(Rectangle[] _rects)
  {
    ValidateRecording();
    RecordCommand(new SetScissorRectsCommand(_rects));
  }

  // === Clear операции ===
  public override void ClearRenderTarget(ITextureView _target, Vector4 _color)
  {
    ValidateRecording();
    RecordCommand(new ClearRenderTargetCommand(_target, _color));
  }

  public override void ClearDepthStencil(ITextureView _target, ClearFlags _flags, float _depth, byte _stencil)
  {
    ValidateRecording();
    RecordCommand(new ClearDepthStencilCommand(_target, _flags, _depth, _stencil));
  }

  public override void ClearUnorderedAccess(ITextureView _target, Vector4 _value)
  {
    ValidateRecording();
    RecordCommand(new ClearUnorderedAccessTextureCommand(_target, _value));
  }

  public override void ClearUnorderedAccess(IBufferView _target, uint _value)
  {
    ValidateRecording();
    RecordCommand(new ClearUnorderedAccessBufferCommand(_target, _value));
  }

  // === Resource State Transitions ===
  public override void TransitionResource(IResource _resource, ResourceState _newState)
  {
    ValidateRecording();
    RecordCommand(new TransitionResourceCommand(_resource, _newState));
  }

  public override void TransitionResources(IResource[] _resources, ResourceState[] _newStates)
  {
    ValidateRecording();
    RecordCommand(new TransitionResourcesCommand(_resources, _newStates));
  }

  // === UAV Barriers ===
  public override void UAVBarrier(IResource _resource)
  {
    ValidateRecording();
    RecordCommand(new UAVBarrierCommand(_resource));
  }

  public override void UAVBarriers(IResource[] _resources)
  {
    ValidateRecording();
    RecordCommand(new UAVBarriersCommand(_resources));
  }

  // === Vertex/Index Buffers ===
  public override void SetVertexBuffer(IBufferView _buffer, uint _slot = 0)
  {
    ValidateRecording();
    RecordCommand(new SetVertexBufferCommand(_buffer, _slot));
  }

  public override void SetVertexBuffers(IBufferView[] _buffers, uint _startSlot = 0)
  {
    ValidateRecording();
    RecordCommand(new SetVertexBuffersCommand(_buffers, _startSlot));
  }

  public override void SetIndexBuffer(IBufferView _buffer, IndexFormat _format)
  {
    ValidateRecording();
    RecordCommand(new SetIndexBufferCommand(_buffer, _format));
  }

  // === Shaders ===
  public override void SetVertexShader(IShader _shader)
  {
    ValidateRecording();
    p_currentShaders[0] = _shader;
    RecordCommand(new SetShaderCommand(ShaderStage.Vertex, _shader));
  }

  public override void SetPixelShader(IShader _shader)
  {
    ValidateRecording();
    p_currentShaders[1] = _shader;
    RecordCommand(new SetShaderCommand(ShaderStage.Pixel, _shader));
  }

  public override void SetComputeShader(IShader _shader)
  {
    ValidateRecording();
    p_currentShaders[5] = _shader;
    RecordCommand(new SetShaderCommand(ShaderStage.Compute, _shader));
  }

  public override void SetGeometryShader(IShader _shader)
  {
    ValidateRecording();
    p_currentShaders[2] = _shader;
    RecordCommand(new SetShaderCommand(ShaderStage.Geometry, _shader));
  }

  public override void SetHullShader(IShader _shader)
  {
    ValidateRecording();
    p_currentShaders[3] = _shader;
    RecordCommand(new SetShaderCommand(ShaderStage.Hull, _shader));
  }

  public override void SetDomainShader(IShader _shader)
  {
    ValidateRecording();
    p_currentShaders[4] = _shader;
    RecordCommand(new SetShaderCommand(ShaderStage.Domain, _shader));
  }

  // === Shader Resources ===
  public override void SetShaderResource(ShaderStage _stage, uint _slot, ITextureView _resource)
  {
    ValidateRecording();
    RecordCommand(new SetShaderResourceCommand(_stage, _slot, _resource));
  }

  public override void SetShaderResources(ShaderStage _stage, uint _startSlot, ITextureView[] _resources)
  {
    ValidateRecording();
    RecordCommand(new SetShaderResourcesCommand(_stage, _startSlot, _resources));
  }

  // === Unordered Access ===
  public override void SetUnorderedAccess(ShaderStage _stage, uint _slot, ITextureView _resource)
  {
    ValidateRecording();
    RecordCommand(new SetUnorderedAccessCommand(_stage, _slot, _resource));
  }

  public override void SetUnorderedAccesses(ShaderStage _stage, uint _startSlot, ITextureView[] _resources)
  {
    ValidateRecording();
    RecordCommand(new SetUnorderedAccessesCommand(_stage, _startSlot, _resources));
  }

  // === Constant Buffers ===
  public override void SetConstantBuffer(ShaderStage _stage, uint _slot, IBufferView _buffer)
  {
    ValidateRecording();
    RecordCommand(new SetConstantBufferCommand(_stage, _slot, _buffer));
  }

  public override void SetConstantBuffers(ShaderStage _stage, uint _startSlot, IBufferView[] _buffers)
  {
    ValidateRecording();
    RecordCommand(new SetConstantBuffersCommand(_stage, _startSlot, _buffers));
  }

  // === Samplers ===
  public override void SetSampler(ShaderStage _stage, uint _slot, ISampler _sampler)
  {
    ValidateRecording();
    RecordCommand(new SetSamplerCommand(_stage, _slot, _sampler));
  }

  public override void SetSamplers(ShaderStage _stage, uint _startSlot, ISampler[] _samplers)
  {
    ValidateRecording();
    RecordCommand(new SetSamplersCommand(_stage, _startSlot, _samplers));
  }

  // === Render States ===
  public override void SetRenderState(IRenderState _renderState)
  {
    ValidateRecording();
    RecordCommand(new SetRenderStateCommand(_renderState));
  }

  public override void SetBlendState(IBlendState _blendState, Vector4 _blendFactor, uint _sampleMask = 0xffffffff)
  {
    ValidateRecording();
    RecordCommand(new SetBlendStateCommand(_blendState, _blendFactor, _sampleMask));
  }

  public override void SetDepthStencilState(IDepthStencilState _depthStencilState, uint _stencilRef = 0)
  {
    ValidateRecording();
    RecordCommand(new SetDepthStencilStateCommand(_depthStencilState, _stencilRef));
  }

  public override void SetRasterizerState(IRasterizerState _rasterizerState)
  {
    ValidateRecording();
    RecordCommand(new SetRasterizerStateCommand(_rasterizerState));
  }

  public override void SetPrimitiveTopology(PrimitiveTopology _topology)
  {
    ValidateRecording();
    p_currentTopology = _topology;
    RecordCommand(new SetPrimitiveTopologyCommand(_topology));
  }

  // === Draw Commands ===
  public override void Draw(uint _vertexCount, uint _instanceCount = 1, uint _startVertex = 0, uint _startInstance = 0)
  {
    ValidateRecording();
    RecordCommand(new DrawCommand(_vertexCount, _instanceCount, _startVertex, _startInstance));
  }

  public override void DrawIndexed(uint _indexCount, uint _instanceCount = 1, uint _startIndex = 0, int _baseVertex = 0, uint _startInstance = 0)
  {
    ValidateRecording();
    RecordCommand(new DrawIndexedCommand(_indexCount, _instanceCount, _startIndex, _baseVertex, _startInstance));
  }

  public override void DrawIndirect(IBufferView _argsBuffer, ulong _offset = 0)
  {
    ValidateRecording();
    RecordCommand(new DrawIndirectCommand(_argsBuffer, _offset));
  }

  public override void DrawIndexedIndirect(IBufferView _argsBuffer, ulong _offset = 0)
  {
    ValidateRecording();
    RecordCommand(new DrawIndexedIndirectCommand(_argsBuffer, _offset));
  }

  // === Compute Commands ===
  public override void Dispatch(uint _groupCountX, uint _groupCountY = 1, uint _groupCountZ = 1)
  {
    ValidateRecording();
    RecordCommand(new DispatchCommand(_groupCountX, _groupCountY, _groupCountZ));
  }

  public override void DispatchIndirect(IBufferView _argsBuffer, ulong _offset = 0)
  {
    ValidateRecording();
    RecordCommand(new DispatchIndirectCommand(_argsBuffer, _offset));
  }

  // === Copy Operations ===
  public override void CopyTexture(ITexture _src, ITexture _dst)
  {
    ValidateRecording();
    RecordCommand(new CopyTextureCommand(_src, _dst));
  }

  public override void CopyTextureRegion(ITexture _src, uint _srcMip, uint _srcArray, Box _srcBox, ITexture _dst, uint _dstMip, uint _dstArray, Point3D _dstOffset)
  {
    ValidateRecording();
    RecordCommand(new CopyTextureRegionCommand(_src, _srcMip, _srcArray, _srcBox, _dst, _dstMip, _dstArray, _dstOffset));
  }

  public override void CopyBuffer(IBuffer _src, IBuffer _dst)
  {
    ValidateRecording();
    RecordCommand(new CopyBufferCommand(_src, _dst));
  }

  public override void CopyBufferRegion(IBuffer _src, ulong _srcOffset, IBuffer _dst, ulong _dstOffset, ulong _size)
  {
    ValidateRecording();
    RecordCommand(new CopyBufferRegionCommand(_src, _srcOffset, _dst, _dstOffset, _size));
  }

  public override void ResolveTexture(ITexture _src, uint _srcArray, ITexture _dst, uint _dstArray, TextureFormat _format)
  {
    ValidateRecording();
    RecordCommand(new ResolveTextureCommand(_src, _srcArray, _dst, _dstArray, _format));
  }

  // === Queries ===
  public override void BeginQuery(IQuery _query)
  {
    ValidateRecording();
    RecordCommand(new BeginQueryCommand(_query));
  }

  public override void EndQuery(IQuery _query)
  {
    ValidateRecording();
    RecordCommand(new EndQueryCommand(_query));
  }

  // === Debug ===
  public override void PushDebugGroup(string _name)
  {
    ValidateRecording();
    RecordCommand(new PushDebugGroupCommand(_name));
  }

  public override void PopDebugGroup()
  {
    ValidateRecording();
    RecordCommand(new PopDebugGroupCommand());
  }

  public override void InsertDebugMarker(string _name)
  {
    ValidateRecording();
    RecordCommand(new InsertDebugMarkerCommand(_name));
  }

  // === State Queries ===

  /// <summary>
  /// Получить текущие render targets
  /// </summary>
  public IReadOnlyList<ITextureView> GetCurrentRenderTargets() => p_currentRenderTargets;

  /// <summary>
  /// Получить текущий depth target
  /// </summary>
  public ITextureView GetCurrentDepthTarget() => p_currentDepthTarget;

  /// <summary>
  /// Получить текущую primitive topology
  /// </summary>
  public PrimitiveTopology GetCurrentTopology() => p_currentTopology;

  /// <summary>
  /// Получить текущий шейдер для указанной стадии
  /// </summary>
  public IShader GetCurrentShader(ShaderStage _stage)
  {
    var index = _stage switch
    {
      ShaderStage.Vertex => 0,
      ShaderStage.Pixel => 1,
      ShaderStage.Geometry => 2,
      ShaderStage.Hull => 3,
      ShaderStage.Domain => 4,
      ShaderStage.Compute => 5,
      _ => -1
    };

    return index >= 0 ? p_currentShaders[index] : null;
  }

  /// <summary>
  /// Проверить, установлены ли все необходимые шейдеры для graphics pipeline
  /// </summary>
  public bool IsGraphicsPipelineValid()
  {
    return p_currentShaders[0] != null && // Vertex shader обязателен
           p_currentRenderTargets.Length > 0; // Render target обязателен
  }

  /// <summary>
  /// Проверить, установлен ли compute shader
  /// </summary>
  public bool IsComputePipelineValid()
  {
    return p_currentShaders[5] != null;
  }

  // === Utility Methods ===

  /// <summary>
  /// Получить статистику командного буфера
  /// </summary>
  public CommandBufferStats GetStats()
  {
    var stats = new CommandBufferStats();

    foreach(var command in p_commands)
    {
      switch(command)
      {
        case DrawCommand _:
        case DrawIndexedCommand _:
        case DrawIndirectCommand _:
        case DrawIndexedIndirectCommand _:
          stats.DrawCalls++;
          break;
        case DispatchCommand _:
        case DispatchIndirectCommand _:
          stats.DispatchCalls++;
          break;
        case SetShaderResourceCommand _:
          stats.ResourceBindings += 1;
          break;
        case SetShaderResourcesCommand cmd:
          stats.ResourceBindings += cmd.Resources?.Length ?? 1;
          break;
        case TransitionResourceCommand _:
          stats.ResourceTransitions += 1;
          break;
        case TransitionResourcesCommand cmd:
          stats.ResourceTransitions += cmd.Resources?.Length ?? 1;
          break;
      }
    }

    stats.TotalCommands = p_commands.Count;
    return stats;
  }

  /// <summary>
  /// Оптимизировать командный буфер (удалить избыточные команды)
  /// </summary>
  public virtual void Optimize()
  {
    if(IsRecording)
      throw new InvalidOperationException("Cannot optimize while recording");

    // Простая оптимизация - удаление дублирующихся state changes
    var optimizedCommands = new List<ICommand>();
    var lastStateCommands = new Dictionary<Type, ICommand>();

    foreach(var command in p_commands)
    {
      var commandType = command.GetType();

      // Для state команд - сохраняем только последнюю
      if(IsStateCommand(command))
      {
        lastStateCommands[commandType] = command;
      }
      else
      {
        // Для non-state команд - добавляем все накопленные state changes, затем саму команду
        foreach(var stateCommand in lastStateCommands.Values)
        {
          optimizedCommands.Add(stateCommand);
        }
        lastStateCommands.Clear();

        optimizedCommands.Add(command);
      }
    }

    // Добавляем оставшиеся state commands
    foreach(var stateCommand in lastStateCommands.Values)
    {
      optimizedCommands.Add(stateCommand);
    }

    p_commands.Clear();
    p_commands.AddRange(optimizedCommands);
  }

  private static bool IsStateCommand(ICommand _command)
  {
    return _command is SetViewportCommand ||
           _command is SetRenderTargetsCommand ||
           _command is SetShaderCommand ||
           _command is SetRenderStateCommand ||
           _command is SetPrimitiveTopologyCommand ||
           _command is SetBlendStateCommand ||
           _command is SetDepthStencilStateCommand ||
           _command is SetRasterizerStateCommand;
  }

  public override void Dispose()
  {
    p_commands.Clear();
    p_state.Clear();
    ResetState();
  }
}