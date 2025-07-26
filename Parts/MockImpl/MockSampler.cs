using Core.Enums;

using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace MockImpl;

public class MockSampler: ISampler
{
  public MockSampler(SamplerDescription _description)
  {
    Description = _description;
    Name = _description.Name;
  }

  public string Name { get; set; }
  public ResourceType ResourceType => ResourceType.Buffer;
  public bool IsDisposed { get; private set; }
  public SamplerDescription Description { get; }
  public IntPtr GetNativeHandle() => new IntPtr(12345);
  public ulong GetMemorySize() => 64;

  public void Dispose()
  {
    if(IsDisposed)
      return;

    Console.WriteLine($"    [Resource] Disposed sampler {Name}");
    IsDisposed = true;
  }
}
