# ADR - Architectural Decision Records

## Renderer Backend Architecture

### ADR-001: Graphics API Abstraction Layer

**Status:** Accepted  
**Date:** 2026-02-27

#### Context
RenderGraph должен поддерживать несколько графических API (DX12, Vulkan, Metal) с единым интерфейсом. Каждый API имеет свои особенности и типы данных.

#### Decision
Создать промежуточный слой абстракции:

```
Application → RenderGraph Core → GraphicsAPI Interfaces → Backend Implementation
                                                      ↓
                              Resources.Enums (абстрактные enum'ы)
                                                      ↓
                              Backend Extensions (маппинг в нативные типы)
                                                      ↓
                              Silk.NET (DX12/Vulkan/Metal bindings)
```

#### Consequences

| Positive | Negative |
|----------|----------|
| Единый API для всех бекендов | Дополнительный слой маппинга |
| Легкая замена бекенда | Возможны потери в производительности |
| Тестируемость через Mock | Сложность поддержки 22+ extension files |

---

### ADR-002: Extension Methods для Enum Mapping

**Status:** Accepted  
**Date:** 2026-02-27

#### Context
GraphicsAPI определяет абстрактные enum'ы (TextureFormat, BufferUsage и т.д.), которые должны маппиться в нативные типы конкретного API.

#### Decision
Использовать C# extension methods в отдельных файлах:

```
Parts/[Backend]/Extensions/
├── TextureFormatExtensions.cs    # TextureFormat → Silk.NET.Format
├── FormatExtensions.cs           # Silk.NET.Format → TextureFormat
├── ResourceUsageExtensions.cs    # BufferUsage → D3D12_RESOURCE_FLAGS
├── ResourceStateExtensions.cs    # ResourceState → D3D12_RESOURCE_STATES
├── BlendStateExtensions.cs       # BlendDescription → D3D12_BLEND_DESC
└── ... (22 файла для DX12)
```

#### Implementation Pattern (DX12)

```csharp
// TextureFormat → Format (Silk.NET)
public static Format ToDX12(this TextureFormat format) => format switch
{
    TextureFormat.R8G8B8A8_UNORM => Format.FormatR8G8B8A8Unorm,
    TextureFormat.R32G32B32A32_FLOAT => Format.FormatR32G32B32A32Float,
    // ... 80+ форматов
    _ => Format.FormatUnknown
};

// Format → TextureFormat (обратный маппинг)
public static TextureFormat ToGraphicsAPI(this Format format) => format switch
{
    Format.FormatR8G8B8A8Unorm => TextureFormat.R8G8B8A8_UNORM,
    // ...
};
```

#### Consequences

| Positive | Negative |
|----------|----------|
| Type-safe маппинг | Дублирование enum значений |
| IntelliSense support | Сложность синхронизации |
| Compile-time проверки | 22 файла для поддержки |

---

### ADR-003: Resource Base Class Pattern

**Status:** Accepted  
**Date:** 2026-02-27

#### Context
Все GPU ресурсы (Buffer, Texture, Shader) имеют общие операции: создание, удаление, отладка имена.

#### Decision
Использовать базовый класс с inheritance:

```
DX12Resource (abstract)
├── DX12Buffer
├── DX12Texture
├── DX12Shader
├── DX12Sampler
└── ...

VKResource (abstract)
├── VKBuffer
├── VKTexture
├── VKShader
└── ...
```

#### Implementation Pattern

```csharp
public abstract class DX12Resource : IResource
{
    protected ID3D12Resource* p_resource;
    protected string p_name;
    
    public virtual void SetName(string name) { /* D3D12 SetName */ }
    public virtual void Dispose() { /* Release COM */ }
    public abstract IntPtr GetNativeHandle();
}
```

---

### ADR-004: View Caching с Dictionary

**Status:** Accepted  
**Date:** 2026-02-27

#### Context
BufferView и TextureView могут создаваться с разными параметрами. Повторное создание того же view неэффективно.

#### Decision
Кэшировать view в Dictionary с ключом:

```csharp
// DX12 implementation
public readonly struct BufferViewKey : IEquatable<BufferViewKey>
{
    public readonly BufferViewType ViewType;
    public readonly ulong FirstElement;
    public readonly uint NumElements;
    public readonly uint StructureByteStride;
    
    // Equals, GetHashCode implementation
}

public class DX12Buffer
{
    private readonly Dictionary<BufferViewKey, DX12BufferView> p_views = new();
    
    public IBufferView CreateView(BufferViewDescription description)
    {
        var key = new BufferViewKey(description);
        if (!p_views.TryGetValue(key, out var view))
        {
            view = new DX12BufferView(this, description);
            p_views[key] = view;
        }
        return view;
    }
}
```

---

### ADR-005: Memory Management Strategy

**Status:** Accepted  
**Date:** 2026-02-27

#### Context
Разные типы ресурсов требуют разного размещения в памяти GPU.

#### Decision
Использовать стратегию на основе ResourceUsage:

| Usage | Memory Type | Mapping Strategy |
|-------|-------------|------------------|
| Dynamic | HostVisible + HostCoherent | Map directly (vkMapMemory) |
| Default | DeviceLocal | Staging buffer for upload |
| Staging | HostVisible + HostCached | Readback buffer |
| Immutable | DeviceLocal | One-time upload |

#### Implementation Pattern

```csharp
public MemoryRequirements GetMemoryRequirements(BufferDescription desc)
{
    var usageFlags = desc.Usage switch
    {
        BufferUsage.Dynamic => MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
        BufferUsage.Default => MemoryPropertyFlags.DeviceLocalBit,
        BufferUsage.Staging => MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCachedBit,
        _ => MemoryPropertyFlags.DeviceLocalBit
    };
    
    // Запрос через vkGetBufferMemoryRequirements
}
```

---

### ADR-006: Command Buffer Abstraction

**Status:** Accepted  
**Date:** 2026-02-27

#### Context
CommandBuffer должен поддерживать множество API с разными моделями: DX12 - CommandList, Vulkan - CommandBuffer.

#### Decision
Использовать GenericCommandBuffer как базовый класс:

```
GenericCommandBuffer (abstract)
├── DX12CommandBuffer
└── VKCommandBuffer
```

#### Implementation Pattern

```csharp
public abstract class GenericCommandBuffer
{
    protected CommandBufferType p_type;
    protected ITextureView[] p_currentRenderTargets;
    protected ITextureView p_currentDepthTarget;
    // ... общие поля
    
    public abstract void SetRenderTargets(ITextureView[] colorTargets, ITextureView depthTarget);
    public abstract void SetShader(IShader shader);
    public abstract void Draw(...);
}
```

---

### ADR-007: Pipeline State Building

**Status:** Accepted  
**Date:** 2026-02-27

#### Context
Графический пайплайн строится из декларативного описания. DX12 и Vulkan имеют разные API для создания пайплайнов.

#### Decision
Использовать Builder pattern с бэкенд-специфичной реализацией:

```
RenderStateDescription (абстрактное описание)
    ↓
DX12PipelineStateBuilder → ID3D12PipelineState
VKPipelineStateBuilder → VkPipeline
```

#### Implementation Pattern

```csharp
public class DX12PipelineStateBuilder
{
    private readonly D3D12 p_d3d12;
    private readonly ID3D12Device* p_device;
    private GraphicsPipelineStateDescription p_desc;
    
    public DX12PipelineStateBuilder WithVertexShader(byte[] bytecode) { ... }
    public DX12PipelineStateBuilder WithPixelShader(byte[] bytecode) { ... }
    public DX12PipelineStateBuilder WithBlendState(...) { ... }
    public DX12PipelineStateBuilder WithRasterizerState(...) { ... }
    
    public ID3D12PipelineState* Build() { ... }
}
```

---

### ADR-008: Factory Pattern для Backend Creation

**Status:** Accepted  
**Date:** 2026-02-27

#### Context
REQUIREMENTS.md требует runtime backend selection с конфигурацией.

#### Decision
Factory interface с конфигурацией:

```csharp
public interface IGraphicsDeviceFactory
{
    IGraphicsDevice CreateDevice(RenderGraphConfig config);
    IReadOnlyList<IGraphicsAdapter> GetAvailableAdapters();
    bool IsBackendAvailable(string backend);
}

public class RenderGraphConfig
{
    public string Backend { get; set; } = "Auto"; // DX12, Vulkan, Auto
    public uint PreferredAdapter { get; set; } = 0;
    public bool DebugMode { get; set; } = false;
}
```

---

## Summary

| ADR | Pattern | Применимость |
|-----|---------|--------------|
| 001 | Abstraction Layer | Все бекенды |
| 002 | Extension Methods | DX12 ✅, Vulkan ❌, Mock ✅ |
| 003 | Base Resource Class | DX12 ✅, VK ✅, Mock ✅ |
| 004 | View Caching | DX12 ✅, VK ❌ |
| 005 | Memory Strategy | DX12 ✅, VK ❌ |
| 006 | Generic CommandBuffer | DX12 ✅, VK ✅, Mock ✅ |
| 007 | Pipeline Builder | DX12 ✅, VK ❌ |
| 008 | Factory Pattern | Все бекенды |
