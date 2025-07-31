using GraphicsAPI;
using GraphicsAPI.Commands;
using GraphicsAPI.Commands.utils;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using MockImpl;

using System.Numerics;

/// <summary>
/// Пример создания специализированного командного буфера для конкретных задач
/// </summary>
public class RenderPassCommandBuffer: GraphicsAPI.GenericCommandBuffer
{
  private readonly string p_passName;
  private DateTime p_startTime;

  public RenderPassCommandBuffer(string _passName, CommandBufferType _type) : base(_type)
  {
    p_passName = _passName;
  }

  protected override void OnBegin()
  {
    base.OnBegin();
    p_startTime = DateTime.Now;
    Console.WriteLine($"[{p_passName}] Starting render pass...");
  }

  protected override void OnEnd()
  {
    var duration = DateTime.Now - p_startTime;
    var stats = GetStats();
    Console.WriteLine($"[{p_passName}] Pass completed in {duration.TotalMilliseconds:F2}ms");
    Console.WriteLine($"[{p_passName}] {stats}");

    base.OnEnd();
  }

  protected override void ExecuteCommand(ICommand _command)
  {
    // Добавляем префикс к выводу
    Console.WriteLine($"[{p_passName}] Executing: {_command.Type}");

    // Можно добавить специфичную для прохода логику
    if(CommandUtils.IsDrawCommand(_command))
    {
      // Логирование draw calls
      Console.WriteLine($"[{p_passName}] Draw call executed");
    }
  }

  /// <summary>
  /// Специальный метод для настройки типичного geometry pass
  /// </summary>
  public void SetupGeometryPass(ITextureView _colorTarget, ITextureView _depthTarget)
  {
    SetRenderTarget(_colorTarget, _depthTarget);

    if(_colorTarget is MockTextureView mockColor)
    {
      SetViewportFullTexture(mockColor.Texture);
    }

    ClearRenderTarget(_colorTarget, new Vector4(0.1f, 0.1f, 0.2f, 1.0f)); // Темно-синий
    ClearDepthStencil(_depthTarget);

    SetPrimitiveTopology(PrimitiveTopology.TriangleList);
  }
}