using Core;
using Core.Enums;

using MockImpl;

namespace ResourcesTests;

public class RenderGraphBuilderTests: IDisposable
{
  private readonly MockGraphicsDevice p_device;
  private readonly ResourceManager p_resourceManager;
  private readonly RenderGraphBuilder p_builder;

  public RenderGraphBuilderTests()
  {
    p_device = new MockGraphicsDevice();
    p_resourceManager = new ResourceManager(p_device);
    p_builder = new RenderGraphBuilder(p_resourceManager);
  }

  [Fact]
  public void Builder_Should_Track_Resource_Usage()
  {
    // Given a mock render pass and texture
    var mockPass = new MockRenderPass("TestPass");
    p_builder.SetCurrentPass(mockPass);

    var textureHandle = p_builder.CreateColorTarget("TestTexture", 256, 256);

    // When marking texture for read and write separately
    p_builder.ReadTexture(textureHandle);

    // ИСПРАВЛЕНИЕ: Создаем отдельный pass для write, чтобы было 2 usage
    p_builder.FinishCurrentPass();

    var anotherPass = new MockRenderPass("AnotherPass");
    p_builder.SetCurrentPass(anotherPass);
    p_builder.WriteTexture(textureHandle);
    p_builder.FinishCurrentPass();

    // Then usage should be tracked
    var usages = p_builder.GetResourceUsages(textureHandle).ToList();
    Assert.Contains(usages, _u => _u.AccessType == ResourceAccessType.Read);
    Assert.Contains(usages, _u => _u.AccessType == ResourceAccessType.Write);
  }

  [Fact]
  public void Builder_Should_Validate_Resource_Conflicts()
  {
    // Given two passes that both write to same resource
    var pass1 = new MockRenderPass("Pass1");
    var pass2 = new MockRenderPass("Pass2");

    p_builder.SetCurrentPass(pass1);
    var handle = p_builder.CreateColorTarget("SharedTexture", 256, 256);
    p_builder.WriteTexture(handle);
    p_builder.FinishCurrentPass();

    p_builder.SetCurrentPass(pass2);
    p_builder.WriteTexture(handle); // Conflict: both passes write to same resource
    p_builder.FinishCurrentPass();

    // When validating resource usage
    // Then it should detect conflicts
    Assert.Throws<InvalidOperationException>(() => p_builder.ValidateResourceUsages());
  }

  public void Dispose()
  {
    p_resourceManager?.Dispose();
    p_device?.Dispose();
  }
}
