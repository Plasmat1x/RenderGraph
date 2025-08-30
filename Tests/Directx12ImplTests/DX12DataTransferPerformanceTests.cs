using Directx12Impl;

using Resources;
using Resources.Enums;

namespace Directx12ImplTests;

/// <summary>
/// Performance тесты для системы передачи данных
/// </summary>
public class DX12DataTransferPerformanceTests: IDisposable
{
  private readonly DX12GraphicsDevice p_device;

  public DX12DataTransferPerformanceTests()
  {
    p_device = new DX12GraphicsDevice(false);
  }

  [Fact]
  public void Upload_Performance_Should_Be_Reasonable()
  {

    var dataSize = 4 * 1024 * 1024;
    var testData = new byte[dataSize];
    new Random().NextBytes(testData);

    var buffer = CreateLargeBuffer(dataSize);

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    buffer.SetData(testData);
    stopwatch.Stop();


    var mbPerSecond = (dataSize / (1024.0 * 1024.0)) / stopwatch.Elapsed.TotalSeconds;

    Assert.True(mbPerSecond > 100,
        $"Upload speed too slow: {mbPerSecond:F2} MB/s");

    Console.WriteLine($"Upload performance: {mbPerSecond:F2} MB/s");

    buffer.Dispose();
  }

  [Fact]
  public void Batch_Upload_Should_Be_More_Efficient_Than_Individual()
  {
    var smallBuffers = Enumerable.Range(0, 100)
        .Select(_i => CreateSmallBuffer($"SmallBuffer_{_i}", 1024))
        .ToArray();

    var testData = smallBuffers.Select(_ => new byte[1024]).ToArray();
    foreach(var data in testData)
    {
      new Random().NextBytes(data);
    }

    var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
    for(int i = 0; i < smallBuffers.Length; i++)
    {
      smallBuffers[i].SetData(testData[i]);
    }
    stopwatch1.Stop();

    var batchBuffers = Enumerable.Range(0, 100)
        .Select(_i => CreateSmallBuffer($"BatchBuffer_{_i}", 1024))
        .ToArray();

    var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
    p_device.BatchUploadResources(_uploader => {
      for(int i = 0; i < batchBuffers.Length; i++)
      {
        _uploader.UploadBuffer(batchBuffers[i], testData[i]);
      }
    });
    stopwatch2.Stop();

    Console.WriteLine($"Individual uploads: {stopwatch1.ElapsedMilliseconds}ms");
    Console.WriteLine($"Batch upload: {stopwatch2.ElapsedMilliseconds}ms");

    Assert.True(stopwatch2.ElapsedMilliseconds <= stopwatch1.ElapsedMilliseconds * 1.5);


    foreach(var buffer in smallBuffers.Concat(batchBuffers))
    {
      buffer.Dispose();
    }
  }

  private DX12Buffer CreateLargeBuffer(int _sizeInBytes)
  {
    var desc = new BufferDescription
    {
      Name = "LargePerformanceBuffer",
      Size = (ulong)_sizeInBytes,
      BufferUsage = BufferUsage.Structured,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default,
      Stride = 4
    };

    return p_device.CreateBuffer(desc) as DX12Buffer;
  }

  private DX12Buffer CreateSmallBuffer(string _name, int _sizeInBytes)
  {
    var desc = new BufferDescription
    {
      Name = _name,
      Size = (ulong)_sizeInBytes,
      BufferUsage = BufferUsage.Structured,
      BindFlags = BindFlags.ShaderResource,
      Usage = ResourceUsage.Default,
      Stride = 4
    };

    return p_device.CreateBuffer(desc) as DX12Buffer;
  }

  public void Dispose()
  {
    p_device?.Dispose();
  }
}