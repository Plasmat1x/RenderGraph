using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using GraphicsAPI.Reflections;
using GraphicsAPI.Reflections.Extensions;

using Resources.Enums;

namespace MockImpl;

public class MockShader: IShader
{
  private readonly ShaderReflection p_reflection;
  private static readonly MockShaderReflectionProvider s_reflectionProvider = new();

  public MockShader(uint _id, ShaderDescription _description)
  {
    Id = _id;
    Description = _description;
    Name = _description.Name;
    Stage = _description.Stage;
    Bytecode = _description.ByteCode ?? new byte[0];

    if(_description.CachedReflection != null)
    {
      p_reflection = _description.CachedReflection;
    }
    else
    {
      p_reflection = s_reflectionProvider.CreateReflection(Bytecode, Stage);
      _description.CachedReflection = p_reflection;
    }

    Console.WriteLine($"[MockShader] Created shader '{Name}' (Stage: {Stage}, ID: {Id})");
    LogReflectionInfo();
  }

  public uint Id { get; }
  public string Name { get; set; }
  public ResourceType ResourceType => ResourceType.Buffer;
  public bool IsDisposed { get; private set; }
  public ShaderStage Stage { get; }
  public ShaderDescription Description { get; }
  public byte[] Bytecode { get; }


  public ShaderReflection GetReflection()
  {
    Console.WriteLine($"    [Resource] Getting reflection for shader {Name}");
    return p_reflection;
  }

  public bool HasConstantBuffer(string _name) => p_reflection.ConstantBuffers.Any(_cb => _cb.Name == _name);

  public bool HasTexture(string _name) => p_reflection.BoundResources.Any(_r => _r.Name == _name && _r.Type == ResourceBindingType.ShaderResource);

  public bool HasSampler(string _name) => p_reflection.Samplers.Any(_s => _s.Name == _name);

  public bool HasUnordererAccess(string _name) => p_reflection.UnorderedAccessViews.Any(_uav => _uav.Name == _name);

  public ConstantBufferInfo GetConstantBufferInfo(string _name) => p_reflection.GetConstantBuffer(_name);

  public ResourceBindingInfo GetResourceInfo(string _name) => p_reflection.GetResource(_name);

  public SamplerBindingInfo GetSamplerInfo(string _name) => p_reflection.GetSampler(_name);

  public IntPtr GetNativeHandle() => new IntPtr(Id + 2000);

  public ulong GetMemorySize() => (ulong)Bytecode.Length;

  public bool IsCompatibleWith(IShader _otherShader)
  {
    if(_otherShader == null)
      return false;

    var otherReflection = _otherShader.GetReflection();
    return p_reflection.Compatible(otherReflection);
  }

  public void Dispose()
  {
    if(IsDisposed)
      return;

    Console.WriteLine($"    [Resource] Disposed shader {Name} (ID: {Id})");
    IsDisposed = true;

  }

  private void LogReflectionInfo()
  {
    Console.WriteLine($"[MockShader] Reflection info for '{Name}':");
    Console.WriteLine($"  - Shader Model: {p_reflection.Info.ShaderModel}");
    Console.WriteLine($"  - Instruction Count: {p_reflection.Info.InstructionCount}");

    if(p_reflection.ConstantBuffers.Count > 0)
    {
      Console.WriteLine($"  - Constant Buffers ({p_reflection.ConstantBuffers.Count}):");
      foreach(var cb in p_reflection.ConstantBuffers)
      {
        Console.WriteLine($"    * {cb.Name} (Slot: {cb.BindPoint}, Size: {cb.Size} bytes)");
        foreach(var var in cb.Variables)
        {
          Console.WriteLine($"      - {var.Name}: {var.Type} (Offset: {var.Offset}, Size: {var.Size})");
        }
      }
    }

    if(p_reflection.BoundResources.Count > 0)
    {
      Console.WriteLine($"  - Bound Resources ({p_reflection.BoundResources.Count}):");
      foreach(var res in p_reflection.BoundResources)
      {
        Console.WriteLine($"    * {res.Name} (Slot: {res.BindPoint}, Type: {res.Dimension})");
      }
    }

    if(p_reflection.Samplers.Count > 0)
    {
      Console.WriteLine($"  - Samplers ({p_reflection.Samplers.Count}):");
      foreach(var sampler in p_reflection.Samplers)
      {
        Console.WriteLine($"    * {sampler.Name} (Slot: {sampler.BindPoint})");
      }
    }

    if(p_reflection.UnorderedAccessViews.Count > 0)
    {
      Console.WriteLine($"  - UAVs ({p_reflection.UnorderedAccessViews.Count}):");
      foreach(var uav in p_reflection.UnorderedAccessViews)
      {
        Console.WriteLine($"    * {uav.Name} (Slot: {uav.BindPoint}, Type: {uav.Dimension})");
      }
    }

    if(Stage == ShaderStage.Vertex && p_reflection.InputParameters.Count > 0)
    {
      Console.WriteLine($"  - Input Parameters ({p_reflection.InputParameters.Count}):");
      foreach(var input in p_reflection.InputParameters)
      {
        Console.WriteLine($"    * {input.SemanticName}{input.SemanticIndex} (Register: {input.Register}, Components: {input.GetComponentCount()})");
      }
    }

    if(Stage == ShaderStage.Compute)
    {
      Console.WriteLine($"  - Thread Group Size: [{p_reflection.ThreadGroupSize.X}, {p_reflection.ThreadGroupSize.Y}, {p_reflection.ThreadGroupSize.Z}]");
    }
  }

}
