namespace Directx12Impl.Parts.Utils;

/// <summary>
/// Результат валидации SwapChain
/// </summary>
public class ValidationResult
{
  public List<string> Errors { get; } = new();
  public List<string> Warnings { get; } = new();

  public bool IsValid => Errors.Count == 0;
  public bool HasWarnings => Warnings.Count > 0;

  public void AddError(string _message) => Errors.Add(_message);
  public void AddWarning(string _message) => Warnings.Add(_message);

  public void LogResults(string _prefix = "[SwapChain Validation]")
  {
    if(Errors.Count > 0)
    {
      Console.WriteLine($"{_prefix} Errors:");
      foreach(var error in Errors)
      {
        Console.WriteLine($"{_prefix}   - {error}");
      }
    }

    if(Warnings.Count > 0)
    {
      Console.WriteLine($"{_prefix} Warnings:");
      foreach(var warning in Warnings)
      {
        Console.WriteLine($"{_prefix}   - {warning}");
      }
    }

    if(IsValid && !HasWarnings)
    {
      Console.WriteLine($"{_prefix} All checks passed");
    }
  }
}