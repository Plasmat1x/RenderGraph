using GraphicsAPI.Interfaces;
using GraphicsAPI.Enums;

namespace VulkanImpl;

public class VKBatchUploader : IBatchUploader
{
  public VKBatchUploader()
  {
  }

  public void UploadBuffer(IBuffer _buffer, ulong _offset, ulong _size, IntPtr _data)
  {
    Console.WriteLine($"  [Vulkan] UploadBuffer");
  }

  public void UploadTexture(ITexture _texture, uint _mipLevel, uint _arraySlice, IntPtr _data, ulong _rowPitch, ulong _slicePitch)
  {
    Console.WriteLine($"  [Vulkan] UploadTexture");
  }

  public void AddBufferUpload(IBuffer _buffer, ulong _offset, ulong _size, IntPtr _data)
  {
    Console.WriteLine($"  [Vulkan] AddBufferUpload");
  }

  public void AddTextureUpload(ITexture _texture, uint _mipLevel, uint _arraySlice, IntPtr _data, ulong _rowPitch, ulong _slicePitch)
  {
    Console.WriteLine($"  [Vulkan] AddTextureUpload");
  }

  public void UploadBuffer<T>(IBuffer _buffer, T[] _data, ulong _offset = 0) where T : unmanaged
  {
    Console.WriteLine($"  [Vulkan] UploadBuffer<T>");
  }

  public void UploadTexture<T>(ITexture _texture, T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    Console.WriteLine($"  [Vulkan] UploadTexture<T>");
  }

  public void Flush()
  {
    Console.WriteLine($"  [Vulkan] Flush uploads");
  }

  public void Dispose()
  {
    Console.WriteLine($"  [Vulkan] Dispose batch uploader");
  }
}
