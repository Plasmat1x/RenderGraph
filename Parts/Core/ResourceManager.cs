using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

using System.Reflection.Metadata;

using Utility;

namespace Core;

public class ResourceManager: IDisposable
{
  private readonly Dictionary<ResourceHandle, IResource> p_resources = [];
  private readonly Dictionary<ResourceHandle, ResourceDescription> p_resourceDescriptions = [];
  private readonly ResourcePool<ITexture> p_texturePool;
  private readonly ResourcePool<IBuffer> p_bufferPool;
  private readonly Dictionary<ResourceHandle, ResourceHandle> p_alliasedResources = [];
  private readonly Dictionary<ResourceHandle, ResourceLifetime> p_resourceLifetimes = [];
  private readonly ResourceHandleGenerator p_handleGenerator = new();
  private readonly IGraphicsDevice p_device;
  private bool p_disposed = false;

  public ResourceManager(IGraphicsDevice _device)
  {
    p_device = _device ?? throw new ArgumentNullException(nameof(_device));
    p_texturePool = new ResourcePool<ITexture>(() => null);
    p_bufferPool = new ResourcePool<IBuffer>(() => null);

  }
  public ResourceHandle CreateBuffer(BufferDescription _desc)
  {
    if(_desc == null)
      throw new ArgumentNullException(nameof(_desc));

    var handle = p_handleGenerator.Generate(ResourceType.Buffer, _desc.Name);
    var buffer = TryGetFromBufferPool(_desc);

    if(buffer == null)
      buffer = p_device.CreateBuffer(_desc);

    p_resources[handle] = buffer;
    p_resourceDescriptions[handle] = _desc;
    p_resourceLifetimes[handle] = ResourceLifetime.Transient;

    return handle;
  }

  public ResourceHandle CreateTexture(TextureDescription _desc)
  {
    if(_desc == null)
      throw new ArgumentNullException(nameof(_desc));

    var handle = p_handleGenerator.Generate(GetResourceTypeFromTexture(_desc), _desc.Name);
    var texture = TryGetFromTexturePool(_desc);

    if(texture == null)
      texture = p_device.CreateTexture(_desc);

    p_resources[handle] = texture;
    p_resourceDescriptions[handle] = _desc;
    p_resourceLifetimes[handle] = ResourceLifetime.Transient;

    return handle;
  }

  public ITexture GetTexture(ResourceHandle _handle)
  {
    if(!_handle.IsValid())
      throw new ArgumentException("Invalid resource handle provided", nameof(_handle));

    var actualHandle = GetActualHandle(_handle);

    if(p_resources.TryGetValue(actualHandle, out var resource))
    {
      if(resource is ITexture texture)
        return texture;
      else
        throw new InvalidOperationException($"Resource {_handle} is not a texture");
    }
    throw new ArgumentException($"Resource not found: {_handle}", nameof(_handle));

  }

  public IBuffer GetBuffer(ResourceHandle _handle)
  {
    if(!_handle.IsValid())
      throw new ArgumentException("Invalid resource handle provided", nameof(_handle));

    var actualHandle = GetActualHandle(_handle);

    if(p_resources.TryGetValue(_handle, out var resource))
    {
      if(resource is IBuffer buffer)
        return buffer;
      else
        throw new InvalidOperationException($"Resource {_handle} is not a buffer");
    }
    throw new ArgumentException($"Resource not found: {_handle}", nameof(_handle));

  }

  public void AlliaseResource(ResourceHandle _source, ResourceHandle _target)
  {
    if(!IsValidHandle(_source))
      throw new ArgumentException($"Invalid source handle: {_source}");
    if(!IsValidHandle(_target))
      throw new ArgumentException($"Invalid target handle: {_target}");

    if(!AreResourceComaptible(_source, _target))
      throw new InvalidOperationException($"Resources are not compatible for aliasing: {_source} -> {_target}");

    p_alliasedResources[_target] = _source;
  }

  public void TransitionResource(ResourceHandle _handle, ResourceState _state)
  {
    if(!IsValidHandle(_handle))
      throw new ArgumentException($"Invalid resource handle: {_handle}");

    //TODO: Implement actual resource transition
  }

  public ResourceDescription GetResourceDescription(ResourceHandle _handle)
  {
    if(!IsValidHandle(_handle))
      throw new ArgumentException($"Invalid resource handle: {_handle}");

    var actualHandle = GetActualHandle(_handle);

    if(p_resourceDescriptions.TryGetValue(actualHandle, out var description))
      return description;

    throw new KeyNotFoundException($"Resource description not found: {_handle}");
  }

  public void OptimizeResourceUsage()
  {
    var unusedHandles = new List<ResourceHandle>();

    foreach(var kvp in p_resources)
    {
      var handle = kvp.Key;
      var lifetime = p_resourceLifetimes.GetValueOrDefault(handle, ResourceLifetime.Transient);

      if(lifetime == ResourceLifetime.Transient)
      {
        //TODO: RG Deps Analyze
      }
    }

    foreach(var handle in unusedHandles)
    {
      ReleaseResource(handle);
    }

    p_texturePool.Defragment();
    p_bufferPool.Defragment();
  }

  public MemoryUsageInfo GetMemoryUsage()
  {
    ulong totalTextuMemory = 0;
    ulong totalBufferMemory = 0;
    ulong totalAllocated = 0;

    foreach(var kvp in p_resourceDescriptions)
    {
      var description = kvp.Value;
      var memorySize = description.GetMemorySize();
      totalAllocated += memorySize;

      if(description is TextureDescription)
        totalTextuMemory += memorySize;
      else if(description is BufferDescription)
        totalBufferMemory += memorySize;
    }

    return new MemoryUsageInfo
    {
      TotalAllocated = totalAllocated,
      TotalUsed = totalAllocated, //TODO: Observe usage
      TextureMemory = totalTextuMemory,
      BufferMemory = totalBufferMemory,
      PeakUsage = totalAllocated, //TODO: Observe peaks
    };
  }

  public void ReleaseResource(ResourceHandle _handle)
  {
    if(!IsValidHandle(_handle))
      return;

    var actualHandle = GetActualHandle(_handle);

    if(p_resources.TryGetValue(actualHandle, out var resource))
    {
      var lifettime = p_resourceLifetimes.GetValueOrDefault(actualHandle, ResourceLifetime.Transient);

      if(lifettime == ResourceLifetime.Transient)
      {
        ReturnToPool(resource);
      }
      else if(lifettime == ResourceLifetime.Persistent)
      {
        return;
      }
      else if(lifettime == ResourceLifetime.External || lifettime == ResourceLifetime.Imported)
      {
        p_resources.Remove(actualHandle);
        p_resourceDescriptions.Remove(actualHandle);
        p_resourceLifetimes.Remove(actualHandle);
        return;
      }

      p_resources.Remove(actualHandle);
      p_resourceDescriptions.Remove(actualHandle);
      p_resourceLifetimes.Remove(actualHandle);
    }
    p_handleGenerator.Release(_handle);
    p_alliasedResources.Remove(_handle);
  }

  public ResourceLifetime GetResourceLifetime(ResourceHandle _handle)
  {
    if(!IsValidHandle(_handle))
      throw new ArgumentException($"Invalid resource handle: {_handle}");

    return p_resourceLifetimes.GetValueOrDefault(_handle, ResourceLifetime.Transient);
  }

  public void SetResourceLifetime(ResourceHandle _handle, ResourceLifetime _lifetime)
  {
    if(!IsValidHandle(_handle))
      throw new ArgumentException($"Invalid resource handle: {_handle}");

    p_resourceLifetimes[_handle] = _lifetime;
  }

  public ResourceHandle ImportTexture(string _name, ITexture _texture)
  {
    if(_texture == null)
      throw new ArgumentNullException(nameof(_texture));

    var handle = p_handleGenerator.Generate(ResourceType.Texture2D, _name);
    p_resourceDescriptions[handle] = _texture.Description;
    p_resourceLifetimes[handle] = ResourceLifetime.Imported;

    return handle;
  }

  public ResourceHandle ImportBuffer(string _name, IBuffer _buffer)
  {
    if(_buffer == null)
      throw new ArgumentNullException(nameof(_buffer));

    var handle = p_handleGenerator.Generate(ResourceType.Buffer, _name);
    p_resourceDescriptions[handle] = _buffer.Description;
    p_resourceLifetimes[handle] = ResourceLifetime.Imported;

    return handle;
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    foreach(var resource in p_resources.Values)
    {
      resource?.Dispose();
    }

    p_resources.Clear();
    p_resourceDescriptions.Clear();
    p_resourceLifetimes.Clear();
    p_resourceLifetimes.Clear();
    p_texturePool?.Dispose();
    p_bufferPool?.Dispose();

    p_disposed = true;
  }

  private bool IsValidHandle(ResourceHandle _handle) => _handle.IsValid() && p_handleGenerator.IsHandleValid(_handle);

  private ResourceHandle GetActualHandle(ResourceHandle _handle)
  {
    if(p_alliasedResources.TryGetValue(_handle, out var actualHandle))
      return actualHandle;
    return _handle;
  }

  private bool AreResourceComaptible(ResourceHandle _source, ResourceHandle _target)
  {
    if(!p_resourceDescriptions.TryGetValue(_source, out var sourceDesc))
      return false;
    if(!p_resourceDescriptions.TryGetValue(_target, out var targetDesc))
      return false;

    return sourceDesc.IsCompatible(targetDesc);
  }

  private ITexture TryGetFromTexturePool(TextureDescription _desc)
  {
    var availableTextures = p_texturePool.GetAvailableResources();

    foreach(var texture in availableTextures)
    {
      if(texture.Description.IsCompatible(_desc))
        return p_texturePool.Rent();
    }

    return null;
  }

  private IBuffer TryGetFromBufferPool(BufferDescription _desc)
  {
    var availableBuffers = p_bufferPool.GetAvailableResources();

    foreach(var buffer in availableBuffers)
    {
      if(buffer.Description.IsCompatible(_desc))
        return p_bufferPool.Rent();
    }

    return null;
  }

  private void ReturnToPool(IResource _resource)
  {
    switch(_resource)
    {
      case ITexture texture:
        p_texturePool.Return(texture);
        break;
      case IBuffer buffer:
        p_bufferPool.Return(buffer);
        break;
    }
  }

  private static ResourceType GetResourceTypeFromTexture(TextureDescription _desc)
  {
    return _desc.Depth > 1 ? ResourceType.Texture3D :
      _desc.ArraySize > 1 ? ResourceType.Texture2DArray :
      ResourceType.Texture2D;
  }
}
