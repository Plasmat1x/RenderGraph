using Core;

using Resources.Enums;

namespace ResourcesTests;

public class ResourceHandleTests
{
  [Fact]
  public void ResourceHandle_Creation_Should_Be_Valid()
  {
    // Given a valid resource handle parameters
    uint id = 42;
    var type = ResourceType.Texture2D;
    uint generation = 1;
    string name = "TestTexture";

    // When creating a resource handle
    var handle = new ResourceHandle(id, type, generation, name);

    // Then the handle should be valid
    Assert.True(handle.IsValid());
    Assert.Equal(id, handle.Id);
    Assert.Equal(type, handle.Type);
    Assert.Equal(generation, handle.Generation);
    Assert.Equal(name, handle.Name);
  }

  [Fact]
  public void ResourceHandle_Default_Should_Be_Invalid()
  {
    // Given a default resource handle
    var handle = new ResourceHandle();

    // When checking validity
    // Then it should be invalid
    Assert.False(handle.IsValid());
  }

  [Fact]
  public void ResourceHandle_Invalid_Constant_Should_Be_Invalid()
  {
    // Given an invalid resource handle
    var handle = ResourceHandle.Invalid;

    // When checking validity
    // Then it should be invalid
    Assert.False(handle.IsValid());
  }

  [Fact]
  public void ResourceHandle_Equality_Should_Work_Correctly()
  {
    // Given two identical resource handles
    var handle1 = new ResourceHandle(1, ResourceType.Texture2D, 1, "Test");
    var handle2 = new ResourceHandle(1, ResourceType.Texture2D, 1, "Test");
    var handle3 = new ResourceHandle(2, ResourceType.Texture2D, 1, "Test");

    // When comparing them
    // Then equality should work correctly
    Assert.Equal(handle1, handle2);
    Assert.NotEqual(handle1, handle3);
    Assert.True(handle1 == handle2);
    Assert.True(handle1 != handle3);
  }

  [Fact]
  public void ResourceHandle_HashCode_Should_Be_Consistent()
  {
    // Given two identical resource handles
    var handle1 = new ResourceHandle(1, ResourceType.Texture2D, 1, "Test");
    var handle2 = new ResourceHandle(1, ResourceType.Texture2D, 1, "Test");

    // When getting hash codes
    // Then they should be equal
    Assert.Equal(handle1.GetHashCode(), handle2.GetHashCode());
  }
}
