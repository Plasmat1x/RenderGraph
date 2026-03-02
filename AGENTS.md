# AGENTS.md - RenderGraph Developer Guide

## Pipeline: Production Lifecycle

**Rule #1**: Every agent MUST sync with `.opencode/current_task_plan.md`.
**Rule #2**: Communication between agents is done via `@agentName` tags.

### [Phase 0] Analysis (@analyst)
- **Goal**: Requirements decomposition and impact analysis.
- **Action**: Write detailed step-by-step execution plan to `.opencode/current_task_plan.md`.
- **Hand-off**: `👉 @architect, the plan is ready in .opencode/current_task_plan.md. Design the interfaces.`

### [Phase 1] Design (@architect)
- **Goal**: High-level technical design and contract definitions.
- **Action**: Append technical specs (classes, methods, APIs) to `.opencode/current_task_plan.md`.
- **Hand-off**: `👉 @coder, design is approved. Proceed with implementation.`

### [Phase 2] Implementation (@coder)
- **Goal**: Clean, production-ready code.
- **Rules**: Follow project-specific style guides (see Code Style Guidelines below).
- **Hand-off**: `👉 @reviewer, implementation is done. Please review for bugs and architecture fit.`

### [Phase 3] Verification (@reviewer)
- **Goal**: Code quality, security, and logic check.
- **Decision**:
  - If issues: `👉 @coder, [list of issues]`.
  - If OK: `👉 @tester, code verified. Write and run tests.`

### [Phase 3.5] Security Audit (@security)
- **Goal**: Identify vulnerabilities, hardcoded secrets, and injection points.
- **Action**: Run static analysis or manual code audit.
- **Logic**:
  - If vulnerabilities found: `👉 @coder, SECURITY ALERT: [vulnerability details]. Fix immediately.`
  - If safe: `👉 @tester, security check passed. Proceed to verification.`

### [Phase 4] Quality Assurance (@tester)
- **Goal**: Unit/Integration testing and coverage.
- **Action**: Create tests and run them. Fix code if tests fail.
- **Hand-off**: `👉 @writer, tests passed. Finalize documentation.`

### [Phase 5] Documentation (@writer)
- **Goal**: Project clarity and technical debt logging.
- **Action**: Update README, API docs, and comments.
- **Exit**: Summarize task completion for the User.

---

## Build Commands

```bash
# Restore dependencies
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build Parts/Core/Core.csproj

# Build in Release mode
dotnet build -c Release

# Run all tests
dotnet test

# Run a single test by method name (xUnit)
dotnet test --filter "FullyQualifiedName~RenderGraph.Tests.RenderPassTests.RenderPass_Should_Fail_Validation_Without_Name"

# Run tests in specific project
dotnet test Tests/RenderGraph.Tests/

# Run tests with verbose output
dotnet test -v n

# Run tests with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Run only platform-agnostic tests (Linux/Windows)
dotnet test Tests/RenderGraph.Tests/
```

---

## Platform Notes

### Windows
- All tests pass including DirectX 12 implementation tests
- Full graphics API support (DirectX 12, Vulkan)

### Linux
- **DirectX 12 tests will fail** — This is expected and by design
- DirectX 12 requires Windows-only native libraries (d3d12.dll)
- Run Core tests only: `dotnet test Tests/RenderGraph.Tests/`
- Vulkan implementation tests require Vulkan-capable GPU and drivers

---

## Code Style Guidelines

### General

- **Target Framework**: .NET 9.0
- **Implicit Usings**: Enabled
- **Nullable**: Enabled
- **Language Version**: Latest (C# 12 features allowed)
- **Indent**: 2 spaces (no tabs)
- **Line Endings**: CRLF
- **PROHIBITED**: `#region` blocks

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes/Records | PascalCase | `RenderGraph`, `ResourceHandle` |
| Interfaces | PascalCase with `I` prefix | `IGraphicsDevice` |
| Methods/Properties | PascalCase | `AddPass()`, `IsCompiled` |
| Private Fields | `p_` prefix + camelCase | `p_enabled`, `p_inputs` |
| Private Static Fields | `p_` prefix + camelCase | `p_cachedNodes` |
| Parameters | `_` prefix + camelCase | `_name`, `_device` |
| Constants (private) | PascalCase | `MaxNodeIndex` |
| Public Fields | PascalCase | `Name`, `Enabled` |
| Enums | PascalCase | `PassCategory`, `ResourceLifetime` |

### Member Ordering (per class)

```csharp
class Foo : IDisposable
{
  // 1. Nested types
  class Node {}

  // 2. Private constants
  private const byte MaxNodeIndex = 255;

  // 3. Private static readonly
  private static readonly ConcurrentDictionary<int,bool> CachedNodes = new();

  // 4. Private instance fields
  private bool p_started = false;

  // 5. Static constructor
  static Foo() { }

  // 6. Public static methods
  public static Foo Create() => new(-1);

  // 7. Constructors
  public Foo(int _depth) { }

  // 8. Public properties
  public bool IsWorking => p_started && !p_terminated;

  // 9. Private properties
  private bool IsCorrupted => !p_started && p_terminated;

  // 10. Public methods
  public void Start() => p_started = true;

  // 11. Dispose pattern
  public void Dispose() => Disposing(true);

  // 12. Protected virtual methods
  protected virtual void OnReset() { }

  // 13. Private methods
  private void Reset() => OnReset();
}
```

### Code Formatting

- **Expression-bodied members**: Use where concise (properties, simple methods)
- **Braces**: Required for `for`, `while`; optional for single-line `if`
- **Object initializers**: Prefer on same line when brief
- **Var**: Use everywhere (`csharp_style_var_elsewhere = true`)
- **File-scoped namespaces**: Preferred (`namespace X.Y;`)
- **This qualifier**: Optional (hints available)
- **Parentheses**: Avoid redundant in arithmetic/relational operators

### Error Handling

- Use `ArgumentNullException.ThrowIfNull()` for null checks
- Throw specific exceptions with meaningful messages
- Document exceptions in XML comments for public APIs

### Documentation

- XML documentation comments for all public APIs (summary, params, returns)
- Russian-language comments in Russian-language codebases (see existing code)
- Keep comments current with code changes

### Architecture Notes

- **GraphicsAPI**: Abstraction layer for DirectX 12, Vulkan backends
- **Core**: RenderGraph, RenderPass, ResourceManager, DependencyResolver
- **Passes**: Built-in render passes (Geometry, Blur, PostProcessing, etc.)
- **Tests**: xUnit with internal visibility for testing
