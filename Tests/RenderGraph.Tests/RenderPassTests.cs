using Core;
using Core.Enums;

using MockImpl;

namespace ResourcesTests;

public class RenderPassTests
{
  [Fact]
  public void RenderPass_Should_Fail_Validation_Without_Name()
  {
    // Given a render pass with empty name (разрешаем создание)
    var pass = new MockRenderPass(""); // ИСПРАВЛЕНИЕ: не кидаем exception в конструкторе

    // When validating
    var isValid = pass.Validate(out string errorMessage);

    // Then validation should fail
    Assert.False(isValid);
    Assert.Contains("name cannot be empty", errorMessage);
  }
}