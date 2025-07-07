using Core.Enums;

using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace Core;

public class RenderGraphBuilder
{
  private readonly ResourceManager p_resourceManager;
  private readonly Dictionary<ResourceHandle, ResourceUsageInfo> p_resourceUsages = [];
  private readonly Dictionary<string, ResourceHandle> p_namedResources = [];
  private readonly List<ResourceUsageInfo> p_currentPassUsages = [];
  private RenderPass p_currentPass;

  public RenderGraphBuilder(ResourceManager _resourceManager)
  {
    p_resourceManager = _resourceManager ?? throw new ArgumentNullException(nameof(_resourceManager));
  }

  public ResourceHandle CreateTexture(string _name, TextureDescription _desc)
  {
    if(string.IsNullOrEmpty(_name))
      throw new ArgumentException("Resource name cannot be null or empty", nameof(_name));
    if(_desc == null)
      throw new ArgumentNullException(nameof(_desc));
    if(p_currentPass == null)
      throw new InvalidOperationException("CreateTexture can only be call during RenderPass.Setup()");

    _desc.Name = _name;

    var handle = p_resourceManager.CreateTexture(_desc);

    p_namedResources[_name] = handle;

    return handle;
  }

  public ResourceHandle CreateBuffer(string _name, BufferDescription _desc)
  {
    if(string.IsNullOrEmpty(_name))
      throw new ArgumentException("Resource name cannot be null or empty", nameof(_name));
    if(_desc == null)
      throw new ArgumentNullException(nameof(_desc));
    if(p_currentPass == null)
      throw new InvalidOperationException("CreateBuffer can only be call during RenderPass.Setup()");

    _desc.Name = _name;

    var handle = p_resourceManager.CreateBuffer(_desc);

    p_namedResources[_name] = handle;

    return handle;
  }

  public void ReadTexture(ResourceHandle _handle)
  {
    if(p_currentPass == null)
      throw new InvalidOperationException("ReadTexture can only be called during RenderPass.Setup()");

    ValidateTextureHandle(_handle);

    var usage = new ResourceUsageInfo
    {
      Handle = _handle,
      AccessType = ResourceAccessType.Read,
      Usage = ResourceUsage.Default,
      State = ResourceState.ShaderResource,
      PassName = p_currentPass.Name
    };

    p_currentPassUsages.Add(usage);
    p_currentPass.AddInput(_handle);
  }

  public void WriteTexture(ResourceHandle _handle)
  {
    if(p_currentPass == null)
      throw new InvalidOperationException("WriteTexture can only be called during RenderPass.Setup()");

    ValidateTextureHandle(_handle);

    var usage = new ResourceUsageInfo
    {
      Handle = _handle,
      AccessType = ResourceAccessType.Write,
      Usage = ResourceUsage.Default,
      State = ResourceState.RenderTarget,
      PassName = p_currentPass.Name
    };

    p_currentPassUsages.Add(usage);
    p_currentPass.AddOutput(_handle);
  }

  public void ReadBuffer(ResourceHandle _handle)
  {
    if(p_currentPass == null)
      throw new InvalidOperationException("ReadBuffer can only be called during RenderPass.Setup()");

    ValidateBufferHandle(_handle);

    var usage = new ResourceUsageInfo
    {
      Handle = _handle,
      AccessType = ResourceAccessType.Read,
      Usage = ResourceUsage.Default,
      State = ResourceState.ShaderResource,
      PassName = p_currentPass.Name,
    };

    p_currentPassUsages.Add(usage);
    p_currentPass.AddInput(_handle);
  }

  public void WriteBuffer(ResourceHandle _handle)
  {
    if(p_currentPass == null)
      throw new InvalidOperationException("WriteBuffer can only be called during RenderPass.Setup()");

    ValidateBufferHandle(_handle);

    var usage = new ResourceUsageInfo
    {
      Handle = _handle,
      AccessType = ResourceAccessType.Write,
      Usage = ResourceUsage.Default,
      State = ResourceState.UnorderedAccess,
      PassName = p_currentPass.Name,
    };

    p_currentPassUsages.Add(usage);
    p_currentPass.AddOutput(_handle);
  }

  public ResourceHandle ImportTexture(string _name, ITexture _texture)
  {
    if(string.IsNullOrEmpty(_name))
      throw new ArgumentException("Resource name cannot be null or empty", nameof(_name));
    if(_texture == null)
      throw new ArgumentNullException(nameof(_texture));

    var handle = p_resourceManager.ImportTexture(_name, _texture);

    p_namedResources[_name] = handle;

    return handle;
  }

  public ResourceHandle ImportBuffer(string _name, IBuffer _buffer)
  {
    if(string.IsNullOrEmpty(_name))
      throw new ArgumentException("Resource name cannot be null or empty", nameof(_name));
    if(_buffer == null)
      throw new ArgumentNullException(nameof(_buffer));

    var handle = p_resourceManager.ImportBuffer(_name, _buffer);

    p_namedResources[_name] = handle;

    return handle;
  }

  public void SetResourceLifetime(ResourceHandle _handle, ResourceLifetime _lifetime)
  {
    if(!_handle.IsValid())
      throw new ArgumentException("Invalid resource handle", nameof(_handle));

    p_resourceManager.SetResourceLifetime(_handle, _lifetime);
  }

  public ResourceHandle GetNamedReource(string _name)
  {
    if(string.IsNullOrEmpty(_name))
      throw new ArgumentException("Resource name cannot be null or empty", nameof(_name));

    if(p_namedResources.TryGetValue(_name, out var handle))
      return handle;

    throw new KeyNotFoundException($"Named resource '{_name}' not found");
  }

  public bool HasNamedResource(string _name)
  {
    return !string.IsNullOrEmpty(_name) && p_namedResources.ContainsKey(_name);
  }

  public void ReadWriteTexture(ResourceHandle _handle)
  {
    if(p_currentPass == null)
      throw new InvalidOperationException("ReadWritteTexture can only be called during RenderPass.Setup()");

    ValidateTextureHandle(_handle);

    var usage = new ResourceUsageInfo
    {
      Handle = _handle,
      AccessType = ResourceAccessType.ReadWrite,
      Usage = ResourceUsage.Default,
      State = ResourceState.UnorderedAccess,
      PassName = p_currentPass.Name,
    };

    p_currentPassUsages.Add(usage);
    p_currentPass.AddInput(_handle);
    p_currentPass.AddOutput(_handle);
  }

  public void ReadWriteBuffer(ResourceHandle _handle)
  {
    if(p_currentPass == null)
      throw new InvalidOperationException("ReadWritteBuffer can only be called during RenderPass.Setup()");

    ValidateBufferHandle(_handle);

    var usage = new ResourceUsageInfo
    {
      Handle = _handle,
      AccessType = ResourceAccessType.ReadWrite,
      Usage = ResourceUsage.Default,
      State = ResourceState.UnorderedAccess,
      PassName = p_currentPass.Name,
    };

    p_currentPassUsages.Add(usage);
    p_currentPass.AddInput(_handle);
    p_currentPass.AddOutput(_handle);
  }

  public void ReadTextureAsDepth(ResourceHandle _handle)
  {
    if(p_currentPass == null)
      throw new InvalidOperationException("ReadTextureAsDepth can only be called during RenderPass.Setup()");

    ValidateTextureHandle(_handle);

    var usage = new ResourceUsageInfo
    {
      Handle = _handle,
      AccessType = ResourceAccessType.Read,
      Usage = ResourceUsage.Default,
      State = ResourceState.DepthRead,
      PassName = p_currentPass.Name,
    };

    p_currentPassUsages.Add(usage);
    p_currentPass.AddInput(_handle);
  }

  public void WriteTextureAsDepth(ResourceHandle _handle)
  {
    if(p_currentPass == null)
      throw new InvalidOperationException("WriteTextureAsDepth can only be called during RenderPass.Setup()");

    ValidateTextureHandle(_handle);

    var usage = new ResourceUsageInfo
    {
      Handle = _handle,
      AccessType = ResourceAccessType.Write,
      Usage = ResourceUsage.Default,
      State = ResourceState.DepthWrite,
      PassName = p_currentPass.Name,
    };

    p_currentPassUsages.Add(usage);
    p_currentPass.AddInput(_handle);
  }

  public void ReadTextureAsRenderTarget(ResourceHandle _handle)
  {
    if(p_currentPass == null)
      throw new InvalidOperationException("ReadTextureAsRenderTarget can only be called during RenderPass.Setup()");

    ValidateTextureHandle(_handle);

    var usage = new ResourceUsageInfo
    {
      Handle = _handle,
      AccessType = ResourceAccessType.Read,
      Usage = ResourceUsage.Default,
      State = ResourceState.RenderTarget,
      PassName = p_currentPass.Name,
    };

    p_currentPassUsages.Add(usage);
    p_currentPass.AddInput(_handle);
  }

  public IEnumerable<ResourceUsageInfo> GetResourceUsages()
  {
    return p_resourceUsages.Values;
  }

  public IEnumerable<ResourceUsageInfo> GetResourceUsages(ResourceHandle _handle)
  {
    foreach(var usage in p_resourceUsages.Values)
    {
      if(usage.Handle == _handle)
        yield return usage;
    }
  }

  public ResourceUsageInfo? GetResourceUsage(ResourceHandle _handle, string _passName)
  {
    foreach(var usage in p_resourceUsages.Values)
    {
      if(usage.Handle == _handle && usage.PassName == _passName)
        return usage;
    }
    return null;
  }

  public IEnumerable<string> GetNameResourceNames()
  {
    return p_namedResources.Keys;
  }

  public ResourceDescription GetResourceDescription(ResourceHandle _handle)
  {
    return p_resourceManager.GetResourceDescription(_handle);
  }

  public void ValidateResourceUsages()
  {
    var conflicts = new List<string>();

    var resourcesByHandle = new Dictionary<ResourceHandle, List<ResourceUsageInfo>>();

    foreach(var usage in p_resourceUsages.Values)
    {
      if(!resourcesByHandle.ContainsKey(usage.Handle))
        resourcesByHandle[usage.Handle] = new List<ResourceUsageInfo>();

      resourcesByHandle[usage.Handle].Add(usage);
    }

    foreach(var kvp in resourcesByHandle)
    {
      var handle = kvp.Key;
      var usages = kvp.Value;

      for(int i = 0; i < usages.Count; i++)
      {
        for(int j = i + 1; j < usages.Count; j++)
        {
          if(usages[i].ConflictsWith(usages[j]))
            conflicts.Add($"Resource conflict: {handle} between passes '{usages[i].PassName}' and '{usages[j].PassName}'");
        }
      }
    }

    if(conflicts.Count > 0)
      throw new InvalidOperationException($"Resource usage conflicts detected:\n{string.Join("\n", conflicts)}");
  }

  public void Clear()
  {
    p_resourceUsages.Clear();
    p_currentPass = null;
    p_namedResources.Clear();
    p_currentPassUsages.Clear();
  }

  private void ValidateTextureHandle(ResourceHandle _handle)
  {
    if(!_handle.IsValid())
      throw new ArgumentException("Invalid resource handle", nameof(_handle));

    if(_handle.Type != ResourceType.Texture1D &&
       _handle.Type != ResourceType.Texture2D &&
       _handle.Type != ResourceType.Texture3D &&
       _handle.Type != ResourceType.TextureCube &&
       _handle.Type != ResourceType.Texture2DArray &&
       _handle.Type != ResourceType.TextureCubeArray)
      throw new ArgumentException($"Handle {_handle} is not a texture handle", nameof(_handle));
  }

  private void ValidateBufferHandle(ResourceHandle _handle)
  {
    if(!_handle.IsValid())
      throw new ArgumentException("Invalid resource handle", nameof(_handle));

    if(_handle.Type != ResourceType.Buffer &&
       _handle.Type != ResourceType.StructuredBuffer &&
       _handle.Type != ResourceType.RawBuffer )
      throw new ArgumentException($"Handle {_handle} is not a buffer handle", nameof(_handle));
  }

  internal void SetCurrentPass(RenderPass _pass)
  {
    p_currentPass = _pass ?? throw new ArgumentNullException(nameof(_pass));
    p_currentPassUsages.Clear();
  }

  internal void FinishCurrentPass()
  {
    if(p_currentPass == null)
      return;

    foreach(var usage in p_currentPassUsages)
    {
      p_resourceUsages[usage.Handle] = usage;
    }

    p_currentPass = null;
    p_currentPassUsages.Clear();
  }
}
