using GraphicsAPI.Descriptions;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace MockImpl;

public class MockTexture: ITexture
{
  private readonly Dictionary<TextureViewType, ITextureView> p_defaultViews = new();

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

  public ITextureView CreateView(TextureViewDescription _description)
  {
    Console.WriteLine($"    [Resource] Creating texture view for {Name} ({_description.ViewType})");
    return new MockTextureView(this, _description);
  }

  public ITextureView GetDefaultShaderResourceView()
  {
    if(!p_defaultViews.ContainsKey(TextureViewType.ShaderResource))
    {
      var desc = new TextureViewDescription
      {
        ViewType = TextureViewType.ShaderResource,
        Format = Format,
        MipLevels = MipLevels,
        ArraySize = ArraySize
      };
      p_defaultViews[TextureViewType.ShaderResource] = CreateView(desc);
    }
    return p_defaultViews[TextureViewType.ShaderResource];
  }

  public ITextureView GetDefaultRenderTargetView()
  {
    if(!p_defaultViews.ContainsKey(TextureViewType.RenderTarget))
    {
      var desc = new TextureViewDescription
      {
        ViewType = TextureViewType.RenderTarget,
        Format = Format,
        MipLevels = 1,
        ArraySize = ArraySize
      };
      p_defaultViews[TextureViewType.RenderTarget] = CreateView(desc);
    }
    return p_defaultViews[TextureViewType.RenderTarget];
  }

  public ITextureView GetDefaultDepthStencilView()
  {
    if(!p_defaultViews.ContainsKey(TextureViewType.DepthStencil))
    {
      var desc = new TextureViewDescription
      {
        ViewType = TextureViewType.DepthStencil,
        Format = Format,
        MipLevels = 1,
        ArraySize = ArraySize
      };
      p_defaultViews[TextureViewType.DepthStencil] = CreateView(desc);
    }
    return p_defaultViews[TextureViewType.DepthStencil];
  }

  public ITextureView GetDefaultUnorderedAccessView()
  {
    if(!p_defaultViews.ContainsKey(TextureViewType.UnorderedAccess))
    {
      var desc = new TextureViewDescription
      {
        ViewType = TextureViewType.UnorderedAccess,
        Format = Format,
        MipLevels = 1,
        ArraySize = ArraySize
      };
      p_defaultViews[TextureViewType.UnorderedAccess] = CreateView(desc);
    }
    return p_defaultViews[TextureViewType.UnorderedAccess];
  }

  public void SetData<T>(T[] _data, uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
  {
    Console.WriteLine($"    [Resource] Setting data for texture {Name} (mip: {_mipLevel}, slice: {_arraySlice}, {_data.Length} elements)");
  }

  public T[] GetData<T>(uint _mipLevel = 0, uint _arraySlice = 0) where T : unmanaged
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
    foreach(var view in p_defaultViews.Values)
    {
      view?.Dispose();
    }
    p_defaultViews.Clear();
    IsDisposed = true;
  }
}
