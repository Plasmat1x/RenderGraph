namespace Resources.Enums;
/// <summary>
/// Расширенные флаги для ресурсов
/// </summary>
[Flags]
public enum ResourceMiscFlags
{
  None = 0,
  GenerateMips = 1 << 0,
  Shared = 1 << 1,
  TextureCube = 1 << 2,
  DrawIndirectArgs = 1 << 3,
  BufferAllowRawViews = 1 << 4,
  BufferStructured = 1 << 5,
  ResourceClamp = 1 << 6,
  SharedKeyedmutex = 1 << 7,
  GDICompatible = 1 << 8,
  SharedNTHandle = 1 << 9,
  RestrictedContent = 1 << 10,
  RestrictSharedResource = 1 << 11,
  RestrictSharedResourceDriver = 1 << 12,
  Guarded = 1 << 13,
  TilePool = 1 << 14,
  Tiled = 1 << 15,
  HWProtected = 1 << 16,
  SharedKeyedMutex = 1 << 17
}
