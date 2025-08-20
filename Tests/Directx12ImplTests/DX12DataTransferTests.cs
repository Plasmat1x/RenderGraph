using Directx12Impl;

using GraphicsAPI.Enums;

using Resources;
using Resources.Enums;

using System.Numerics;

namespace Directx12ImplTests;

/// <summary>
/// Тесты для системы передачи данных DirectX 12
/// </summary>
public unsafe class DX12DataTransferTests: IDisposable
{
  private readonly DX12GraphicsDevice device;

  public DX12DataTransferTests()
  {
    // Создаем device для тестов (с подавлением debug вывода)
    device = new DX12GraphicsDevice(false);
  }

  [Fact]
  public void Buffer_SetData_Should_Work_For_Default_Buffers()
  {
    // Arrange
    var testData = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };

    var bufferDesc = new BufferDescription
    {
      Name = "TestBuffer",
      Size = (ulong)(testData.Length * sizeof(float)),
      BufferUsage = BufferUsage.Vertex,
      BindFlags = BindFlags.VertexBuffer,
      Usage = ResourceUsage.Default,
      Stride = sizeof(float)
    };

    // Act
    var buffer = device.CreateBuffer(bufferDesc) as DX12Buffer;

    // Should not throw
    var exception = Record.Exception(() => buffer.SetData(testData));

    // Assert
    Assert.Null(exception);
    Assert.Equal(bufferDesc.Size, buffer.Size);

    buffer.Dispose();
  }

  [Fact]
  public void Buffer_SetData_Should_Work_For_Dynamic_Buffers()
  {
    // Arrange
    var testData = new int[] { 10, 20, 30, 40, 50 };

    var bufferDesc = new BufferDescription
    {
      Name = "DynamicTestBuffer",
      Size = (ulong)(testData.Length * sizeof(int)),
      BufferUsage = BufferUsage.Constant,
      BindFlags = BindFlags.ConstantBuffer,
      Usage = ResourceUsage.Dynamic,
      CPUAccessFlags = CPUAccessFlags.Write
    };

    // Act
    var buffer = device.CreateBuffer(bufferDesc) as DX12Buffer;

    // Should not throw
    var exception = Record.Exception(() => buffer.SetData(testData));

    // Assert
    Assert.Null(exception);
    Assert.True(buffer.CanMap());

    buffer.Dispose();
  }

  [Fact]
  public void Buffer_GetData_Should_Return_Correct_Data()
  {
    // Arrange
    var originalData = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

    var bufferDesc = new BufferDescription
    {
      Name = "ReadbackTestBuffer",
      Size = (ulong)originalData.Length,
      BufferUsage = BufferUsage.Structured,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default,
      Stride = 1
    };

    var buffer = device.CreateBuffer(bufferDesc) as DX12Buffer;

    // Act
    buffer.SetData(originalData);
    var readbackData = buffer.GetData<byte>(0, -1);

    // Assert
    Assert.Equal(originalData.Length, readbackData.Length);
    Assert.Equal(originalData, readbackData);

    buffer.Dispose();
  }

  [Fact]
  public void Buffer_SetData_With_Offset_Should_Work()
  {
    // Arrange
    var bufferSize = 1024;
    var testData = new float[] { 1.5f, 2.5f, 3.5f };
    var offset = 512UL;

    var bufferDesc = new BufferDescription
    {
      Name = "OffsetTestBuffer",
      Size = (ulong)bufferSize,
      BufferUsage = BufferUsage.Structured,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default,
      Stride = sizeof(float)
    };

    var buffer = device.CreateBuffer(bufferDesc) as DX12Buffer;

    // Act & Assert
    var exception = Record.Exception(() => buffer.SetData(testData, offset));
    Assert.Null(exception);

    buffer.Dispose();
  }

  [Fact]
  public void Texture_SetData_Should_Work_For_2D_Textures()
  {
    // Arrange
    var width = 64u;
    var height = 64u;
    var pixelData = CreateTestTextureData(width, height);

    var textureDesc = new TextureDescription
    {
      Name = "TestTexture2D",
      Width = width,
      Height = height,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Format = TextureFormat.R8G8B8A8_UNORM,
      TextureUsage = TextureUsage.ShaderResource,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default
    };

    // Act
    var texture = device.CreateTexture(textureDesc) as DX12Texture;

    // Should not throw
    var exception = Record.Exception(() => texture.SetData(pixelData));

    // Assert
    Assert.Null(exception);
    Assert.Equal(width, texture.Description.Width);
    Assert.Equal(height, texture.Description.Height);

    texture.Dispose();
  }

  [Fact]
  public void Texture_SetData_Should_Work_For_Mipped_Textures()
  {
    // Arrange
    var width = 128u;
    var height = 128u;
    var mipLevels = 4u;

    var textureDesc = new TextureDescription
    {
      Name = "MippedTestTexture",
      Width = width,
      Height = height,
      Depth = 1,
      MipLevels = mipLevels,
      ArraySize = 1,
      Format = TextureFormat.R8G8B8A8_UNORM,
      TextureUsage = TextureUsage.ShaderResource,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default
    };

    var texture = device.CreateTexture(textureDesc) as DX12Texture;

    // Act & Assert - Upload data to each mip level
    for(uint mip = 0; mip < mipLevels; mip++)
    {
      var mipWidth = Math.Max(1u, width >> (int)mip);
      var mipHeight = Math.Max(1u, height >> (int)mip);
      var mipData = CreateTestTextureData(mipWidth, mipHeight);

      var exception = Record.Exception(() => texture.SetData(mipData, mip, 0));
      Assert.Null(exception);
    }

    texture.Dispose();
  }

  [Fact]
  public void Texture_GetData_Should_Return_Correct_Data()
  {
    // Arrange
    var width = 32u;
    var height = 32u;
    var originalData = CreateCheckerboardPattern(width, height);

    var textureDesc = new TextureDescription
    {
      Name = "ReadbackTestTexture",
      Width = width,
      Height = height,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Format = TextureFormat.R8G8B8A8_UNORM,
      TextureUsage = TextureUsage.ShaderResource,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default
    };

    var texture = device.CreateTexture(textureDesc) as DX12Texture;

    // Act
    texture.SetData(originalData);
    var readbackData = texture.GetData<byte>();

    // Assert
    Assert.Equal(originalData.Length, readbackData.Length);

    Assert.Equal(originalData[0], readbackData[0]);
    Assert.Equal(originalData[originalData.Length - 1], readbackData[readbackData.Length - 1]);

    texture.Dispose();
  }

  [Fact]
  public void BatchUpload_Should_Handle_Multiple_Resources()
  {
    // Arrange
    var buffer1Data = new float[] { 1.0f, 2.0f, 3.0f };
    var buffer2Data = new int[] { 100, 200, 300, 400 };
    var textureData = CreateTestTextureData(64, 64);

    var buffer1 = CreateTestBuffer("Buffer1", buffer1Data.Length * sizeof(float));
    var buffer2 = CreateTestBuffer("Buffer2", buffer2Data.Length * sizeof(int));
    var texture = CreateTestTexture("Texture1", 64, 64);

    // Act
    var exception = Record.Exception(() =>
    {
      device.BatchUploadResources(uploader =>
      {
        uploader.UploadBuffer(buffer1, buffer1Data);
        uploader.UploadBuffer(buffer2, buffer2Data);
        uploader.UploadTexture(texture, textureData);
      });
    });

    // Assert
    Assert.Null(exception);

    // Cleanup
    buffer1.Dispose();
    buffer2.Dispose();
    texture.Dispose();
  }

  [Fact]
  public void Map_Unmap_Should_Work_For_Dynamic_Buffers()
  {
    // Arrange
    var bufferDesc = new BufferDescription
    {
      Name = "MappableBuffer",
      Size = 1024,
      BufferUsage = BufferUsage.Constant,
      BindFlags = BindFlags.ConstantBuffer,
      Usage = ResourceUsage.Dynamic,
      CPUAccessFlags = CPUAccessFlags.Write
    };

    var buffer = device.CreateBuffer(bufferDesc) as DX12Buffer;

    // Act
    var mappedPtr = device.MapBuffer(buffer, MapMode.WriteDiscard);

    // Write some test data
    unsafe
    {
      var floatPtr = (float*)mappedPtr.ToPointer();
      *floatPtr = 42.0f;
    }

    var exception = Record.Exception(() => device.UnmapBuffer(buffer));

    // Assert
    Assert.NotEqual(IntPtr.Zero, mappedPtr);
    Assert.Null(exception);

    buffer.Dispose();
  }

  [Fact]
  public void Upload_System_Should_Handle_Large_Data()
  {
    // Arrange
    var largeDataSize = 16 * 1024 * 1024; // 16MB
    var largeData = new byte[largeDataSize];

    // Fill with pattern
    for(int i = 0; i < largeDataSize; i++)
    {
      largeData[i] = (byte)(i % 256);
    }

    var bufferDesc = new BufferDescription
    {
      Name = "LargeBuffer",
      Size = (ulong)largeDataSize,
      BufferUsage = BufferUsage.Structured,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default,
      Stride = 1
    };

    var buffer = device.CreateBuffer(bufferDesc) as DX12Buffer;

    // Act
    var exception = Record.Exception(() => buffer.SetData(largeData));

    // Assert
    Assert.Null(exception);

    buffer.Dispose();
  }

  [Theory]
  [InlineData(ResourceUsage.Default)]
  [InlineData(ResourceUsage.Dynamic)]
  public void SetData_Should_Respect_Usage_Patterns(ResourceUsage usage)
  {
    // Arrange
    var testData = new Vector4[]
    {
            new Vector4(1, 0, 0, 1),
            new Vector4(0, 1, 0, 1),
            new Vector4(0, 0, 1, 1)
    };

    var bufferDesc = new BufferDescription
    {
      Name = $"UsageTest_{usage}",
      Size = (ulong)(testData.Length * sizeof(Vector4)),
      BufferUsage = BufferUsage.Vertex,
      BindFlags = BindFlags.VertexBuffer,
      Usage = usage,
      CPUAccessFlags = usage == ResourceUsage.Dynamic ? CPUAccessFlags.Write : CPUAccessFlags.None
    };

    var buffer = device.CreateBuffer(bufferDesc) as DX12Buffer;

    // Act & Assert
    var exception = Record.Exception(() => buffer.SetData(testData));
    Assert.Null(exception);

    buffer.Dispose();
  }

  // === Helper methods ===

  private DX12Buffer CreateTestBuffer(string name, int sizeInBytes)
  {
    var desc = new BufferDescription
    {
      Name = name,
      Size = (ulong)sizeInBytes,
      BufferUsage = BufferUsage.Structured,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default,
      Stride = 4
    };

    return device.CreateBuffer(desc) as DX12Buffer;
  }

  private DX12Texture CreateTestTexture(string name, uint width, uint height)
  {
    var desc = new TextureDescription
    {
      Name = name,
      Width = width,
      Height = height,
      Depth = 1,
      MipLevels = 1,
      ArraySize = 1,
      Format = TextureFormat.R8G8B8A8_UNORM,
      TextureUsage = TextureUsage.ShaderResource,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default
    };

    return device.CreateTexture(desc) as DX12Texture;
  }

  private byte[] CreateTestTextureData(uint width, uint height)
  {
    var data = new byte[width * height * 4]; // RGBA
    var random = new Random(42); // Deterministic for tests
    random.NextBytes(data);
    return data;
  }

  private byte[] CreateCheckerboardPattern(uint width, uint height)
  {
    var data = new byte[width * height * 4]; // RGBA
    var index = 0;

    for(uint y = 0; y < height; y++)
    {
      for(uint x = 0; x < width; x++)
      {
        bool isWhite = ((x / 8) + (y / 8)) % 2 == 0;
        byte color = (byte)(isWhite ? 255 : 0);

        data[index++] = color; // R
        data[index++] = color; // G
        data[index++] = color; // B
        data[index++] = 255;   // A
      }
    }

    return data;
  }

  public void Dispose()
  {
    device?.Dispose();
  }
}
