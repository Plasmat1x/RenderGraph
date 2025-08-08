using GraphicsAPI.Descriptions;

namespace Directx12Impl;

/// <summary>
/// Кэш скомпилированных шейдеров
/// </summary>
public partial class ShaderCache
{
  private readonly string p_cacheDirectory;
  private readonly Dictionary<string, CachedShaderInfo> p_cache = new();

  public ShaderCache(string _cacheDir = "ShaderCache")
  {
    p_cacheDirectory = _cacheDir;
    Directory.CreateDirectory(p_cacheDirectory);
    LoadCacheManifest();
  }

  public bool TryGetCachedShader(ShaderDescription _description, out byte[] _bytecode)
  {
    var hash = ComputeShaderHash(_description);
    var cachePath = Path.Combine(p_cacheDirectory, $"{hash}.cso");

    if(File.Exists(cachePath))
    {
      if(IsSourceNewer(_description, cachePath))
      {
        _bytecode = null;
        return false;
      }

      _bytecode = File.ReadAllBytes(cachePath);
      return true;
    }

    _bytecode = null;
    return false;
  }

  public void CacheShader(ShaderDescription _description, byte[] _bytecode)
  {
    var hash = ComputeShaderHash(_description);
    var cachePath = Path.Combine(p_cacheDirectory, $"{hash}.cso");

    File.WriteAllBytes(cachePath, _bytecode);

    p_cache[hash] = new CachedShaderInfo
    {
      Hash = hash,
      Name = _description.Name,
      Stage = _description.Stage,
      ShaderModel = _description.ShaderModel,
      CachedTime = DateTime.Now
    };

    SaveCacheManifest();
  }

  private string ComputeShaderHash(ShaderDescription _description)
  {
    using var sha256 = System.Security.Cryptography.SHA256.Create();
    var input = $"{_description.Name}_{_description.Stage}_{_description.EntryPoint}_{_description.ShaderModel}";

    if(_description.SourceCode != null)
      input += _description.SourceCode;

    if(_description.Defines != null)
    {
      foreach(var define in _description.Defines)
      {
        input += $"{define.Name}={define.Definition}";
      }
    }

    var bytes = System.Text.Encoding.UTF8.GetBytes(input);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-");
  }

  private bool IsSourceNewer(ShaderDescription description, string cachePath)
  {
    if(!string.IsNullOrEmpty(description.FilePath) && File.Exists(description.FilePath))
    {
      var sourceTime = File.GetLastWriteTime(description.FilePath);
      var cacheTime = File.GetLastWriteTime(cachePath);
      return sourceTime > cacheTime;
    }
    return false;
  }

  private void LoadCacheManifest()
  {
    var manifestPath = Path.Combine(p_cacheDirectory, "manifest.json");
    if(File.Exists(manifestPath))
    {
      var json = File.ReadAllText(manifestPath);
    }
  }

  private void SaveCacheManifest()
  {
    var manifestPath = Path.Combine(p_cacheDirectory, "manifest.json");
  }
}