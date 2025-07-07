using GraphicsAPI;
using GraphicsAPI.Interfaces;

using Resources;
using Resources.Enums;

namespace MockImpl;

public class MockTexture: ITexture
{
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

  private readonly Dictionary<TextureViewType, ITextureView> _defaultViews = new();

  public MockTexture(uint id, TextureDescription description)
  {
    Id = id;
    Description = description;
    Name = description.Name;
  }

  public ITextureView CreateView(TextureViewDescription description)
  {
    Console.WriteLine($"    [Resource] Creating texture view for {Name} ({description.ViewType})");
    return new MockTextureView(this, description);
  }

  public ITextureView GetDefaultShaderResourceView()
  {
    if(!_defaultViews.ContainsKey(TextureViewType.ShaderResource))
    {
      var desc = new TextureViewDescription
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
      var desc = new TextureViewDescription
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
      var desc = new TextureViewDescription
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
      var desc = new TextureViewDescription
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

  public void SetData<T>(T[] data, uint mipLevel = 0, uint arraySlice = 0) where T : struct
  {
    Console.WriteLine($"    [Resource] Setting data for texture {Name} (mip: {mipLevel}, slice: {arraySlice}, {data.Length} elements)");
  }

  public T[] GetData<T>(uint mipLevel = 0, uint arraySlice = 0) where T : struct
  {
    Console.WriteLine($"    [Resource] Getting data from texture {Name} (mip: {mipLevel}, slice: {arraySlice})");
    return new T[Width * Height];
  }

  public uint GetSubresourceIndex(uint mipLevel, uint arraySlice)
  {
    return mipLevel + arraySlice * MipLevels;
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
    if(!IsDisposed)
    {
      Console.WriteLine($"    [Resource] Disposed texture {Name} (ID: {Id})");
      foreach(var view in _defaultViews.Values)
      {
        view?.Dispose();
      }
      _defaultViews.Clear();
      IsDisposed = true;
    }
  }
}
