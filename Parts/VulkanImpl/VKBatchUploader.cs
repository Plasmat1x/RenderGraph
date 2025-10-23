using GraphicsAPI.Interfaces;

namespace VulkanImpl;

public class VKBatchUploader : IBatchUploader
{
  public void UploadBuffer<T>(IBuffer _buffer, T[] _data, ulong _offset = 0) where T : unmanaged
  {
    throw new NotImplementedException();
  }

  public void UploadTexture<T>(ITexture _texture, T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    throw new NotImplementedException();
  }
}
