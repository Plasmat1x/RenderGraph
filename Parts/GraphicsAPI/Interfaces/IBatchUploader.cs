namespace GraphicsAPI.Interfaces;

/// <summary>
/// Интерфейс для пакетной загрузки
/// </summary>
public interface IBatchUploader
{
  void UploadBuffer<T>(IBuffer _buffer, T[] _data, ulong _offset = 0) where T : unmanaged;
  void UploadTexture<T>(ITexture _texture, T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged;

  void AddBufferUpload(IBuffer _buffer, ulong _offset, ulong _size, IntPtr _data);
  void AddTextureUpload(ITexture _texture, uint _mipLevel, uint _arraySlice, IntPtr _data, ulong _rowPitch, ulong _slicePitch);
  void Flush();
}
