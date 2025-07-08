using Core;

using MockImpl;

using Resources;
using Resources.Enums;

namespace ResourcesTests;

public class ResourceManagerTests: IDisposable
{
  private readonly MockGraphicsDevice _device;
  private readonly ResourceManager _resourceManager;

  public ResourceManagerTests()
  {
    _device = new MockGraphicsDevice();
    _resourceManager = new ResourceManager(_device);
  }

  [Fact]
  public void ResourceManager_Should_Handle_Invalid_Handle()
  {
    // Given an invalid resource handle
    var invalidHandle = ResourceHandle.Invalid;

    // When trying to get a texture with invalid handle
    // Then it should throw ArgumentException (не ArgumentNullException)
    Assert.Throws<ArgumentException>(() => _resourceManager.GetTexture(invalidHandle));
    Assert.Throws<ArgumentException>(() => _resourceManager.GetBuffer(invalidHandle));
  }

  [Fact]
  public void ResourceManager_Should_Release_Resources()
  {
    // Given a created texture
    var desc = new TextureDescription
    {
      Name = "TestTexture",
      Width = 256,
      Height = 256,
      Format = TextureFormat.R8G8B8A8_UNORM
    };
    var handle = _resourceManager.CreateTexture(desc);

    // When releasing the resource
    _resourceManager.ReleaseResource(handle);

    // Then getting the texture should throw ArgumentException (не KeyNotFoundException)
    Assert.Throws<ArgumentException>(() => _resourceManager.GetTexture(handle));
  }

  [Fact]
  public void ResourceManager_Should_Handle_Invalid_Handle_Fixed()
  {
    // Given an EXPLICITLY invalid resource handle
    var invalidHandle = new ResourceHandle(0, ResourceType.Texture2D, 0, ""); // Explicitly invalid

    // When trying to get a texture with invalid handle
    // Then it should throw ArgumentException
    var ex1 = Assert.Throws<ArgumentException>(() => _resourceManager.GetTexture(invalidHandle));
    var ex2 = Assert.Throws<ArgumentException>(() => _resourceManager.GetBuffer(invalidHandle));

    // Verify the exception type is correct
    Assert.IsType<ArgumentException>(ex1);
    Assert.IsType<ArgumentException>(ex2);
  }

  public void Dispose()
  {
    _resourceManager?.Dispose();
    _device?.Dispose();
  }
}
