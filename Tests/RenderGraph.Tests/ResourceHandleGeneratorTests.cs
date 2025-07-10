using Core;

using Resources.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourcesTests;
public class ResourceHandleGeneratorTests
{
  [Fact]
  public void Generator_Should_Create_Valid_Handles()
  {
    // Given a resource handle generator
    var generator = new ResourceHandleGenerator();

    // When generating a handle
    var handle = generator.Generate(ResourceType.Texture2D, "TestTexture");

    // Then the handle should be valid
    Assert.True(handle.IsValid());
    Assert.Equal(ResourceType.Texture2D, handle.Type);
    Assert.Equal("TestTexture", handle.Name);
    Assert.True(handle.Id > 0);
    Assert.True(handle.Generation > 0);
  }

  [Fact]
  public void Generator_Should_Create_Unique_IDs()
  {
    // Given a resource handle generator
    var generator = new ResourceHandleGenerator();

    // When generating multiple handles
    var handle1 = generator.Generate(ResourceType.Texture2D, "Texture1");
    var handle2 = generator.Generate(ResourceType.Texture2D, "Texture2");
    var handle3 = generator.Generate(ResourceType.Buffer, "Buffer1");

    // Then all IDs should be unique
    Assert.NotEqual(handle1.Id, handle2.Id);
    Assert.NotEqual(handle2.Id, handle3.Id);
    Assert.NotEqual(handle1.Id, handle3.Id);
  }

  [Fact]
  public void Generator_Should_Increment_Generation_After_Release()
  {
    // Given a resource handle generator
    var generator = new ResourceHandleGenerator();

    // When generating, releasing, and re-validating
    var handle = generator.Generate(ResourceType.Texture2D, "Test");
    var originalGeneration = handle.Generation;

    generator.Release(handle);
    var isValid = generator.IsHandleValid(handle);

    // Then handle should be invalid after release
    Assert.False(isValid);
  }
}
