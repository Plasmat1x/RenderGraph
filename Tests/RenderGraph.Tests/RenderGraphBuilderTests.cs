using Core;
using Core.Enums;

using MockImpl;

namespace ResourcesTests;

public class RenderGraphBuilderTests: IDisposable
{
  private readonly MockGraphicsDevice _device;
  private readonly ResourceManager _resourceManager;
  private readonly RenderGraphBuilder _builder;

  public RenderGraphBuilderTests()
  {
    _device = new MockGraphicsDevice();
    _resourceManager = new ResourceManager(_device);
    _builder = new RenderGraphBuilder(_resourceManager);
  }

  [Fact]
  public void Builder_Should_Track_Resource_Usage()
  {
    // Given a mock render pass and texture
    var mockPass = new MockRenderPass("TestPass");
    _builder.SetCurrentPass(mockPass);

    var textureHandle = _builder.CreateColorTarget("TestTexture", 256, 256);

    // When marking texture for read and write separately
    _builder.ReadTexture(textureHandle);

    // ИСПРАВЛЕНИЕ: Создаем отдельный pass для write, чтобы было 2 usage
    _builder.FinishCurrentPass();

    var anotherPass = new MockRenderPass("AnotherPass");
    _builder.SetCurrentPass(anotherPass);
    _builder.WriteTexture(textureHandle);
    _builder.FinishCurrentPass();

    // Then usage should be tracked
    var usages = _builder.GetResourceUsages(textureHandle).ToList();
    Assert.Contains(usages, u => u.AccessType == ResourceAccessType.Read);
    Assert.Contains(usages, u => u.AccessType == ResourceAccessType.Write);
  }

  [Fact]
  public void Builder_Should_Validate_Resource_Conflicts()
  {
    // Given two passes that both write to same resource
    var pass1 = new MockRenderPass("Pass1");
    var pass2 = new MockRenderPass("Pass2");

    _builder.SetCurrentPass(pass1);
    var handle = _builder.CreateColorTarget("SharedTexture", 256, 256);
    _builder.WriteTexture(handle);
    _builder.FinishCurrentPass();

    _builder.SetCurrentPass(pass2);
    _builder.WriteTexture(handle); // Conflict: both passes write to same resource
    _builder.FinishCurrentPass();

    // When validating resource usage
    // Then it should detect conflicts
    Assert.Throws<InvalidOperationException>(() => _builder.ValidateResourceUsages());
  }

  public void Dispose()
  {
    _resourceManager?.Dispose();
    _device?.Dispose();
  }
}
