using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using GraphicsAPI.Reflections;
using Resources.Enums;

namespace VulkanImpl;

public class VKShader : IShader
{
  private readonly ShaderDescription p_description;
  private bool p_disposed;

  public VKShader(ShaderDescription _description)
  {
    p_description = _description;
  }

  public ShaderDescription Description => p_description;
  public ShaderStage Stage => p_description.Stage;
  public string Name => p_description.Name;
  public byte[]? Bytecode => p_description.ByteCode;
  public bool IsDisposed => p_disposed;
  public ResourceType ResourceType => ResourceType.Shader;
  public ulong GetMemorySize() => 1024;

  public ShaderReflection GetReflection()
  {
    Console.WriteLine($"  [Vulkan] GetReflection shader {p_description.Name}");
    throw new NotImplementedException("VKShader - GetReflection not implemented");
  }

  public ConstantBufferInfo? GetConstantBufferInfo(string _name) => null;
  public ResourceBindingInfo? GetResourceInfo(string _name) => null;
  public SamplerBindingInfo? GetSamplerInfo(string _name) => null;
  public bool HasConstantBuffer(string _name) => false;
  public bool HasTexture(string _name) => false;
  public bool HasSampler(string _name) => false;
  public bool HasUnordererAccess(string _name) => false;
  public bool IsCompatibleWith(IShader _other) => true;

  public IntPtr GetNativeHandle() => IntPtr.Zero;

  public void Dispose()
  {
    if(p_disposed)
      return;
    Console.WriteLine($"  [Vulkan] Dispose shader {p_description.Name}");
    p_disposed = true;
  }
}
