using Core;

using Resources.Enums;

namespace ResourcesTests;

public class ResourceHandleTests
{
  [Fact]
  public void ResourceHandle_Creation_Should_Be_Valid()
  {
    uint id = 42;
    var type = ResourceType.Texture2D;
    uint generation = 1;
    string name = "TestTexture";

    var handle = new ResourceHandle(id, type, generation, name);

    Assert.True(handle.IsValid());
    Assert.Equal(id, handle.Id);
    Assert.Equal(type, handle.Type);
    Assert.Equal(generation, handle.Generation);
    Assert.Equal(name, handle.Name);
  }

  [Fact]
  public void ResourceHandle_Default_Should_Be_Invalid()
  {
    var handle = new ResourceHandle();

    Assert.False(handle.IsValid());
  }

  [Fact]
  public void ResourceHandle_Invalid_Constant_Should_Be_Invalid()
  {
    var handle = ResourceHandle.Invalid;

    Assert.False(handle.IsValid());
  }

  [Fact]
  public void ResourceHandle_Equality_Should_Work_Correctly()
  {
    var handle1 = new ResourceHandle(1, ResourceType.Texture2D, 1, "Test");
    var handle2 = new ResourceHandle(1, ResourceType.Texture2D, 1, "Test");
    var handle3 = new ResourceHandle(2, ResourceType.Texture2D, 1, "Test");

    Assert.Equal(handle1, handle2);
    Assert.NotEqual(handle1, handle3);
    Assert.True(handle1 == handle2);
    Assert.True(handle1 != handle3);
  }

  [Fact]
  public void ResourceHandle_HashCode_Should_Be_Consistent()
  {
    var handle1 = new ResourceHandle(1, ResourceType.Texture2D, 1, "Test");
    var handle2 = new ResourceHandle(1, ResourceType.Texture2D, 1, "Test");

    Assert.Equal(handle1.GetHashCode(), handle2.GetHashCode());
  }
}
