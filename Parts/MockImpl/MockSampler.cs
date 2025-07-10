using Core.Enums;

using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockSampler: ISampler
{
  public string Name { get; set; }
  public ResourceType ResourceType => ResourceType.Buffer;
  public bool IsDisposed { get; private set; }
  public SamplerDescription Description { get; }

  public MockSampler(SamplerDescription description)
  {
    Description = description;
    Name = description.Name;
  }

  public IntPtr GetNativeHandle() => new IntPtr(12345);
  public ulong GetMemorySize() => 64; // Small sampler object

  public void Dispose()
  {
    if(!IsDisposed)
    {
      Console.WriteLine($"    [Resource] Disposed sampler {Name}");
      IsDisposed = true;
    }
  }
}
