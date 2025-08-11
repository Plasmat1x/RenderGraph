using GraphicsAPI.Descriptions;

namespace Directx12Impl.Parts.Managers;

/// <summary>
/// Менеджер для горячей перезагрузки шейдеров
/// </summary>
public class DX12ShaderHotReloadManager: IDisposable
{
  private Dictionary<string, ShaderDescription> p_registeredShaders = new();
  private Dictionary<string, DX12Shader> p_loadedShaders = new();
  private FileSystemWatcher p_watcher;
  private bool p_isWatching;

  public event Action<string, DX12Shader> ShaderReloaded;

  public void RegisterShader(string _filePath, ShaderDescription _description)
  {
    p_registeredShaders[_filePath] = _description;

    var desc = _description;
    desc.FilePath = _filePath;
    p_loadedShaders[_filePath] = new DX12Shader(desc);
  }

  public void StartWatching()
  {
    if(p_isWatching)
      return;

    var directory = Path.GetDirectoryName(p_registeredShaders.Keys.First());
    p_watcher = new FileSystemWatcher(directory)
    {
      Filter = "*.hlsl",
      NotifyFilter = NotifyFilters.LastWrite
    };

    p_watcher.Changed += OnFileChanged;
    p_watcher.EnableRaisingEvents = true;
    p_isWatching = true;
  }

  private void OnFileChanged(object _sender, FileSystemEventArgs _e)
  {
    if(p_registeredShaders.ContainsKey(_e.FullPath))
    {
      Task.Delay(100).ContinueWith(_ => ReloadShader(_e.FullPath));
    }
  }

  private void ReloadShader(string _filePath)
  {
    try
    {
      if(p_loadedShaders.TryGetValue(_filePath, out var oldShader))
      {
        oldShader.Dispose();
      }

      var desc = p_registeredShaders[_filePath];
      desc.SourceCode = File.ReadAllText(_filePath);
      var newShader = new DX12Shader(desc);

      p_loadedShaders[_filePath] = newShader;
      ShaderReloaded?.Invoke(desc.Name, newShader);
    }
    catch(Exception ex)
    {
      Console.WriteLine($"Failed to reload shader '{_filePath}': {ex.Message}");
    }
  }

  public void Dispose()
  {
    p_watcher?.Dispose();
    foreach(var shader in p_loadedShaders.Values)
    {
      shader.Dispose();
    }
  }
}
