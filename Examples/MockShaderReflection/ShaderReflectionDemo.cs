using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Extensions;
using GraphicsAPI.Reflections.Extensions;

using MockImpl;

namespace Examples.ShaderReflectionDemo;

public class ShaderReflectionDemo
{
  public static void Run()
  {
    Console.WriteLine("=== Shader Reflection Demo ===\n");

    using var device = new MockGraphicsDevice();

    // Демонстрация 1: Создание шейдеров и получение рефлексии
    DemoBasicReflection(device);

    // Демонстрация 2: Автоматическое создание Input Layout
    DemoInputLayoutGeneration(device);

    // Демонстрация 3: Проверка совместимости шейдеров
    DemoShaderCompatibility(device);

    // Демонстрация 4: Работа с константными буферами
    DemoConstantBufferReflection(device);

    // Демонстрация 5: Работа с ресурсами
    DemoResourceBindings(device);

    Console.WriteLine("\n=== Demo Complete ===");
  }

  private static void DemoBasicReflection(MockGraphicsDevice device)
  {
    Console.WriteLine("\n📊 Demo 1: Basic Shader Reflection");
    Console.WriteLine("==================================");

    // Создаем вершинный шейдер
    var vsDesc = new ShaderDescription
    {
      Name = "BasicVertexShader",
      Stage = ShaderStage.Vertex,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 }, // Mock bytecode
      EntryPoint = "VSMain",
      ShaderModel = "5_1"
    };

    using var vertexShader = device.CreateShader(vsDesc);
    var vsReflection = vertexShader.GetReflection();

    Console.WriteLine($"\n✅ Created {vertexShader.Name}");
    Console.WriteLine($"   Constant Buffers: {vsReflection.ConstantBuffers.Count}");
    Console.WriteLine($"   Input Parameters: {vsReflection.InputParameters.Count}");
    Console.WriteLine($"   Output Parameters: {vsReflection.OutputParameters.Count}");

    // Создаем пиксельный шейдер
    var psDesc = new ShaderDescription
    {
      Name = "BasicPixelShader",
      Stage = ShaderStage.Pixel,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 },
      EntryPoint = "PSMain",
      ShaderModel = "5_1"
    };

    using var pixelShader = device.CreateShader(psDesc);
    var psReflection = pixelShader.GetReflection();

    Console.WriteLine($"\n✅ Created {pixelShader.Name}");
    Console.WriteLine($"   Constant Buffers: {psReflection.ConstantBuffers.Count}");
    Console.WriteLine($"   Textures: {psReflection.BoundResources.Count}");
    Console.WriteLine($"   Samplers: {psReflection.Samplers.Count}");
  }

  private static void DemoInputLayoutGeneration(MockGraphicsDevice device)
  {
    Console.WriteLine("\n\n🔧 Demo 2: Automatic Input Layout Generation");
    Console.WriteLine("=============================================");

    // Создаем вершинный шейдер
    var vsDesc = new ShaderDescription
    {
      Name = "VertexShaderWithInputs",
      Stage = ShaderStage.Vertex,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 }
    };

    using var shader = device.CreateShader(vsDesc);

    // Генерируем Input Layout из рефлексии
    var inputLayout = InputLayoutDescription.FromShader(shader);

    Console.WriteLine($"\n✅ Generated Input Layout with {inputLayout.Elements.Count} elements:");

    foreach(var element in inputLayout.Elements)
    {
      Console.WriteLine($"   - {element.SemanticName}{element.SemanticIndex}:");
      Console.WriteLine($"     Format: {element.Format}");
      Console.WriteLine($"     Offset: {element.AlignedByteOffset} bytes");
      Console.WriteLine($"     Slot: {element.InputSlot}");
    }

    // Вычисляем размер вершины
    uint vertexSize = inputLayout.GetVertexSizeForSlot(0);
    Console.WriteLine($"\n📏 Total vertex size: {vertexSize} bytes");

    // Демонстрация multi-stream layout
    Console.WriteLine("\n🌊 Multi-stream Input Layout:");

    var semanticMapping = new Dictionary<string, uint>
        {
            { "POSITION", 0 },
            { "NORMAL", 0 },
            { "TEXCOORD", 1 },
            { "COLOR", 1 }
        };

    var multiStreamLayout = InputLayoutDescription.FromMultipleStreams(shader, semanticMapping);

    Console.WriteLine($"   Stream 0 size: {multiStreamLayout.GetVertexSizeForSlot(0)} bytes");
    Console.WriteLine($"   Stream 1 size: {multiStreamLayout.GetVertexSizeForSlot(1)} bytes");
  }

  private static void DemoShaderCompatibility(MockGraphicsDevice device)
  {
    Console.WriteLine("\n\n🔗 Demo 3: Shader Compatibility Check");
    Console.WriteLine("======================================");

    // Создаем совместимые шейдеры
    var vs1 = device.CreateShader(new ShaderDescription
    {
      Name = "CompatibleVS",
      Stage = ShaderStage.Vertex,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 }
    });

    var ps1 = device.CreateShader(new ShaderDescription
    {
      Name = "CompatiblePS",
      Stage = ShaderStage.Pixel,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 }
    });

    bool areCompatible = vs1.GetReflection()
      .Compatible(ps1.GetReflection());

    Console.WriteLine($"\n✅ Vertex Shader '{vs1.Name}' and Pixel Shader '{ps1.Name}':");
    Console.WriteLine($"   Compatibility: {(areCompatible ? "✓ Compatible" : "✗ Incompatible")}");

    // Проверяем метод IsCompatibleWith
    bool directCheck = vs1.IsCompatibleWith(ps1);
    Console.WriteLine($"   Direct check: {(directCheck ? "✓ Compatible" : "✗ Incompatible")}");

    vs1.Dispose();
    ps1.Dispose();
  }

  private static void DemoConstantBufferReflection(MockGraphicsDevice device)
  {
    Console.WriteLine("\n\n💾 Demo 4: Constant Buffer Reflection");
    Console.WriteLine("======================================");

    var shader = device.CreateShader(new ShaderDescription
    {
      Name = "ShaderWithConstants",
      Stage = ShaderStage.Vertex,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 }
    });

    var reflection = shader.GetReflection();

    Console.WriteLine($"\n✅ Shader '{shader.Name}' constant buffers:");

    foreach(var cb in reflection.ConstantBuffers)
    {
      Console.WriteLine($"\n   📦 {cb.Name}:");
      Console.WriteLine($"      Bind Point: {cb.BindPoint}");
      Console.WriteLine($"      Size: {cb.Size} bytes");
      Console.WriteLine($"      Variables: {cb.Variables.Count}");

      foreach(var variable in cb.Variables)
      {
        Console.WriteLine($"      - {variable.Name} ({variable.Type})");
        Console.WriteLine($"        Offset: {variable.Offset}, Size: {variable.Size}");
      }
    }

    // Проверка наличия константного буфера
    string cbName = "PerFrameConstants";
    bool hasCB = shader.HasConstantBuffer(cbName);
    Console.WriteLine($"\n🔍 Has '{cbName}': {hasCB}");

    if(hasCB)
    {
      var cbInfo = shader.GetConstantBufferInfo(cbName);
      Console.WriteLine($"   Size: {cbInfo.Size} bytes");
      Console.WriteLine($"   Slot: {cbInfo.BindPoint}");
    }

    // Вычисление общего размера
    uint totalCBSize = ShaderReflectionExtensions.CalculateTotalConstantBufferSize(reflection);
    Console.WriteLine($"\n📊 Total constant buffer memory: {totalCBSize} bytes");

    shader.Dispose();
  }

  private static void DemoResourceBindings(MockGraphicsDevice device)
  {
    Console.WriteLine("\n\n🎨 Demo 5: Resource Bindings");
    Console.WriteLine("=============================");

    var shader = device.CreateShader(new ShaderDescription
    {
      Name = "ShaderWithResources",
      Stage = ShaderStage.Pixel,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 }
    });

    var reflection = shader.GetReflection();

    // Текстуры
    Console.WriteLine($"\n🖼️  Textures ({reflection.BoundResources.Count}):");
    foreach(var resource in reflection.BoundResources)
    {
      Console.WriteLine($"   - {resource.Name}:");
      Console.WriteLine($"     Type: {resource.Dimension}");
      Console.WriteLine($"     Slot: {resource.BindPoint}");
      Console.WriteLine($"     Return Type: {resource.ReturnType}");
    }

    // Сэмплеры
    Console.WriteLine($"\n🔍 Samplers ({reflection.Samplers.Count}):");
    foreach(var sampler in reflection.Samplers)
    {
      Console.WriteLine($"   - {sampler.Name} (Slot: {sampler.BindPoint})");
    }

    // Проверка наличия ресурсов
    Console.WriteLine("\n📋 Resource checks:");
    string[] texturesToCheck = { "DiffuseTexture", "NormalTexture", "SpecularTexture", "NonExistentTexture" };

    foreach(var texName in texturesToCheck)
    {
      bool hasTexture = shader.HasTexture(texName);
      Console.WriteLine($"   Has '{texName}': {(hasTexture ? "✓" : "✗")}");

      if(hasTexture)
      {
        var texInfo = shader.GetResourceInfo(texName);
        Console.WriteLine($"     → Slot: {texInfo.BindPoint}, Dimension: {texInfo.Dimension}");
      }
    }

    // Используемые слоты
    var usedTextureSlots = ShaderReflectionExtensions.GetUsedResourceSlots(
        reflection,
        ResourceBindingType.ShaderResource
    );

    Console.WriteLine($"\n🎯 Used texture slots: [{string.Join(", ", usedTextureSlots)}]");

    shader.Dispose();
  }

  // Дополнительный пример: Создание Pipeline State с автоматической настройкой
  public static void CreatePipelineStateWithReflection(MockGraphicsDevice device)
  {
    Console.WriteLine("\n\n🚀 Bonus: Automatic Pipeline State Configuration");
    Console.WriteLine("================================================");

    var vsDesc = new ShaderDescription
    {
      Name = "AutoVS",
      Stage = ShaderStage.Vertex,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 }
    };

    var psDesc = new ShaderDescription
    {
      Name = "AutoPS",
      Stage = ShaderStage.Pixel,
      ByteCode = new byte[] { 0x44, 0x58, 0x42, 0x43 }
    };

    using var vs = device.CreateShader(vsDesc);
    using var ps = device.CreateShader(psDesc);

    var pipelineDesc = new PipelineStateDescription
    {
      Name = "AutoConfiguredPipeline",
      VertexShader = vs,
      PixelShader = ps
    };

    pipelineDesc.ApplyReflectionToPipelineState(
        vs.GetReflection(),
        ps.GetReflection()
    );

    Console.WriteLine($"\n✅ Pipeline State '{pipelineDesc.Name}' configured:");
    Console.WriteLine($"   Input Layout Elements: {pipelineDesc.InputLayout.Elements.Count}");
    Console.WriteLine($"   Vertex Shader: {pipelineDesc.VertexShader.Name}");
    Console.WriteLine($"   Pixel Shader: {pipelineDesc.PixelShader.Name}");

    // Валидация
    if(pipelineDesc.InputLayout.Validate(out string error))
    {
      Console.WriteLine("   ✓ Input Layout is valid");
    }
    else
    {
      Console.WriteLine($"   ✗ Input Layout validation failed: {error}");
    }
  }
}