using GraphicsAPI.Interfaces;

using Resources.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI;
public class TextureViewStub : ITextureView
{
  public ITexture Texture { get; }
  public TextureViewType ViewType { get; }

  public TextureViewStub(ITexture _texture, TextureViewType _viewType)
  {
    Texture = _texture;
    ViewType = _viewType;
  }

  public void Dispose()
  {

  }
}


