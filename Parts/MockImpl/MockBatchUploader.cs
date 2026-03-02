using GraphicsAPI.Interfaces;

using Resources;

namespace MockImpl;

public class MockBatchUploader : IBatchUploader
{
  private readonly List<(IBuffer buffer, ulong offset, ulong size, IntPtr data)> p_bufferUploads = [];
  private readonly List<(ITexture texture, uint mipLevel, uint arraySlice, IntPtr data, ulong rowPitch, ulong slicePitch)> p_textureUploads = [];

  public void UploadBuffer<T>(IBuffer _buffer, T[] _data, ulong _offset = 0) where T : unmanaged
  {
    Console.WriteLine($"  [MockUploader] Upload buffer {_buffer.Name} at offset {_offset}, {_data.Length} elements");
  }

  public void UploadTexture<T>(ITexture _texture, T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    Console.WriteLine($"  [MockUploader] Upload texture {_texture.Name} mip {_mipLevel}, slice {_arraySlice}, {_data.Length} elements");
  }

  public void AddBufferUpload(IBuffer _buffer, ulong _offset, ulong _size, IntPtr _data)
  {
    Console.WriteLine($"  [MockUploader] Add buffer upload {_buffer.Name} at offset {_offset}, size {_size}");
    p_bufferUploads.Add((_buffer, _offset, _size, _data));
  }

  public void AddTextureUpload(ITexture _texture, uint _mipLevel, uint _arraySlice, IntPtr _data, ulong _rowPitch, ulong _slicePitch)
  {
    Console.WriteLine($"  [MockUploader] Add texture upload {_texture.Name} mip {_mipLevel}, slice {_arraySlice}, rowPitch {_rowPitch}, slicePitch {_slicePitch}");
    p_textureUploads.Add((_texture, _mipLevel, _arraySlice, _data, _rowPitch, _slicePitch));
  }

  public void Flush()
  {
    Console.WriteLine($"  [MockUploader] Flush {p_bufferUploads.Count} buffer uploads and {p_textureUploads.Count} texture uploads");
    p_bufferUploads.Clear();
    p_textureUploads.Clear();
  }
}
