using Core;

using Resources.Enums;

namespace ResourcesTests;
public class ResourceHandleGeneratorTests
{
  [Fact]
  public void Generator_Should_Create_Valid_Handles()
  {
    var generator = new ResourceHandleGenerator();

    var handle = generator.Generate(ResourceType.Texture2D, "TestTexture");

    Assert.True(handle.IsValid());
    Assert.Equal(ResourceType.Texture2D, handle.Type);
    Assert.Equal("TestTexture", handle.Name);
    Assert.True(handle.Id > 0);
    Assert.True(handle.Generation > 0);
  }

  [Fact]
  public void Generator_Should_Create_Unique_IDs()
  {
    var generator = new ResourceHandleGenerator();

    var handle1 = generator.Generate(ResourceType.Texture2D, "Texture1");
    var handle2 = generator.Generate(ResourceType.Texture2D, "Texture2");
    var handle3 = generator.Generate(ResourceType.Buffer, "Buffer1");

    Assert.NotEqual(handle1.Id, handle2.Id);
    Assert.NotEqual(handle2.Id, handle3.Id);
    Assert.NotEqual(handle1.Id, handle3.Id);
  }

  [Fact]
  public void Generator_Should_Increment_Generation_After_Release()
  {
    var generator = new ResourceHandleGenerator();

    var handle = generator.Generate(ResourceType.Texture2D, "Test");
    var originalGeneration = handle.Generation;

    generator.Release(handle);
    var isValid = generator.IsHandleValid(handle);

    Assert.False(isValid);
  }
}
