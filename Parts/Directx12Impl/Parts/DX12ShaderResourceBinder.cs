using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

namespace Directx12Impl.Parts;

/// <summary>
/// Менеджер шейдерных ресурсов для автоматического связывания
/// </summary>
public class DX12ShaderResourceBinder
{
  private readonly DX12GraphicsDevice p_device;
  private readonly DX12CommandBuffer p_commandBuffer;
  private readonly Dictionary<string, IResource> p_namedResources = new();

  public DX12ShaderResourceBinder(DX12GraphicsDevice _device, DX12CommandBuffer _commandBuffer)
  {
    p_device = _device;
    p_commandBuffer = _commandBuffer;
  }

  public void RegisterResource(string _name, IResource _resource)
  {
    p_namedResources[_name] = _resource;
  }

  public void AutoBindShaderResources(DX12Shader _shader)
  {
    var reflection = _shader.GetReflection();

    // Автоматическое связывание константных буферов
    foreach(var cb in reflection.ConstantBuffers)
    {
      if(p_namedResources.TryGetValue(cb.Name, out var resource))
      {
        if(resource is IBufferView bufferView)
        {
          p_commandBuffer.SetConstantBuffer(_shader.Stage, cb.BindPoint, bufferView);
        }
      }
    }

    // Автоматическое связывание текстур
    foreach(var tex in reflection.BoundResources)
    {
      if(tex.Type == ResourceBindingType.ShaderResource)
      {
        if(p_namedResources.TryGetValue(tex.Name, out var resource))
        {
          if(resource is ITextureView textureView)
          {
            p_commandBuffer.SetShaderResource(_shader.Stage, tex.BindPoint, textureView);
          }
        }
      }
    }

    // Автоматическое связывание сэмплеров
    foreach(var sampler in reflection.Samplers)
    {
      if(p_namedResources.TryGetValue(sampler.Name, out var resource))
      {
        if(resource is ISampler samplerResource)
        {
          p_commandBuffer.SetSampler(_shader.Stage, sampler.BindPoint, samplerResource);
        }
      }
    }

    // Автоматическое связывание UAV
    foreach(var uav in reflection.UnorderedAccessViews)
    {
      if(p_namedResources.TryGetValue(uav.Name, out var resource))
      {
        if(resource is ITextureView textureView)
        {
          p_commandBuffer.SetUnorderedAccess(_shader.Stage, uav.BindPoint, textureView);
        }
      }
    }
  }

  public List<string> GetMissingResources(DX12Shader _shader)
  {
    var missing = new List<string>();
    var reflection = _shader.GetReflection();

    foreach(var cb in reflection.ConstantBuffers)
    {
      if(!p_namedResources.ContainsKey(cb.Name))
        missing.Add($"CB: {cb.Name}");
    }

    foreach(var tex in reflection.BoundResources)
    {
      if(!p_namedResources.ContainsKey(tex.Name))
        missing.Add($"Texture: {tex.Name}");
    }

    foreach(var sampler in reflection.Samplers)
    {
      if(!p_namedResources.ContainsKey(sampler.Name))
        missing.Add($"Sampler: {sampler.Name}");
    }

    foreach(var uav in reflection.UnorderedAccessViews)
    {
      if(!p_namedResources.ContainsKey(uav.Name))
        missing.Add($"UAV: {uav.Name}");
    }

    return missing;
  }
}

