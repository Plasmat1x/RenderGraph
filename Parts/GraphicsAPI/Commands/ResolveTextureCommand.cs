using GraphicsAPI.Commands.enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

namespace GraphicsAPI.Commands;

public record ResolveTextureCommand(ITexture Src, uint SrcArray, ITexture Dst, uint DstArray, TextureFormat Format): ICommand
{
  public CommandType Type => CommandType.ResolveTexture;
  public int SizeInBytes => sizeof(long) * 2 + sizeof(uint) * 2 + sizeof(int);
}
