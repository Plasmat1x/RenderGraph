using Resources.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources;
public struct TextureDescription
{
  public uint Width { get; set; }
  public uint Height { get; set; }
  public TextureFormat Format { get; set; }
  public uint MipLevels { get; set; }
  public uint ArraySize { get; set; }
  public TextureUsage Usage { get; set; }
  public BindFlags bindFlags { get; set; }
}
