using Core.Enums;

namespace Core;
public abstract class RenderPass: IDisposable
{
  private readonly List<ResourceHandle> p_inputs = new();
  private readonly List<ResourceHandle> p_outputs = new();
  private readonly List<RenderPass> p_dependencies = new();
  private bool p_enabled = true;
  private int p_executionOrder = 0;

  protected RenderPass(string _name)
  {
    Name = _name ?? string.Empty;
  }

  public string Name { get; protected set; }
  public bool Enabled
  {
    get => p_enabled;
    set => p_enabled = value;
  }

  public IReadOnlyList<ResourceHandle> Inputs => p_inputs.AsReadOnly();
  public IReadOnlyList<ResourceHandle> Outputs => p_outputs.AsReadOnly();
  public IReadOnlyList<RenderPass> Dependencies => p_dependencies.AsReadOnly();

  public int ExecutionOrder
  {
    get => p_executionOrder;
    set => p_executionOrder = value;
  }

  public bool AlwaysExecute { get; set; } = false;
  public PassCategory Category { get; set; } = PassCategory.Rendering;
  public PassPriority Priority { get; set; } = PassPriority.Normal;

  public PassStatistics Statistics { get; private set; } = new();

  // События
  public event Action<RenderPass> OnPassSetup;
  public event Action<RenderPass> OnPassExecuted;
  public event Action<RenderPass, Exception> OnPassError;

  public abstract void Setup(RenderGraphBuilder _builder);
  public abstract void Execute(RenderPassContext _context);

  /// <summary>
  /// Проверяет может ли проход быть выполнен в текущих условиях
  /// </summary>
  public virtual bool CanExecute()
  {
    if(!Enabled)
      return false;

    if(p_dependencies.Count == 0)
      return true;

    foreach(var dependency in p_dependencies)
    {
      if(dependency.Enabled && !dependency.Statistics.WasExecutedThisFrame)
        return false;
    }

    return true;
  }

  /// <summary>
  /// Выполняется перед Setup для инициализации прохода
  /// </summary>
  public virtual void Initialize() => Statistics.Reset();

  /// <summary>
  /// Выполняется после компиляции render graph для финальной настройки
  /// </summary>
  public virtual void OnGraphCompiled(RenderGraph _renderGraph){}

  /// <summary>
  /// Выполняется в начале каждого кадра
  /// </summary>
  public virtual void OnFrameBegin(FrameData _frameData) => Statistics.StartFrame();

  /// <summary>
  /// Выполняется в конце каждого кадра
  /// </summary>
  public virtual void OnFrameEnd(FrameData _frameData) => Statistics.EndFrame();

  /// <summary>
  /// Возвращает информацию об использовании ресурсов этим проходом
  /// </summary>
  public virtual ResourceUsageInfo GetResourceUsage()
  {
    return new ResourceUsageInfo
    {
      PassName = Name,
    };
  }

  /// <summary>
  /// Проверяет совместимость с другим проходом
  /// </summary>
  public virtual bool IsCompatibleWith(RenderPass _otherPass)
  {
    if(_otherPass == null)
      return false;

    var myOutputs = new HashSet<ResourceHandle>(Outputs);
    var otherOutputs = new HashSet<ResourceHandle>(_otherPass.Outputs);

    return !myOutputs.Overlaps(otherOutputs);
  }

  /// <summary>
  /// Валидирует настройки прохода
  /// </summary>
  public virtual bool Validate(out string _errorMessage)
  {
    _errorMessage = string.Empty;

    if(string.IsNullOrEmpty(Name))
    {
      _errorMessage = "Pass name cannot be empty";
      return false;
    }

    if(HasCircularDependency())
    {
      _errorMessage = $"Circular dependency detected in pass '{Name}'";
      return false;
    }

    return true;
  }

  public void AddDependency(RenderPass _dependency)
  {
    if(_dependency == null)
      throw new ArgumentNullException(nameof(_dependency));

    if(_dependency == this)
      throw new InvalidOperationException("Pass cannot depend on itself");

    if(!p_dependencies.Contains(_dependency))
    {
      p_dependencies.Add(_dependency);
    }
  }

  public void RemoveDependency(RenderPass _dependency)
  {
    if(_dependency != null)
    {
      p_dependencies.Remove(_dependency);
    }
  }

  public void ClearDependencies() => p_dependencies.Clear();

  public bool HasDependency(RenderPass _dependency)
  {
    return _dependency != null && p_dependencies.Contains(_dependency);
  }

  public bool HasDependencyOn(RenderPass _targetPass)
  {
    if(_targetPass == null)
      return false;

    if(p_dependencies.Contains(_targetPass))
      return true;

    foreach(var dependency in p_dependencies)
    {
      if(dependency.HasDependencyOn(_targetPass))
        return true;
    }

    return false;
  }

  public override string ToString() => $"RenderPass(Name: '{Name}', Enabled: {Enabled}, Inputs: {Inputs.Count}, Outputs: {Outputs.Count}, Dependencies: {Dependencies.Count})";

  public override bool Equals(object? _obj) => _obj is RenderPass other && Name == other.Name;

  public override int GetHashCode() => Name?.GetHashCode() ?? 0;

  public virtual void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if(disposing)
    {
      // Освобождаем управляемые ресурсы
    }
  }

  private bool HasCircularDependency()
  {
    var visited = new HashSet<RenderPass>();
    var visiting = new HashSet<RenderPass>();

    return HasCircularDependencyDFS(this, visited, visiting);
  }

  private static bool HasCircularDependencyDFS(RenderPass _pass, HashSet<RenderPass> _visited, HashSet<RenderPass> _visiting)
  {
    if(_visiting.Contains(_pass))
      return true;

    if(_visited.Contains(_pass))
      return false;

    _visiting.Add(_pass);

    foreach(var dependency in _pass.p_dependencies)
    {
      if(HasCircularDependencyDFS(dependency, _visited, _visiting))
        return true;
    }

    _visiting.Remove(_pass);
    _visited.Add(_pass);
    return false;
  }

  internal void AddInput(ResourceHandle _input)
  {
    if(_input.IsValid() && !p_inputs.Contains(_input))
    {
      p_inputs.Add(_input);
    }
  }

  internal void AddOutput(ResourceHandle _output)
  {
    if(_output.IsValid() && !p_outputs.Contains(_output))
    {
      p_outputs.Add(_output);
    }
  }

  internal void ClearInputsOutputs()
  {
    p_inputs.Clear();
    p_outputs.Clear();
  }

  internal void SetExecutionOrder(int _order) => p_executionOrder = _order;

  internal void InternalSetup(RenderGraphBuilder _builder)
  {
    try
    {
      Statistics.StartSetup();
      ClearInputsOutputs();

      Setup(_builder);

      Statistics.EndSetup();
      OnPassSetup?.Invoke(this);
    }
    catch(Exception ex)
    {
      Statistics.RecordError(ex);
      OnPassError?.Invoke(this, ex);
      throw;
    }
  }

  internal void InternalExecute(RenderPassContext _context)
  {
    try
    {
      Statistics.StartExecution();

      Execute(_context);

      Statistics.EndExecution();
      Statistics.MarkExecutedThisFrame();
      OnPassExecuted?.Invoke(this);
    }
    catch(Exception ex)
    {
      Statistics.RecordError(ex);
      OnPassError?.Invoke(this, ex);
      throw;
    }
  }
}