using Directx12Impl;

using Resources;
using Resources.Enums;

namespace Directx12ImplTests;

/// <summary>
/// Performance тесты для системы передачи данных
/// </summary>
public class DX12DataTransferPerformanceTests: IDisposable
{
  private readonly DX12GraphicsDevice device;

  public DX12DataTransferPerformanceTests()
  {
    device = new DX12GraphicsDevice(false);
  }

  [Fact]
  public void Upload_Performance_Should_Be_Reasonable()
  {
    // Arrange
    var dataSize = 4 * 1024 * 1024; // 4MB
    var testData = new byte[dataSize];
    new Random().NextBytes(testData);

    var buffer = CreateLargeBuffer(dataSize);

    // Act
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    buffer.SetData(testData);
    stopwatch.Stop();

    // Assert
    var mbPerSecond = (dataSize / (1024.0 * 1024.0)) / stopwatch.Elapsed.TotalSeconds;

    // Should upload at least 100 MB/s (very conservative)
    Assert.True(mbPerSecond > 100,
        $"Upload speed too slow: {mbPerSecond:F2} MB/s");

    Console.WriteLine($"Upload performance: {mbPerSecond:F2} MB/s");

    buffer.Dispose();
  }

  [Fact]
  public void Batch_Upload_Should_Be_More_Efficient_Than_Individual()
  {
    // Arrange
    var smallBuffers = Enumerable.Range(0, 100)
        .Select(i => CreateSmallBuffer($"SmallBuffer_{i}", 1024))
        .ToArray();

    var testData = smallBuffers.Select(_ => new byte[1024]).ToArray();
    foreach(var data in testData)
    {
      new Random().NextBytes(data);
    }

    // Act - Individual uploads
    var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
    for(int i = 0; i < smallBuffers.Length; i++)
    {
      smallBuffers[i].SetData(testData[i]);
    }
    stopwatch1.Stop();

    // Create new set for batch test
    var batchBuffers = Enumerable.Range(0, 100)
        .Select(i => CreateSmallBuffer($"BatchBuffer_{i}", 1024))
        .ToArray();

    // Act - Batch upload
    var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
    device.BatchUploadResources(uploader =>
    {
      for(int i = 0; i < batchBuffers.Length; i++)
      {
        uploader.UploadBuffer(batchBuffers[i], testData[i]);
      }
    });
    stopwatch2.Stop();

    // Assert
    Console.WriteLine($"Individual uploads: {stopwatch1.ElapsedMilliseconds}ms");
    Console.WriteLine($"Batch upload: {stopwatch2.ElapsedMilliseconds}ms");

    // Batch should be faster (or at least not significantly slower)
    Assert.True(stopwatch2.ElapsedMilliseconds <= stopwatch1.ElapsedMilliseconds * 1.5);

    // Cleanup
    foreach(var buffer in smallBuffers.Concat(batchBuffers))
    {
      buffer.Dispose();
    }
  }

  private DX12Buffer CreateLargeBuffer(int sizeInBytes)
  {
    var desc = new BufferDescription
    {
      Name = "LargePerformanceBuffer",
      Size = (ulong)sizeInBytes,
      BufferUsage = BufferUsage.Structured,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default,
      Stride = 4
    };

    return device.CreateBuffer(desc) as DX12Buffer;
  }

  private DX12Buffer CreateSmallBuffer(string name, int sizeInBytes)
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

  public void Dispose()
  {
    device?.Dispose();
  }
}