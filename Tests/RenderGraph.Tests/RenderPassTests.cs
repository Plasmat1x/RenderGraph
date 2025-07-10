using Core;
using Core.Enums;

using MockImpl;

namespace ResourcesTests;

public class RenderPassTests
{
  [Fact]
  public void RenderPass_Should_Fail_Validation_Without_Name()
  {
    var pass = new MockRenderPass("");

    var isValid = pass.Validate(out string errorMessage);

    Assert.False(isValid);
    Assert.Contains("name cannot be empty", errorMessage);
  }
}