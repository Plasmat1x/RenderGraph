using GraphicsAPI.Commands;
using GraphicsAPI.Commands.Interfaces;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

/// <summary>
/// Пример реализации CommandBuffer на основе Generic базового класса
/// </summary>
public class ExampleCommandBuffer: GraphicsAPI.GenericCommandBuffer
{
  private readonly IGraphicsDevice p_device;

  public ExampleCommandBuffer(IGraphicsDevice _device, CommandBufferType _type)
    : base(_type)
  {
    p_device = _device ?? throw new ArgumentNullException(nameof(_device));
  }

  protected override void ExecuteCommand(ICommand _command)
  {
    // В реальной реализации здесь был бы код выполнения команды на GPU
    Console.WriteLine($"Executing command: {_command.Type} (Size: {_command.SizeInBytes} bytes)");

    // Пример обработки разных типов команд
    switch(_command)
    {
      case DrawCommand draw:
        ExecuteDrawCommand(draw);
        break;
      case SetShaderCommand shader:
        ExecuteSetShaderCommand(shader);
        break;
      case ClearRenderTargetCommand clear:
        ExecuteClearRenderTargetCommand(clear);
        break;
      // ... остальные команды
      default:
        Console.WriteLine($"  Command details: {_command}");
        break;
    }
  }

  private void ExecuteDrawCommand(DrawCommand _command)
  {
    Console.WriteLine($"  Drawing {_command.VertexCount} vertices, {_command.InstanceCount} instances");

    // Проверка состояния pipeline
    if(!IsGraphicsPipelineValid())
    {
      Console.WriteLine("  WARNING: Graphics pipeline is not valid!");
      return;
    }

    // Здесь был бы реальный draw call
  }

  private void ExecuteSetShaderCommand(SetShaderCommand _command)
  {
    Console.WriteLine($"  Setting {_command.Stage} shader: {_command.Shader?.Name ?? "null"}");
  }

  private void ExecuteClearRenderTargetCommand(ClearRenderTargetCommand _command)
  {
    var color = _command.Color;
    Console.WriteLine($"  Clearing render target with color RGBA({color.X:F2}, {color.Y:F2}, {color.Z:F2}, {color.W:F2})");
  }
}
