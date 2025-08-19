using Silk.NET.Direct3D12;

namespace Directx12Impl.Parts;

public unsafe struct TextureUploadBatch
{
  public ID3D12Resource* DestinationTexture;
  public uint Subresource;
  public void* Data;
  public ulong DataSize;
  public uint X, Y, Z;
  public uint Width, Height, Depth;
  public SubresourceFootprint Footprint;
}
