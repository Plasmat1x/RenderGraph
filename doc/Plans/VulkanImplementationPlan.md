# Vulkan Backend Implementation Plan

**Target:** parity with DX12 implementation  
**Priority:** Critical (REQUIREMENTS.md Section 3.2)  
**Platform:** Linux (primary), Windows (secondary)

---

## Phase 0: Foundation (Week 1)

### Goal
Исправить критические ошибки компиляции и создать базовую инфраструктуру.

### Tasks

| # | Task | Files | Hours |
|---|------|-------|-------|
| 0.1 | Создать директорию Extensions | - | 0.5 |
| 0.2 | Создать TextureFormatExtensions (TextureFormat → Format) | VulkanImpl/Extensions/TextureFormatExtensions.cs | 3 |
| 0.3 | Создать FormatExtensions (Format → TextureFormat) | VulkanImpl/Extensions/FormatExtensions.cs | 3 |
| 0.4 | Создать MemoryPropertyExtensions (ResourceUsage → MemoryPropertyFlags) | VulkanImpl/Extensions/MemoryPropertyExtensions.cs | 2 |
| 0.5 | Создать QueueFamilyExtensions | VulkanImpl/Extensions/QueueFamilyExtensions.cs | 1 |
| 0.6 | Создать ImageLayoutExtensions (ResourceState → ImageLayout) | VulkanImpl/Extensions/ImageLayoutExtensions.cs | 2 |
| 0.7 | Создать PipelineStageExtensions | VulkanImpl/Extensions/PipelineStageExtensions.cs | 1 |
| 0.8 | Создать AccessMaskExtensions | VulkanImpl/Extensions/AccessMaskExtensions.cs | 1 |
| 0.9 | Создать CommandBufferLevelExtensions | VulkanImpl/Extensions/CommandBufferLevelExtensions.cs | 0.5 |
| 0.10 | Создать PrimitiveTopologyExtensions | VulkanImpl/Extensions/PrimitiveTopologyExtensions.cs | 1 |
| 0.11 | Создать AddressModeExtensions | VulkanImpl/Extensions/AddressModeExtensions.cs | 0.5 |
| 0.12 | Создать FilterExtensions | VulkanImpl/Extensions/FilterExtensions.cs | 0.5 |

**Subtotal:** ~16 hours

---

## Phase 1: Core Resources (Week 1-2)

### Goal
Реализовать базовые ресурсы: Buffer, Texture, Sampler.

### Tasks

| # | Task | Reference | Hours |
|---|------|-----------|-------|
| 1.1 | VKResource - base class | DX12Resource | 2 |
| 1.2 | VKBuffer - memory management | DX12Buffer | 8 |
| 1.3 | VKBufferView - view creation | DX12BufferView | 4 |
| 1.4 | VKTexture - image creation | DX12Texture | 12 |
| 1.5 | VKTextureView - image views | DX12TextureView | 6 |
| 1.6 | VKSampler - sampler creation | DX12Sampler | 4 |
| 1.7 | VKFence - timeline semaphores | DX12Fence | 3 |

**Subtotal:** ~39 hours

---

## Phase 2: Command Buffer (Week 2-3)

### Goal
Реализовать запись и управление командным буфером.

### Tasks

| # | Task | Reference | Hours |
|---|------|-----------|-------|
| 2.1 | VKCommandBuffer - base recording | DX12CommandBuffer | 16 |
| 2.2 | VKCommandBuffer - state tracking | VKRenderState | 8 |
| 2.3 | VKBatchUploader - data upload | DX12BatchUploader | 6 |

**Subtotal:** ~30 hours

---

## Phase 3: Graphics Device (Week 3)

### Goal
Завершить VKGraphicsDevice с всеми методами.

### Tasks

| # | Task | Reference | Hours |
|---|------|-----------|-------|
| 3.1 | Instance/Device creation | Существующий код | 4 |
| 3.2 | Queue family setup | - | 4 |
| 3.3 | Memory allocator (VmaAllocator) | REQUIRED | 8 |
| 3.4 | Pipeline cache | REQUIRED | 4 |
| 3.5 | Debug utils (VK_EXT_debug_utils) | REQUIRED | 4 |
| 3.6 | Format support queries | DX12GraphicsDevice | 4 |

**Subtotal:** ~28 hours

---

## Phase 4: Pipeline State (Week 3-4)

### Goal
Реализовать создание графических пайплайнов.

### Tasks

| # | Task | Reference | Hours |
|---|------|-----------|-------|
| 4.1 | VKRenderState - pipeline creation | DX12RenderState | 12 |
| 4.2 | VKShader - SPIR-V compilation | DX12Shader | 8 |
| 4.3 | Descriptor set layout | - | 6 |
| 4.4 | Pipeline layout | - | 4 |

**Subtotal:** ~30 hours

---

## Phase 5: Swap Chain & Present (Week 4)

### Goal
Реализовать вывод изображения на экран.

### Tasks

| # | Task | Reference | Hours |
|---|------|-----------|-------|
| 5.1 | VKSwapChain - surface creation | DX12SwapChain | 8 |
| 5.2 | Present logic | Существующий код | 4 |
| 5.3 | Resize handling | DX12SwapChain | 4 |
| 5.4 | Window integration (XCB/Wayland) | REQUIRED | 8 |

**Subtotal:** ~24 hours

---

## Phase 6: Testing & Integration (Week 5)

### Goal
Протестировать и интегрировать с RenderGraph.

### Tasks

| # | Task | Hours |
|---|------|-------|
| 6.1 | Unit tests for VKBuffer | 4 |
| 6.2 | Unit tests for VKTexture | 4 |
| 6.3 | Integration test: triangle render | 8 |
| 6.4 | Integration test: texture sampling | 4 |
| 6.5 | Integration test: MRT | 4 |
| 6.6 | Performance profiling | 4 |

**Subtotal:** ~28 hours

---

## Summary

| Phase | Hours | Deliverable |
|-------|-------|-------------|
| Phase 0: Foundation | 16 | Extension methods (12 files) |
| Phase 1: Core Resources | 39 | Buffer, Texture, Sampler |
| Phase 2: Command Buffer | 30 | Command recording |
| Phase 3: Graphics Device | 28 | VKGraphicsDevice |
| Phase 4: Pipeline State | 30 | Render pipeline |
| Phase 5: Swap Chain | 24 | Present to screen |
| Phase 6: Testing | 28 | Working render |
| **Total** | **~195** | **Functional Vulkan backend** |

---

## Priorities from REQUIREMENTS.md

### Critical (Must Have)
- [ ] VKGraphicsDevice все 20+ методов
- [ ] VKCommandBuffer все методы записи
- [ ] VKBufferGPU memory management
- [ ] VKTexture image creation
- [ ] VKShader compilation
- [ ] VKSwapChain present

### High (Should Have)
- [ ] VKSampler
- [ ] VKFence timeline semaphores
- [ ] VKBufferView
- [ ] VKTextureView

### Medium (Nice to Have)
- [ ] VKMonitor enumeration
- [ ] Pipeline cache
- [ ] VmaAllocator
- [ ] Debug utils

---

## Key Dependencies

```
MockImpl (reference)
    ↓
DX12Impl (implementation pattern)
    ↓
VulkanImpl (target)
```

---

## Notes

1. **MockImpl используется как спецификация интерфейсов** - все методы IGraphicsDevice должны быть реализованы
2. **DX12Impl используется как reference implementation** - копировать паттерны маппинга, builders, view caching
3. **Extension methods критичны** - без них 489+ ошибок компиляции
4. **Vulkan 1.3+ required** - использовать dynamic rendering где возможно
