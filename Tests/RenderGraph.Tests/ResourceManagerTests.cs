using Core;

using MockImpl;

using Resources;
using Resources.Enums;

namespace ResourcesTests;

public class ResourceManagerTests: IDisposable
{
  private readonly MockGraphicsDevice p_device;
  private readonly ResourceManager p_resourceManager;

  public ResourceManagerTests()
  {
    p_device = new MockGraphicsDevice();
    p_resourceManager = new ResourceManager(p_device);
  }

  [Fact]
  public void ResourceManager_Should_Handle_Invalid_Handle()
  {
    // Given an invalid resource handle
    var invalidHandle = ResourceHandle.Invalid;

    // When trying to get a texture with invalid handle
    // Then it should throw ArgumentException (не ArgumentNullException)
    Assert.Throws<ArgumentException>(() => p_resourceManager.GetTexture(invalidHandle));
    Assert.Throws<ArgumentException>(() => p_resourceManager.GetBuffer(invalidHandle));
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
    var handle = p_resourceManager.CreateTexture(desc);

    // When releasing the resource
    p_resourceManager.ReleaseResource(handle);

    // Then getting the texture should throw ArgumentException (не KeyNotFoundException)
    Assert.Throws<ArgumentException>(() => p_resourceManager.GetTexture(handle));
  }

  [Fact]
  public void ResourceManager_Should_Handle_Invalid_Handle_Fixed()
  {
    // Given an EXPLICITLY invalid resource handle
    var invalidHandle = new ResourceHandle(0, ResourceType.Texture2D, 0, ""); // Explicitly invalid

    // When trying to get a texture with invalid handle
    // Then it should throw ArgumentException
    var ex1 = Assert.Throws<ArgumentException>(() => p_resourceManager.GetTexture(invalidHandle));
    var ex2 = Assert.Throws<ArgumentException>(() => p_resourceManager.GetBuffer(invalidHandle));

    // Verify the exception type is correct
    Assert.IsType<ArgumentException>(ex1);
    Assert.IsType<ArgumentException>(ex2);
  }

  public void Dispose()
  {
    p_resourceManager?.Dispose();
    p_device?.Dispose();
  }
}
