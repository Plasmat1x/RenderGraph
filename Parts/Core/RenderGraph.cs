using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Interfaces;

using System.Numerics;

using Utility;

namespace Core;

public class RenderGraph: IDisposable
{
  private readonly List<RenderPass> p_passes = new();
  private readonly ResourceManager p_resourceManager;
  private readonly DependencyResolver p_dependencyResolver;
  private readonly IGraphicsDevice p_device;
  private readonly RenderGraphBuilder p_builder;
  private readonly FrameData p_frameData;

  private bool p_compiled;
  private bool p_disposed;
  private List<RenderPass> p_executionOrder;
  private readonly Dictionary<RenderPass, RenderPassContext> p_passContexts = new();

  public bool IsCompilded => p_compiled;
  public IReadOnlyList<RenderPass> Passes => p_passes.AsReadOnly();
  public IReadOnlyList<RenderPass> ExecutionOrder => p_executionOrder?.AsReadOnly();
  public FrameData FrameData => p_frameData;

  public RenderGraph(IGraphicsDevice _device)
  {
    p_device = _device ?? throw new ArgumentNullException(nameof(_device));
    p_resourceManager = new ResourceManager(p_device);
    p_dependencyResolver = new DependencyResolver();
    p_builder = new RenderGraphBuilder(p_resourceManager);
    p_frameData = new FrameData();
  }

  public void AddPass<T>(T _pass) where T : RenderPass
  {
    if(_pass == null)
      throw new ArgumentNullException(nameof(_pass));
    if(p_passes.Contains(_pass))
      throw new InvalidOperationException($"Pass '{_pass.Name}' already added to the render graph");
    if(string.IsNullOrEmpty(_pass.Name))
      throw new InvalidOperationException($"RenderPass ,ust have a non-empty Name");
    if(p_passes.Any(_p => _p.Name == _pass.Name))
      throw new InvalidOperationException($"Pass with name'{_pass.Name}' already exists");

    p_passes.Add(_pass);
    p_compiled = false;

    var contex = new RenderPassContext
    {
      Resources = p_resourceManager,
      FrameData = p_frameData,
      PassIndex = p_passes.Count - 1,
    };
    p_passContexts[_pass] = contex;
  }

  public void RemovePass(RenderPass _pass)
  {
    if(_pass == null)
      return;

    if(p_passes.Remove(_pass))
    {
      p_passContexts.Remove(_pass);
      p_compiled = false;
    }
  }

  public T GetPass<T>() where T : RenderPass
  {
    return p_passes.OfType<T>().FirstOrDefault();
  }

  public RenderPass GetPass(string _name)
  {
    return p_passes.FirstOrDefault(_p => _p.Name == _name);
  }

  public void Execute(CommandBuffer _commandBuffer)
  {
    if(_commandBuffer == null)
      throw new ArgumentNullException(nameof(_commandBuffer));

    if(!p_compiled)
      throw new InvalidOperationException("Render graph must be compiled before execution");

    if(p_executionOrder.Count == 0)
      return;

    try
    {
      _commandBuffer.Begin();

      p_frameData.FrameIndex++;
      p_frameData.UpdateMatrices();

      foreach(var pass in p_executionOrder)
      {
        pass.OnFrameBegin(p_frameData);
      }

      for(int i = 0; i < p_executionOrder.Count; i++)
      {
        var pass = p_executionOrder[i];

        if(!pass.Enabled) 
        {
          Console.WriteLine($"Skipping disabled pass: {pass.Name}");
          continue;
        }

        if(!pass.CanExecute())
        {
          Console.WriteLine($"Pass {pass.Name} cannot execute (dependencies not met)");
          continue;
        }

        var context = p_passContexts[pass];
        context.CommandBuffer = _commandBuffer;
        context.PassIndex = i;

        TransitionResourcesForPass(pass, _commandBuffer);

        pass.InternalExecute(context);
      }

      foreach(var pass in p_executionOrder)
      {
        pass.OnFrameEnd(p_frameData);
      }

      _commandBuffer.End();
    }
    catch(Exception ex)
    {
      throw new InvalidOperationException($"Failed to execute render grapgh: {ex.Message}", ex);
    }
  }

  public void Compile()
  {
    if(p_passes.Count == 0)
    {
      p_executionOrder = new List<RenderPass>();
      p_compiled = true;
      return;
    }

    try
    {
      SetupPasses();
      BuildDependencyGraph();
      p_executionOrder = p_dependencyResolver.TopologicalSort();
      ValidateGraph();
      OptimizeResources();
      p_compiled = true;
    }
    catch(Exception ex)
    {
      p_compiled = false;
      throw new InvalidOperationException($"Failed to compile render graph: {ex.Message}", ex);
    }
  }

  public void Reset()
  {
    p_passes.Clear();
    p_passContexts.Clear();
    p_executionOrder = null;
    p_compiled = false;
    p_builder.Clear();
    p_dependencyResolver.Clear();
    p_frameData.Reset();
  }

  public bool ValidateGraph()
  {
    try
    {
      var cycles = p_dependencyResolver.DetectCycles();

      if(cycles.Count > 0)
      {
        var cycleNames = string.Join("-->", cycles.Select(_p => _p.Name));
        throw new InvalidOperationException($"Circular dependency detected: {cycleNames}");
      }
      p_builder.ValidateResourceUsages();
      ValidateResourceProducers();

      return true;
    }
    catch
    {
      return false;
    }
  }

  public List<RenderPass> GetExecutionOrder()
  {
    return p_executionOrder?.ToList() ?? new List<RenderPass>();
  }

  public void UpdateFrameData(float _deltaTime, uint _screenWidth, uint _screenHeight)
  {
    p_frameData.DeltaTime = _deltaTime;
    p_frameData.ScreenHeight = _screenHeight;
    p_frameData.ScreenWidth = _screenWidth;

    foreach(var context in p_passContexts.Values)
    {
      context.ViewportWidth = _screenWidth;
      context.ViewportHeight = _screenHeight;
    }
  }

  public void SetViewMatrix(Matrix4x4 _viewMatrix)
  {
    p_frameData.ViewMatrix = _viewMatrix;
  }

  public void SetProjectionMatrix(Matrix4x4 _projectionMatrix)
  {
    p_frameData.ProjectionMatrix = _projectionMatrix;
  }

  public void SetCameraPosition(Vector3 _position)
  {
    p_frameData.CameraPosition = _position;
  }
  public ResourceHandle ImportTexture(string _name, ITexture _texture)
  {
    return p_builder.ImportTexture(_name, _texture);
  }

  public ResourceHandle ImportBuffer(string _name, IBuffer _buffer)
  {
    return p_builder.ImportBuffer(_name, _buffer);
  }

  public ResourceHandle GetNamedResource(string _name)
  {
    return p_builder.GetNamedReource(_name);
  }

  public bool HasNamedResource(string _name)
  {
    return p_builder.HasNamedResource(_name);
  }

  public MemoryUsageInfo GetMemoryUsage()
  {
    return p_resourceManager.GetMemoryUsage();
  }

  public RenderGraphStatistics GetStatistics()
  {
    var enabledPasses = p_passes.Count(_p => _p.Enabled);
    var totalResources = p_builder.GetResourceUsages().Count();
    var memoryUsage = p_resourceManager.GetMemoryUsage();

    return new RenderGraphStatistics
    {
      TotalPasses = p_passes.Count,
      EnabledPasses = enabledPasses,
      DisabledPasses = totalResources,
      TotalResources = totalResources,
      MemoryUsage = memoryUsage,
      IsCompiled = p_compiled,
      LastFrameIndex = p_frameData.FrameIndex
    };
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    Reset();
    p_resourceManager?.Dispose();
    p_disposed = true;
  }

  private void SetupPasses()
  {
    p_builder.Clear();

    foreach(var pass in p_passes)
    {
      pass.ClearInputsOutputs();

      p_builder.SetCurrentPass(pass);

      try
      {
        pass.InternalSetup(p_builder);
      }
      catch(Exception ex)
      {
        throw new InvalidOperationException($"Failed to setup pass '{pass.Name}': {ex.Message}", ex);
      }
      finally
      {
        p_builder.FinishCurrentPass();
      }
    }
  }

  private void BuildDependencyGraph()
  {
    p_dependencyResolver.Clear();

    foreach(var pass in p_passes)
    {
      p_dependencyResolver.AddNode(pass);
    }

    var resourceProducers = new Dictionary<ResourceHandle, RenderPass>();
    var resourceConsumers = new Dictionary<ResourceHandle, List<RenderPass>>();

    foreach(var pass in p_passes)
    {
      foreach(var output in pass.Outputs)
      {
        if(resourceProducers.ContainsKey(output))
        {
          throw new InvalidOperationException($"Resource {output} is produced by multiple passes: '{resourceProducers[output].Name}' and '{pass.Name}'");
        }
        resourceProducers[output] = pass;
      }

      foreach(var input in pass.Inputs)
      {
        if(!resourceConsumers.ContainsKey(input))
          resourceConsumers[input] = new List<RenderPass>();

        resourceConsumers[input].Add(pass);
      }
    }

    foreach(var kvp in resourceConsumers)
    {
      var resource = kvp.Key;
      var consumers = kvp.Value;

      if(resourceProducers.TryGetValue(resource, out var producer))
      {
        foreach(var consumer in consumers)
        {
          if(producer != consumer)
          {
            p_dependencyResolver.AddEdge(producer, consumer);
            consumer.AddDependency(producer);
          }
        }
      }
    }

    foreach(var pass in p_passes)
    {
      foreach(var dependency in pass.Dependencies)
        p_dependencyResolver.AddEdge(dependency, pass);
    }
  }

  private void ValidateResourceProducers()
  {
    var resourceProducers = new HashSet<ResourceHandle>();
    var requiredResources = new HashSet<ResourceHandle>();

    foreach(var pass in p_passes)
    {
      foreach(var output in pass.Outputs)
        resourceProducers.Add(output);
    }
    foreach(var pass in p_passes)
    {
      foreach(var input in pass.Inputs)
        requiredResources.Add(input);
    }

    var missingResource = new List<ResourceHandle>();

    foreach(var required in requiredResources)
    {
      if(!resourceProducers.Contains(required))
      {
        var lifetime = p_resourceManager.GetResourceLifetime(required);
        if(lifetime != ResourceLifetime.Imported && lifetime != ResourceLifetime.External)
          missingResource.Add(required);
      }
    }

    if(missingResource.Count > 0)
    {
      var missingNames = string.Join(", ", missingResource.Select(_r => _r.Name));
      throw new InvalidCastException($"Resources are consumed but not produced: {missingNames}");
    }
  }

  private void OptimizeResources()
  {
    var unusedPasses = p_dependencyResolver.CullUnusedPasses();

    foreach(var unusedPass in unusedPasses)
    {
      unusedPass.Enabled = false;
    }

    p_resourceManager.OptimizeResourceUsage();
  }

  private void TransitionResourcesForPass(RenderPass _pass, CommandBuffer _commandBuffer)
  {
    foreach(var input in _pass.Inputs)
    {
      var usage = p_builder.GetResourceUsage(input, _pass.Name);
      if(usage != null)
      {
        _commandBuffer.TransitionResource(p_resourceManager.GetTexture(input), usage.State);
      }
    }

    foreach(var output in _pass.Outputs)
    {
      var usage = p_builder.GetResourceUsage(output, _pass.Name);
      if(usage != null)
      {
        _commandBuffer.TransitionResource(p_resourceManager.GetTexture(output), usage.State);
      }
    }
  }
}
