Акторы:
→ Developer (один раз при старте сцены)
→ RenderEngine (каждый кадр)

1. Developer:
1.1. Создаёт пустой граф: CreateRenderGraph()
1.2. Объявляет виртуальные ресурсы: DeclareResource()
1.3. Добавляет render-пассы: AddRenderPass()
1.4. Для каждого pass задаёт callback: SetPassCallback()
1.5. При необходимости импортирует внешние ресурсы: ImportExternalResource()

2. RenderEngine (на старте или при изменении графа):
2.1. Вызывает CompileRenderGraph()
2.2. Производится:
- Анализ зависимостей и порядков исполнения
- Анализ времени жизни ресурсов
- Планирование алиасинга
- Выделение физических ресурсов
- Генерация барьеров и переходов состояний

3. RenderEngine (каждый кадр):
3.1. Вызывает ExecuteCompiledGraph()
3.2. Для каждого CompiledPass:
- Выполняется PreparePass() — установка целей и барьеров
- Вызывается PassCallback() — запись команд
- Выполняется SubmitPass() — отправка команд GPU

4. (Опционально)
4.1. Developer экспортирует отладочную информацию: ExportGraphStats()