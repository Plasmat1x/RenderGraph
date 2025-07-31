using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace MockImpl;

public class MockTexture: ITexture
{
  private readonly Dictionary<TextureViewType, ITextureView> _defaultViews = new();

  public MockTexture(uint _id, TextureDescription _description)
  {
    Id = _id;
    Description = _description;
    Name = _description.Name;
  }

  public uint Id { get; }
  public TextureDescription Description { get; }
  public string Name { get; set; }
  public ResourceType ResourceType => ResourceType.Texture2D;
  public bool IsDisposed { get; private set; }

  public uint Width => Description.Width;
  public uint Height => Description.Height;
  public uint Depth => Description.Depth;
  public uint MipLevels => Description.MipLevels;
  public uint ArraySize => Description.ArraySize;
  public TextureFormat Format => Description.Format;
  public uint SampleCount => Description.SampleCount;

  public ITextureView CreateView(_TextureViewDescription _description)
  {
    Console.WriteLine($"    [Resource] Creating texture view for {Name} ({_description.ViewType})");
    return new MockTextureView(this, _description);
  }

  public ITextureView GetDefaultShaderResourceView()
  {
    if(!_defaultViews.ContainsKey(TextureViewType.ShaderResource))
    {
      var desc = new _TextureViewDescription
      {
        ViewType = TextureViewType.ShaderResource,
        Format = Format,
        MipLevels = MipLevels,
        ArraySize = ArraySize
      };
      _defaultViews[TextureViewType.ShaderResource] = CreateView(desc);
    }
    return _defaultViews[TextureViewType.ShaderResource];
  }

  public ITextureView GetDefaultRenderTargetView()
  {
    if(!_defaultViews.ContainsKey(TextureViewType.RenderTarget))
    {
      var desc = new _TextureViewDescription
      {
        ViewType = TextureViewType.RenderTarget,
        Format = Format,
        MipLevels = 1,
        ArraySize = ArraySize
      };
      _defaultViews[TextureViewType.RenderTarget] = CreateView(desc);
    }
    return _defaultViews[TextureViewType.RenderTarget];
  }

  public ITextureView GetDefaultDepthStencilView()
  {
    if(!_defaultViews.ContainsKey(TextureViewType.DepthStencil))
    {
      var desc = new _TextureViewDescription
      {
        ViewType = TextureViewType.DepthStencil,
        Format = Format,
        MipLevels = 1,
        ArraySize = ArraySize
      };
      _defaultViews[TextureViewType.DepthStencil] = CreateView(desc);
    }
    return _defaultViews[TextureViewType.DepthStencil];
  }

  public ITextureView GetDefaultUnorderedAccessView()
  {
    if(!_defaultViews.ContainsKey(TextureViewType.UnorderedAccess))
    {
      var desc = new _TextureViewDescription
      {
        ViewType = TextureViewType.UnorderedAccess,
        Format = Format,
        MipLevels = 1,
        ArraySize = ArraySize
      };
      _defaultViews[TextureViewType.UnorderedAccess] = CreateView(desc);
    }
    return _defaultViews[TextureViewType.UnorderedAccess];
  }

  public void SetData<T>(T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : struct
  {
    Console.WriteLine($"    [Resource] Setting data for texture {Name} (mip: {_mipLevel}, slice: {_arraySlice}, {_data.Length} elements)");
  }

  public T[] GetData<T>(uint _mipLevel = 0, uint _arraySlice = 0) where T : struct
  {
    Console.WriteLine($"    [Resource] Getting data from texture {Name} (mip: {_mipLevel}, slice: {_arraySlice})");
    return new T[Width * Height];
  }

  public uint GetSubresourceIndex(uint _mipLevel, uint _arraySlice)
  {
    return _mipLevel + _arraySlice * MipLevels;
  }

  public void GenerateMips()
  {
    Console.WriteLine($"    [Resource] Generating mips for texture {Name}");
  }

  public IntPtr GetNativeHandle()
  {
    return new IntPtr(Id);
  }

  public ulong GetMemorySize()
  {
    return Description.GetMemorySize();
  }

  public void Dispose()
  {
    if(IsDisposed)
      return;

    Console.WriteLine($"    [Resource] Disposed texture {Name} (ID: {Id})");
    foreach(var view in _defaultViews.Values)
    {
      view?.Dispose();
    }
    _defaultViews.Clear();
    IsDisposed = true;
  }
}
