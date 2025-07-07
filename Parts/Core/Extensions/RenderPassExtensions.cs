using Core.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions;
public static class RenderPassExtensions
{
  public static bool IsPostProcessPass(this RenderPass _pass)
  {
    return _pass.Category == PassCategory.PostProcessing;
  }

  public static bool IsRenderingPass(this RenderPass _pass)
  {
    return _pass.Category == PassCategory.Rendering;
  }

  public static bool IsHighPriority(this RenderPass _pass)
  {
    return _pass.Priority >= PassPriority.High;
  }

  public static void SetCategory(this RenderPass _pass, PassCategory _category)
  {
    _pass.Category = _category;
  }

  public static void SetPriority(this RenderPass _pass, PassPriority _priority)
  {
    _pass.Priority = _priority;
  }

  public static void RequiresPass(this RenderPass _pass, RenderPass _dependency)
  {
    _pass.AddDependency(_dependency);
  }

  public static void MakeOptional(this RenderPass _pass)
  {
    _pass.AlwaysExecute = false;
  }

  public static void MakeRequired(this RenderPass _pass)
  {
    _pass.AlwaysExecute = true;
  }
}
