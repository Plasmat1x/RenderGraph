using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using GraphicsAPI.Reflections;
using Resources.Enums;

namespace VulkanImpl;

public class VKShader : IShader
{
  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public string Name { get; }
  public ResourceType ResourceType { get; }
  public bool IsDisposed { get; }

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public IntPtr GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public ShaderStage Stage { get; }
  public ShaderDescription Description { get; }
  public byte[] Bytecode { get; }

  public ShaderReflection GetReflection()
  {
    throw new NotImplementedException();
  }

  public bool HasConstantBuffer(string _name)
  {
    throw new NotImplementedException();
  }

  public bool HasTexture(string _name)
  {
    throw new NotImplementedException();
  }

  public bool HasSampler(string _name)
  {
    throw new NotImplementedException();
  }

  public bool HasUnordererAccess(string _name)
  {
    throw new NotImplementedException();
  }

  public ConstantBufferInfo GetConstantBufferInfo(string _name)
  {
    throw new NotImplementedException();
  }

  public ResourceBindingInfo GetResourceInfo(string _name)
  {
    throw new NotImplementedException();
  }

  public SamplerBindingInfo GetSamplerInfo(string _name)
  {
    throw new NotImplementedException();
  }

  public bool IsCompatibleWith(IShader _otherShader)
  {
    throw new NotImplementedException();
  }
}
