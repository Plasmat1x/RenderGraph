---
marp: true
theme: default
class: invert
paginate: true
header: "Рендер Граф: Система управления процессом рендеринга"
footer: "Top Academy 2025"
backgroundColor: #111122
author: Мрясов М.С.

---
# Дипломная работа на тему: 
## Рендер Граф: Система управления процессом рендеринга

Выполнил: студент группы ПД212
Мрясов Михаил Сергеевич
Онлайн филиал Top Academy

---
## Проблема и контекст проекта

**Вызов современной компьютерной графики:**
- Рост сложности rendering pipeline'ов с 3-5 до 15-20+ проходов
- Необходимость глубоких знаний DirectX 12, Vulkan для команд приложений
- Высокий риск ошибок при ручном управлении ресурсами и синхронизацией

**Контекст проекта:**
- Компания разрабатывает средства визуализации рекламно-информационных материалов
- Команда разработчиков без глубокой экспертизы
- Неопределенные требования: от базового рендеринга до сложных эффектов
- Необходимость гибкого добавления новых визуальных возможностей

---
## Цель:
Создать библиотеку, снижающую порог входа в программирование графики

---
## Что такое рендер граф

**Определение:**
Рендер граф - декларативная система описания rendering pipeline'а в виде направленного ациклического графа (DAG):
- **Узлы** - render pass'ы (geometry, effects, post-processing)
- **Рёбра** - зависимости через ресурсы

---
## Эволюция подходов:
```
1990-2005: Fixed Pipeline
Vertices → Transform → Lighting → Rasterization

2005-2015: Programmable Shaders  
Geometry → Vertex Shader → Pixel Shader → Output

2015-2020: Multiple Passes
Geometry → G-Buffer → Lighting → Effects → Post-Process

2020+: Render Graphs (Frostbite, Unreal Engine)
Декларативное описание + автоматическая оптимизация
```

---
## Пример современного пайплайна
![w:1000 h:1000](./Materials/rendergraph.svg)

---
**Сложность современных систем:**
- Множество взаимосвязанных этапов
- Различные форматы и размеры ресурсов  
- Сложные зависимости между проходами
- Необходимость оптимального управления памятью

---
### Пример без рендер графа

```csharp
ID3D12Resource* vertexBuffer;
D3D12_VERTEX_BUFFER_VIEW vertexBufferView;

device->CreateCommittedResource(&CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD),
    D3D12_HEAP_FLAG_NONE, &CD3DX12_RESOURCE_DESC::Buffer(sizeof(vertices)),
    D3D12_RESOURCE_STATE_GENERIC_READ, nullptr, IID_PPV_ARGS(&vertexBuffer));

UINT8* pVertexDataBegin;
vertexBuffer->Map(0, &readRange, reinterpret_cast<void**>(&pVertexDataBegin));
memcpy(pVertexDataBegin, vertices, sizeof(vertices));
vertexBuffer->Unmap(0, nullptr);

commandList->Reset(commandAllocator, pipelineState);
commandList->SetGraphicsRootSignature(rootSignature);
commandList->RSSetViewports(1, &viewport);
commandList->RSSetScissorRects(1, &scissorRect);

commandList->ResourceBarrier(1, &CD3DX12_RESOURCE_BARRIER::Transition(
    renderTarget, D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET));

commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, nullptr);
commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);
commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
commandList->IASetVertexBuffers(0, 1, &vertexBufferView);
commandList->DrawInstanced(3, 1, 0, 0);

commandList->ResourceBarrier(1, &CD3DX12_RESOURCE_BARRIER::Transition(
    renderTarget, D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_PRESENT));

commandList->Close();
```

---
### Пример с рендер графом
```csharp
public class TrianglePass : RenderPass
{
    public ResourceHandle ColorTarget { get; private set; }
    
    public override void Setup(RenderGraphBuilder builder) 
    {
        ColorTarget = builder.CreateColorTarget("MainColor", 1920, 1080);
        builder.WriteTexture(ColorTarget);
    }
    
    public override void Execute(RenderPassContext context) 
    {
        var cmd = context.CommandBuffer;
        cmd.SetRenderTarget(context.GetTexture(ColorTarget));
        cmd.ClearRenderTarget(Color.Blue);
        cmd.DrawTriangle();
    }
}

using var renderGraph = new RenderGraph(device);
var trianglePass = new TrianglePass();

renderGraph.AddPass(trianglePass);
renderGraph.Compile(); 

while (running) 
{
    using var cmdBuffer = device.CreateCommandBuffer();
    renderGraph.Execute(cmdBuffer);
    device.Present();
}
```

---
**Автоматические гарантии:**
- Корректный порядок выполнения (топологическая сортировка)
- Валидные resource transitions
- Обнаружение циклических зависимостей
- Потенциал для memory aliasing

---
## Архитектура системы
**Трехуровневая архитектура:**
```
┌─────────────────────────────────────────────┐
│              RenderGraph Core               │
│  ┌───────────┐ ┌────────────┐ ┌───────────┐ │
│  │   Graph   │ │ Resource   │ │Dependency │ │
│  │  Manager  │ │  Manager   │ │ Resolver  │ │
│  └───────────┘ └────────────┘ └───────────┘ │
├─────────────────────────────────────────────┤
│               Render Passes                 │
├─────────────────────────────────────────────┤
│            Graphics API Layer               │
└─────────────────────────────────────────────┘
```

---
## Преимущества подхода
**Для разработчиков:**
- **Снижение сложности:** Декларативный API вместо императивного
- **Защита от ошибок:** Автоматическая валидация зависимостей
- **Фокус на логике:** Меньше времени на изучение графических API

**Для архитектуры:**
- **Модульность:** Каждый pass изолирован и переиспользуем
- **Тестируемость:** Pass'ы можно тестировать независимо
- **Расширяемость:** Легко добавлять новые эффекты

---
**Но именно эта инфраструктура:**
- Скрывает сложность от конечного пользователей
- Обеспечивает типо безопасность и валидацию
- Создает основу для новых расширений

---
## Заключение и выводы
**Что реально достигнуто:**
- ✅ Исследована и обоснована концепция render graph системы
- ✅ Спроектирована архитектура с использованием алгоритмов теории графов
- ✅ Реализована основа DirectX 12 рендер (треугольник)

**Извлеченные уроки:**
- Graphics API абстракция сложнее алгоритмов графа в 10+ раз
- Shader cross-compilation - критически важная, но блокирующая задача
- Полностью готовые системы требуют существенно больше усилий, чем ожидалось

---

**Ценность работы:**
- **Для образования:** Глубокое понимание современных rendering архитектур
- **Для проекта:** Прочный фундамент для будущей разработки

**Ключевой вывод:**
Render graph - перспективная концепция, но переход от исследования к реальному использованию требует значительных инженерных усилий.

---
**Ключевой результат:**
Создана система, которая позволяет разработчикам сосредоточиться на создании визуальных эффектов, а не на изучении сложностей современных графических API.

---
**Спасибо за внимание!**