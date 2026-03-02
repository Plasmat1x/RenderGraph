# RenderGraph - Документация

## Статус
- **Agent**: @writer
- **Task**: Создание PlantUML диаграмм классов
- **Status**: ✅ Завершено

---

## Созданные диаграммы

### 1. doc/graphicsapi.puml (8 KB)
**Интерфейсы GraphicsAPI:**
- IResource - базовый интерфейс всех GPU ресурсов
- ITexture / IBuffer - текстуры и буферы
- ITextureView / IBufferView - представления ресурсов
- IShader / ISampler - шейдеры и сэмплеры
- IRenderState / IBlendState / IDepthStencilState / IRasterizerState - состояния рендеринга
- IFence / IQuery - синхронизация и запросы
- ISwapChain / IMonitor - цепочка буферов и мониторы
- IBatchUploader - пакетная загрузка
- IGraphicsDevice - главное устройство

### 2. doc/core.puml (13 KB)
**Классы Core:**
- RenderGraph - основной класс графа рендеринга
- RenderPass - абстрактный базовый класс прохода
- RenderGraphBuilder - построитель графа
- ResourceManager - управление ресурсами
- DependencyResolver - разрешение зависимостей
- RenderPassContext - контекст выполнения
- FrameData - данные кадра
- ResourceHandle - хендл ресурса
- ResourceBarrier - барьер ресурса
- ResourceUsageInfo - информация об использовании

### 3. doc/resources.puml (4 KB)
**Описания ресурсов:**
- ResourceDescription - базовый класс описания
- TextureDescription - описание текстуры
- BufferDescription - описание буфера

### 4. doc/architecture.puml (9 KB)
**Общая архитектура:**
- 5 слоёв: GraphicsAPI → Core → Resources → Enums → Implementations
- DirectX12 и Vulkan реализации
- Все зависимости между слоями

---

## Файлы диаграмм

| Файл | Размер | Описание |
|------|--------|----------|
| graphicsapi.puml | 8 KB | Интерфейсы GraphicsAPI |
| core.puml | 13 KB | Классы Core |
| resources.puml | 4 KB | Описания ресурсов |
| architecture.puml | 9 KB | Общая архитектура |

---

## ✅ Диаграммы классов созданы