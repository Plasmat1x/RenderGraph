using Silk.NET.DXGI;

namespace Directx12Impl.Parts;

/// <summary>
/// Данные мип-уровня для пакетной загрузки
/// </summary>
public unsafe struct MipLevelData
{
  public void* Data;
  public ulong DataSize;
  public uint Width, Height, Depth;
  public uint RowPitch;
  public ulong SlicePitch;
  public uint ArraySize;
  public Format Format;
}