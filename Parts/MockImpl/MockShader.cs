using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockShader: IShader
{
  public MockShader(uint _id, ShaderDescription _description)
  {
    Id = _id;
    Description = _description;
    Name = _description.Name;
    Stage = _description.Stage;
    Bytecode = _description.ByteCode ?? new byte[0];
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
    return new ShaderReflection
    {
      ConstantBuffers = new List<ConstantBufferReflection>
            {
                new ConstantBufferReflection { Name = "GlobalConstants", Size = 256, Slot = 0 }
            },
      Resources = new List<ResourceBinding>
            {
                new ResourceBinding { Name = "MainTexture", Type = ResourceBindingType.Texture, Slot = 0 }
            }
    };
  }

  public bool HasConstantBuffer(string _name) => true;
  public bool HasTexture(string _name) => true;
  public bool HasSampler(string _name) => true;

  public IntPtr GetNativeHandle() => new IntPtr(Id + 2000);
  public ulong GetMemorySize() => (ulong)Bytecode.Length;

  public void Dispose()
  {
    if(IsDisposed)
      return;

    Console.WriteLine($"    [Resource] Disposed shader {Name} (ID: {Id})");
    IsDisposed = true;

  }
}
