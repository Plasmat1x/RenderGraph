using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace GraphicsAPI;

/// <summary>
/// Базовый класс command buffer с общей функциональностью
/// </summary>
public abstract class CommandBuffer: IDisposable
{
  public abstract bool IsRecording { get; protected set; }
  public abstract CommandBufferType Type { get; }
  public string Name { get; set; } = string.Empty;

  // === Основные операции ===
  public abstract void Begin();
  public abstract void End();
  public abstract void Reset();

  // === Render Targets ===
  public abstract void SetRenderTargets(ITextureView[] _colorTargets, ITextureView _depthTarget);
  public abstract void SetRenderTarget(ITextureView _colorTarget, ITextureView _depthTarget = null);

  // === Viewport и Scissor ===
  public abstract void SetViewport(Viewport _viewport);
  public abstract void SetViewports(Viewport[] _viewports);
  public abstract void SetScissorRect(Rectangle _rect);
  public abstract void SetScissorRects(Rectangle[] _rects);

  // === Clear операции ===
  public abstract void ClearRenderTarget(ITextureView _target, Vector4 _color);
  public abstract void ClearDepthStencil(ITextureView _target, ClearFlags _flags, float _depth, byte _stencil);
  public abstract void ClearUnorderedAccess(ITextureView _target, Vector4 _value);
  public abstract void ClearUnorderedAccess(IBufferView _target, uint _value);

  // === Resource State Transitions ===
  public abstract void TransitionResource(IResource _resource, ResourceState _newState);
  public abstract void TransitionResources(IResource[] _resources, ResourceState[] _newStates);

  // === UAV Barriers ===
  public abstract void UAVBarrier(IResource _resource);
  public abstract void UAVBarriers(IResource[] _resources);

  // === Vertex/Index Buffers ===
  public abstract void SetVertexBuffer(IBufferView _buffer, uint _slot = 0);
  public abstract void SetVertexBuffers(IBufferView[] _buffers, uint _startSlot = 0);
  public abstract void SetIndexBuffer(IBufferView _buffer, IndexFormat _format);

  // === Shaders ===
  public abstract void SetVertexShader(IShader _shader);
  public abstract void SetPixelShader(IShader _shader);
  public abstract void SetComputeShader(IShader _shader);
  public abstract void SetGeometryShader(IShader _shader);
  public abstract void SetHullShader(IShader _shader);
  public abstract void SetDomainShader(IShader _shader);

  // === Shader Resources ===
  public abstract void SetShaderResource(ShaderStage _stage, uint _slot, ITextureView _resource);
  public abstract void SetShaderResources(ShaderStage _stage, uint _startSlot, ITextureView[] _resources);

  // === Unordered Access ===
  public abstract void SetUnorderedAccess(ShaderStage _stage, uint _slot, ITextureView _resource);
  public abstract void SetUnorderedAccesses(ShaderStage _stage, uint _startSlot, ITextureView[] _resources);

  // === Constant Buffers ===
  public abstract void SetConstantBuffer(ShaderStage _stage, uint _slot, IBufferView _buffer);
  public abstract void SetConstantBuffers(ShaderStage _stage, uint _startSlot, IBufferView[] _buffers);

  // === Samplers ===
  public abstract void SetSampler(ShaderStage _stage, uint _slot, ISampler _sampler);
  public abstract void SetSamplers(ShaderStage _stage, uint _startSlot, ISampler[] _samplers);

  // === Render States ===
  public abstract void SetRenderState(IRenderState _renderState);
  public abstract void SetPrimitiveTopology(PrimitiveTopology _topology);
  public abstract void SetBlendState(IBlendState _blendState, Vector4 _blendFactor, uint _sampleMask = 0xffffffff);
  public abstract void SetDepthStencilState(IDepthStencilState _depthStencilState, uint _stencilRef = 0);
  public abstract void SetRasterizerState(IRasterizerState _rasterizerState);

  // === Draw Commands ===
  public abstract void Draw(uint _vertexCount, uint _instanceCount = 1, uint _startVertex = 0, uint _startInstance = 0);
  public abstract void DrawIndexed(uint _indexCount, uint _instanceCount = 1, uint _startIndex = 0, int _baseVertex = 0, uint _startInstance = 0);
  public abstract void DrawIndirect(IBufferView _argsBuffer, ulong _offset = 0);
  public abstract void DrawIndexedIndirect(IBufferView _argsBuffer, ulong _offset = 0);

  // === Compute Commands ===
  public abstract void Dispatch(uint _groupCountX, uint _groupCountY = 1, uint _groupCountZ = 1);
  public abstract void DispatchIndirect(IBufferView _argsBuffer, ulong _offset = 0);

  // === Copy Operations ===
  public abstract void CopyTexture(ITexture _src, ITexture _dst);
  public abstract void CopyTextureRegion(ITexture _src, uint _srcMip, uint _srcArray, Box _srcBox, ITexture _dst, uint _dstMip, uint _dstArray, Point3D _dstOffset);
  public abstract void CopyBuffer(IBuffer _src, IBuffer _dst);
  public abstract void CopyBufferRegion(IBuffer _src, ulong _srcOffset, IBuffer _dst, ulong _dstOffset, ulong _size);
  public abstract void ResolveTexture(ITexture _src, uint _srcArray, ITexture _dst, uint _dstArray, TextureFormat _format);

  // === Queries ===
  public abstract void BeginQuery(IQuery _query);
  public abstract void EndQuery(IQuery _query);

  // === Debug ===
  public abstract void PushDebugGroup(string _name);
  public abstract void PopDebugGroup();
  public abstract void InsertDebugMarker(string _name);

  // === Методы-удобности (реализованы в базовом классе) ===

  /// <summary>
  /// Рисует fullscreen quad (3 вершины для треугольника, покрывающего весь экран)
  /// </summary>
  public void DrawFullscreenQuad()
  {
    SetPrimitiveTopology(PrimitiveTopology.TriangleList);
    Draw(3, 1, 0, 0);
  }

  /// <summary>
  /// Устанавливает render target из текстур
  /// </summary>
  public void SetRenderTarget(ITexture _colorTarget, ITexture _depthTarget = null)
  {
    var colorView = _colorTarget?.GetDefaultRenderTargetView();
    var depthView = _depthTarget?.GetDefaultDepthStencilView();
    SetRenderTarget(colorView, depthView);
  }

  /// <summary>
  /// Устанавливает shader resource из текстуры
  /// </summary>
  public void SetShaderResource(ShaderStage _stage, uint _slot, ITexture _texture)
  {
    var view = _texture?.GetDefaultShaderResourceView();
    SetShaderResource(_stage, _slot, view);
  }

  /// <summary>
  /// Устанавливает unordered access из текстуры
  /// </summary>
  public void SetUnorderedAccess(ShaderStage _stage, uint _slot, ITexture _texture)
  {
    var view = _texture?.GetDefaultUnorderedAccessView();
    SetUnorderedAccess(_stage, _slot, view);
  }

  /// <summary>
  /// Устанавливает constant buffer из буфера
  /// </summary>
  public void SetConstantBuffer(ShaderStage _stage, uint _slot, IBuffer _buffer)
  {
    var view = _buffer?.GetDefaultShaderResourceView();
    SetConstantBuffer(_stage, _slot, view);
  }

  /// <summary>
  /// Очищает render target стандартным черным цветом
  /// </summary>
  public void ClearRenderTarget(ITextureView _target)
  {
    ClearRenderTarget(_target, Vector4.Zero);
  }

  /// <summary>
  /// Очищает depth buffer стандартными значениями (1.0f depth, 0 stencil)
  /// </summary>
  public void ClearDepthStencil(ITextureView _target)
  {
    ClearDepthStencil(_target, ClearFlags.Depth | ClearFlags.Stencil, 1.0f, 0);
  }

  /// <summary>
  /// Очищает только depth buffer
  /// </summary>
  public void ClearDepth(ITextureView _target, float _depth = 1.0f)
  {
    ClearDepthStencil(_target, ClearFlags.Depth, _depth, 0);
  }

  /// <summary>
  /// Очищает только stencil buffer
  /// </summary>
  public void ClearStencil(ITextureView _target, byte _stencil = 0)
  {
    ClearDepthStencil(_target, ClearFlags.Stencil, 1.0f, _stencil);
  }

  /// <summary>
  /// Устанавливает viewport на всю текстуру
  /// </summary>
  public void SetViewportFullTexture(ITexture _texture)
  {
    var viewport = new Viewport
    {
      X = 0,
      Y = 0,
      Width = _texture.Description.Width,
      Height = _texture.Description.Height,
      MinDepth = 0.0f,
      MaxDepth = 1.0f
    };
    SetViewport(viewport);
  }

  /// <summary>
  /// Устанавливает scissor rect на всю текстуру
  /// </summary>
  public void SetScissorRectFullTexture(ITexture _texture)
  {
    var rect = new Rectangle
    {
      X = 0,
      Y = 0,
      Width = (int)_texture.Description.Width,
      Height = (int)_texture.Description.Height
    };
    SetScissorRect(rect);
  }

  /// <summary>
  /// Выполняет переход ресурса и UAV barrier за одну операцию
  /// </summary>
  public void TransitionResourceWithBarrier(IResource _resource, ResourceState _newState)
  {
    TransitionResource(_resource, _newState);
    if(_newState == ResourceState.UnorderedAccess)
    {
      UAVBarrier(_resource);
    }
  }

  /// <summary>
  /// Начинает новый debug scope (с автоматическим закрытием через using)
  /// </summary>
  public DebugScope BeginDebugScope(string _name)
  {
    return new DebugScope(this, _name);
  }

  /// <summary>
  /// Копирует всю текстуру (все mip levels и array slices)
  /// </summary>
  public void CopyFullTexture(ITexture _src, ITexture _dst)
  {
    if(_src.Description.MipLevels != _dst.Description.MipLevels ||
        _src.Description.ArraySize != _dst.Description.ArraySize)
    {
      throw new ArgumentException("Source and destination textures must have the same mip levels and array size");
    }

    for(uint arraySlice = 0; arraySlice < _src.Description.ArraySize; arraySlice++)
    {
      for(uint mipLevel = 0; mipLevel < _src.Description.MipLevels; mipLevel++)
      {
        var width = Math.Max(1u, _src.Description.Width >> (int)mipLevel);
        var height = Math.Max(1u, _src.Description.Height >> (int)mipLevel);
        var depth = Math.Max(1u, _src.Description.Depth >> (int)mipLevel);

        var srcBox = new Box
        {
          Left = 0,
          Top = 0,
          Front = 0,
          Right = width,
          Bottom = height,
          Back = depth
        };

        CopyTextureRegion(_src, mipLevel, arraySlice, srcBox, _dst, mipLevel, arraySlice, new Point3D(0, 0, 0));
      }
    }
  }

  /// <summary>
  /// Выполняет полную очистку multiple render targets
  /// </summary>
  public void ClearMultipleRenderTargets(ITextureView[] _colorTargets, Vector4[] _clearColors, ITextureView _depthTarget = null, float _depth = 1.0f, byte _stencil = 0)
  {
    if(_colorTargets.Length != _clearColors.Length)
      throw new ArgumentException("Color targets and clear colors arrays must have the same length");

    for(int i = 0; i < _colorTargets.Length; i++)
    {
      if(_colorTargets[i] != null)
      {
        ClearRenderTarget(_colorTargets[i], _clearColors[i]);
      }
    }

    if(_depthTarget != null)
    {
      ClearDepthStencil(_depthTarget, ClearFlags.Depth | ClearFlags.Stencil, _depth, _stencil);
    }
  }

  /// <summary>
  /// Устанавливает все шейдеры graphics pipeline
  /// </summary>
  public void SetGraphicsShaders(IShader _vertexShader = null, IShader _pixelShader = null,
                                IShader _geometryShader = null, IShader _hullShader = null, IShader _domainShader = null)
  {
    if(_vertexShader != null)
      SetVertexShader(_vertexShader);
    if(_pixelShader != null)
      SetPixelShader(_pixelShader);
    if(_geometryShader != null)
      SetGeometryShader(_geometryShader);
    if(_hullShader != null)
      SetHullShader(_hullShader);
    if(_domainShader != null)
      SetDomainShader(_domainShader);
  }

  /// <summary>
  /// Batch операция для установки нескольких shader resources
  /// </summary>
  public void SetShaderResourceBatch(ShaderStage _stage, uint _startSlot, params ITextureView[] _resources)
  {
    SetShaderResources(_stage, _startSlot, _resources);
  }

  /// <summary>
  /// Batch операция для установки нескольких constant buffers
  /// </summary>
  public void SetConstantBufferBatch(ShaderStage _stage, uint _startSlot, params IBufferView[] _buffers)
  {
    SetConstantBuffers(_stage, _startSlot, _buffers);
  }

  /// <summary>
  /// Batch операция для установки нескольких samplers
  /// </summary>
  public void SetSamplerBatch(ShaderStage _stage, uint _startSlot, params ISampler[] _samplers)
  {
    SetSamplers(_stage, _startSlot, _samplers);
  }

  /// <summary>
  /// Выполняет instanced draw с автоматическим расчетом количества instances
  /// </summary>
  public void DrawInstanced(uint _vertexCount, uint _totalInstanceCount, uint _instancesPerBatch = 1000)
  {
    uint remainingInstances = _totalInstanceCount;
    uint startInstance = 0;

    while(remainingInstances > 0)
    {
      uint currentBatchSize = Math.Min(remainingInstances, _instancesPerBatch);
      Draw(_vertexCount, currentBatchSize, 0, startInstance);

      remainingInstances -= currentBatchSize;
      startInstance += currentBatchSize;
    }
  }

  /// <summary>
  /// Выполняет indexed instanced draw с автоматическим расчетом количества instances
  /// </summary>
  public void DrawIndexedInstanced(uint _indexCount, uint _totalInstanceCount, uint _instancesPerBatch = 1000, int _baseVertex = 0)
  {
    uint remainingInstances = _totalInstanceCount;
    uint startInstance = 0;

    while(remainingInstances > 0)
    {
      uint currentBatchSize = Math.Min(remainingInstances, _instancesPerBatch);
      DrawIndexed(_indexCount, currentBatchSize, 0, _baseVertex, startInstance);

      remainingInstances -= currentBatchSize;
      startInstance += currentBatchSize;
    }
  }

  public abstract void Dispose();

  /// <summary>
  /// RAII helper для debug scopes
  /// </summary>
  public sealed class DebugScope: IDisposable
  {
    private readonly CommandBuffer p_commandBuffer;
    private bool p_disposed;

    internal DebugScope(CommandBuffer _commandBuffer, string _name)
    {
      p_commandBuffer = _commandBuffer ?? throw new ArgumentNullException(nameof(_commandBuffer));
      p_commandBuffer.PushDebugGroup(_name);
    }

    public void Dispose()
    {
      if(!p_disposed)
      {
        p_commandBuffer.PopDebugGroup();
        p_disposed = true;
      }
    }
  }
}
