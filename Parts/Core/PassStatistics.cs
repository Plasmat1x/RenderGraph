namespace Core;

public class PassStatistics
{
  private DateTime p_setupStartTime;
  private DateTime p_executionStartTime;
  private readonly List<Exception> p_errors = new();

  public TimeSpan LastSetupTime { get; private set; }
  public TimeSpan LastExecutionTime { get; private set; }
  public TimeSpan TotalSetupTime { get; private set; }
  public TimeSpan TotalExecutionTime { get; private set; }

  public int SetupCount { get; private set; }
  public int ExecutionCount { get; private set; }
  public int ErrorCount => p_errors.Count;

  public bool WasExecutedThisFrame { get; private set; }
  public int CurrentFrameNumber { get; private set; }

  public TimeSpan AverageSetupTime => SetupCount > 0 ? TimeSpan.FromTicks(TotalSetupTime.Ticks / SetupCount) : TimeSpan.Zero;
  public TimeSpan AverageExecutionTime => ExecutionCount > 0 ? TimeSpan.FromTicks(TotalExecutionTime.Ticks / ExecutionCount) : TimeSpan.Zero;

  public void StartSetup()
  {
    p_setupStartTime = DateTime.UtcNow;
  }

  public void EndSetup()
  {
    var elapsed = DateTime.UtcNow - p_setupStartTime;
    LastSetupTime = elapsed;
    TotalSetupTime += elapsed;
    SetupCount++;
  }

  public void StartExecution()
  {
    p_executionStartTime = DateTime.UtcNow;
  }

  public void EndExecution()
  {
    var elapsed = DateTime.UtcNow - p_executionStartTime;
    LastExecutionTime = elapsed;
    TotalExecutionTime += elapsed;
    ExecutionCount++;
  }

  public void MarkExecutedThisFrame()
  {
    WasExecutedThisFrame = true;
  }

  public void StartFrame()
  {
    WasExecutedThisFrame = false;
    CurrentFrameNumber++;
  }

  public void EndFrame()
  {

  }

  public void RecordError(Exception _exception)
  {
    if(_exception != null)
    {
      p_errors.Add(_exception);
    }
  }

  public IReadOnlyList<Exception> GetErrors()
  {
    return p_errors.AsReadOnly();
  }

  public Exception GetLastError()
  {
    return p_errors.LastOrDefault();
  }

  public void Reset()
  {
    LastSetupTime = TimeSpan.Zero;
    LastExecutionTime = TimeSpan.Zero;
    TotalSetupTime = TimeSpan.Zero;
    TotalExecutionTime = TimeSpan.Zero;
    SetupCount = 0;
    ExecutionCount = 0;
    WasExecutedThisFrame = false;
    CurrentFrameNumber = 0;
    p_errors.Clear();
  }

  public void ClearErrors()
  {
    p_errors.Clear();
  }

  public override string ToString()
  {
    return $"PassStats(Setup: {SetupCount}, Execution: {ExecutionCount}, " +
           $"AvgSetup: {AverageSetupTime.TotalMilliseconds:F2}ms, " +
           $"AvgExecution: {AverageExecutionTime.TotalMilliseconds:F2}ms, " +
           $"Errors: {ErrorCount})";
  }
}