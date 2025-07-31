using GraphicsAPI.Enums;
using GraphicsAPI.Reflections;

namespace GraphicsAPI.Descriptions;

public class ShaderDescription: ICloneable
{
  public string Name { get; set; } = string.Empty;
  public ShaderStage Stage { get; set; }
  public string EntryPoint { get; set; } = "main";
  public string ShaderModel { get; set; } = "5_1";
  public byte[] ByteCode { get; set; }
  public string FilePath { get; set; }
  public string SourceCode { get; set; }

  public List<ShaderMacro> Defines { get; set; } = [];
  public List<string> IncludePaths { get; set; } = [];

  public ShaderCompileFlags CompileFlags { get; set; } = ShaderCompileFlags.None;
  public ShaderReflection CachedReflection { get; set; }

  public Dictionary<string, object> Metadata { get; set; } = [];

  public bool Validate(out string _errorMessage)
  {
    _errorMessage = null;

    if(string.IsNullOrWhiteSpace(Name))
    {
      _errorMessage = "Shader name cannot be empty";
      return false;
    }

    if(Stage == ShaderStage.Unknown)
    {
      _errorMessage = "Shader stage must be specified";
      return false;
    }

    bool hasSource = (ByteCode != null && ByteCode.Length > 0) ||
                    !string.IsNullOrWhiteSpace(FilePath) ||
                    !string.IsNullOrWhiteSpace(SourceCode);

    if(!hasSource)
    {
      _errorMessage = "Shader must have ByteCode, FilePath, or SourceCode";
      return false;
    }

    if(string.IsNullOrWhiteSpace(EntryPoint))
    {
      _errorMessage = "Entry point cannot be empty";
      return false;
    }

    if(!IsValidShaderModel(ShaderModel))
    {
      _errorMessage = $"Invalid shader model: {ShaderModel}";
      return false;
    }

    return true;
  }
  public object Clone()
  {
    return new ShaderDescription
    {
      Name = Name,
      Stage = Stage,
      EntryPoint = EntryPoint,
      ShaderModel = ShaderModel,
      ByteCode = ByteCode?.ToArray(),
      FilePath = FilePath,
      SourceCode = SourceCode,
      Defines = Defines?.ToList() ?? new List<ShaderMacro>(),
      IncludePaths = IncludePaths?.ToList() ?? new List<string>(),
      CompileFlags = CompileFlags,
      CachedReflection = CachedReflection,
      Metadata = new Dictionary<string, object>(Metadata ?? new Dictionary<string, object>())
    };
  }

  public string GetRecommendedFileSuffix()
  {
    return Stage switch
    {
      ShaderStage.Vertex => "_vs",
      ShaderStage.Pixel => "_ps",
      ShaderStage.Geometry => "_gs",
      ShaderStage.Hull => "_hs",
      ShaderStage.Domain => "_ds",
      ShaderStage.Compute => "_cs",
      ShaderStage.Amplification => "_as",
      ShaderStage.Mesh => "_ms",
      _ => ""
    };
  }

  public string GetTargetProfile()
  {
    var stagePrefix = Stage switch
    {
      ShaderStage.Vertex => "vs",
      ShaderStage.Pixel => "ps",
      ShaderStage.Geometry => "gs",
      ShaderStage.Hull => "hs",
      ShaderStage.Domain => "ds",
      ShaderStage.Compute => "cs",
      ShaderStage.Amplification => "as",
      ShaderStage.Mesh => "ms",
      _ => throw new InvalidOperationException($"Unknown shader stage: {Stage}")
    };

    return $"{stagePrefix}_{ShaderModel.Replace(".", "_")}";
  }

  private static bool IsValidShaderModel(string _model)
  {
    if(string.IsNullOrWhiteSpace(_model))
      return false;

    var validModels = new[]
    {
            "4_0", "4_1",
            "5_0", "5_1",
            "6_0", "6_1", "6_2", "6_3", "6_4", "6_5", "6_6", "6_7"
        };

    return validModels.Contains(_model.Replace("_", "."));
  }
}

public enum ShaderCompileFlags
{
  None = 0,

  // Оптимизация
  SkipOptimization = 1 << 0,
  OptimizationLevel0 = 1 << 1,
  OptimizationLevel1 = 1 << 2,
  OptimizationLevel2 = 1 << 3,
  OptimizationLevel3 = 1 << 4,

  // Отладка
  Debug = 1 << 5,
  SkipValidation = 1 << 6,

  // Поведение
  EnableStrictness = 1 << 7,
  EnableBackwardsCompatibility = 1 << 8,
  IEEEStrictness = 1 << 9,

  // Матрицы
  PackMatrixRowMajor = 1 << 10,
  PackMatrixColumnMajor = 1 << 11,

  // Предупреждения
  WarningsAreErrors = 1 << 12,

  // Ресурсы
  AllResourcesBound = 1 << 13,

  // Частичная точность
  PreferFlowControl = 1 << 14,
  AvoidFlowControl = 1 << 15,

  // Производительность
  EnableMinPrecision = 1 << 16
}
