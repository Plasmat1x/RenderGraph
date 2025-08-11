using GraphicsAPI.Interfaces;

using Silk.NET.Direct3D12;

namespace Directx12Impl;

/// <summary>
/// Реализация пакетной загрузки
/// </summary>
internal unsafe class DX12BatchUploader: IBatchUploader
{
  private readonly DX12GraphicsDevice p_device;
  private readonly ID3D12GraphicsCommandList* p_commandList;

  public DX12BatchUploader(DX12GraphicsDevice _device, ID3D12GraphicsCommandList* _commandList)
  {
    p_device = _device;
    p_commandList = _commandList;
  }

  public void UploadBuffer<T>(IBuffer _buffer, T[] _data, ulong _offset = 0) where T : unmanaged
  {
    if(_buffer is not DX12Buffer dx12Buffer)
      throw new ArgumentException("Buffer must be DX12Buffer");

    fixed(T* pData = _data)
    {
      dx12Buffer.SetDataInternal(p_commandList, pData, (ulong)(_data.Length * sizeof(T)), _offset);
    }
  }

  public void UploadTexture<T>(ITexture _texture, T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    if(_texture is not DX12Texture dx12Texture)
      throw new ArgumentException("Texture must be DX12Texture");

    fixed(T* pData = _data)
    {
      dx12Texture.SetDataInternal(p_commandList, pData, _data.Length * sizeof(T), _mipLevel, _arraySlice);
    }
  }
}
