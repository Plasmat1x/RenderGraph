using GraphicsAPI.Commands.enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI.Commands.utils;

/// <summary>
/// Утилиты для работы с командами
/// </summary>
public static class CommandUtils
{
  /// <summary>
  /// Проверить, является ли команда state-changing
  /// </summary>
  public static bool IsStateCommand(ICommand _command)
  {
    return _command.Type switch
    {
      CommandType.SetRenderTargets or
      CommandType.SetViewport or CommandType.SetViewports or
      CommandType.SetScissorRect or CommandType.SetScissorRects or
      CommandType.SetVertexBuffer or CommandType.SetVertexBuffers or
      CommandType.SetIndexBuffer or
      CommandType.SetShader or
      CommandType.SetShaderResource or CommandType.SetShaderResources or
      CommandType.SetUnorderedAccess or CommandType.SetUnorderedAccesses or
      CommandType.SetConstantBuffer or CommandType.SetConstantBuffers or
      CommandType.SetSampler or CommandType.SetSamplers or
      CommandType.SetRenderState or CommandType.SetBlendState or
      CommandType.SetDepthStencilState or CommandType.SetRasterizerState or
      CommandType.SetPrimitiveTopology => true,
      _ => false
    };
  }

  /// <summary>
  /// Проверить, является ли команда draw command
  /// </summary>
  public static bool IsDrawCommand(ICommand _command)
  {
    return _command.Type switch
    {
      CommandType.Draw or CommandType.DrawIndexed or
      CommandType.DrawIndirect or CommandType.DrawIndexedIndirect => true,
      _ => false
    };
  }

  /// <summary>
  /// Проверить, является ли команда compute command
  /// </summary>
  public static bool IsComputeCommand(ICommand _command)
  {
    return _command.Type switch
    {
      CommandType.Dispatch or CommandType.DispatchIndirect => true,
      _ => false
    };
  }

  /// <summary>
  /// Проверить, является ли команда copy command
  /// </summary>
  public static bool IsCopyCommand(ICommand _command)
  {
    return _command.Type switch
    {
      CommandType.CopyTexture or CommandType.CopyTextureRegion or
      CommandType.CopyBuffer or CommandType.CopyBufferRegion or
      CommandType.ResolveTexture => true,
      _ => false
    };
  }

  /// <summary>
  /// Получить категорию команды
  /// </summary>
  public static CommandCategory GetCommandCategory(ICommand _command)
  {
    if(IsStateCommand(_command))
      return CommandCategory.State;
    if(IsDrawCommand(_command))
      return CommandCategory.Draw;
    if(IsComputeCommand(_command))
      return CommandCategory.Compute;
    if(IsCopyCommand(_command))
      return CommandCategory.Copy;

    return _command.Type switch
    {
      CommandType.ClearRenderTarget or CommandType.ClearDepthStencil or
      CommandType.ClearUnorderedAccess => CommandCategory.Clear,
      CommandType.TransitionResource or CommandType.TransitionResources or
      CommandType.UAVBarrier or CommandType.UAVBarriers => CommandCategory.Synchronization,
      CommandType.BeginQuery or CommandType.EndQuery => CommandCategory.Query,
      CommandType.PushDebugGroup or CommandType.PopDebugGroup or
      CommandType.InsertDebugMarker => CommandCategory.Debug,
      _ => CommandCategory.Other
    };
  }
}