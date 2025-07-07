using Resources.Enums;
using Resources.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources;
public class TextureDescription : IResourceDescription
{
  public uint Width { get; set; }
  public uint Height { get; set; }
  public uint Depth { get; set; }
  public uint MipLevels { get; set; }
  public uint ArraySize { get; set; }
  public uint SampleCount { get; set; }
  public uint SampleQuality { get; set; }
  public TextureFormat Format { get; set; }
  public TextureUsage Usage { get; set; }
  public BindFlags BindFlags { get; set; }

  public string Name { get; set; }

  ResourceUsage IResourceDescription.Usage { get; }

  public CPUAccessFlags CPUAcessFalgs { get; set; }

  public ResourceMiscFlags ResourceMiscFlags { get; set; }

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public bool IsCompatible(TextureDescription _other)
  {
    throw new NotImplementedException();
  }

  public List<TextureDescription> CreateMipChain()
  {
    throw new NotImplementedException();
  }

  public bool IsComaptible(IResourceDescription _other)
  {
    throw new NotImplementedException();
  }
}
