using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;
using GraphicsAPI.Reflections;

using Resources.Enums;

using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;

using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Directx12Impl;
public unsafe class DX12Shader: IShader
{
  // Статический провайдер рефлексии
  private static readonly DX12ShaderReflectionProvider s_reflectionProvider = new();

  // Статический компилятор
  private static readonly D3DCompiler s_compiler = D3DCompiler.GetApi();

  private readonly ShaderDescription p_description;
  private readonly byte[] p_bytecode;
  private readonly ShaderStage p_stage;
  private readonly ShaderBytecode p_d3d12Bytecode;
  private ShaderReflection p_reflection;
  private GCHandle p_bytecodeHandle;
  private bool p_disposed;

  public unsafe DX12Shader(ShaderDescription _desc)
  {
    p_description = _desc ?? throw new ArgumentNullException(nameof(_desc));
    p_stage = _desc.Stage;

    // Получение байткода
    if(_desc.ByteCode != null && _desc.ByteCode.Length > 0)
    {
      p_bytecode = _desc.ByteCode;
    }
    else if(!string.IsNullOrEmpty(_desc.FilePath))
    {
      p_bytecode = File.ReadAllBytes(_desc.FilePath);
    }
    else if(!string.IsNullOrEmpty(_desc.SourceCode))
    {
      p_bytecode = CompileFromSource(_desc);
    }
    else
    {
      throw new ArgumentException("Shader description must contain ByteCode, FilePath, or SourceCode");
    }

    // Создание D3D12 структуры байткода
    p_bytecodeHandle = GCHandle.Alloc(p_bytecode, GCHandleType.Pinned);
    p_d3d12Bytecode = new ShaderBytecode
    {
      PShaderBytecode = p_bytecodeHandle.AddrOfPinnedObject().ToPointer(),
      BytecodeLength = (nuint)p_bytecode.Length
    };

    // Создание рефлексии
    CreateReflection();
  }

  public ShaderStage Stage => p_stage;

  public ShaderDescription Description => p_description;

  public byte[] Bytecode => p_bytecode;

  public ResourceType ResourceType => ResourceType.Shader;

  public bool IsDisposed => p_disposed;

  public string Name => p_description.Name;


  /// <summary>
  /// Возвращает D3D12 структуру байткода для использования в PSO
  /// </summary>
  public ShaderBytecode GetD3D12Bytecode() => p_d3d12Bytecode;

  public ShaderReflection GetReflection()
  {
    if(p_reflection == null)
    {
      CreateReflection();
    }
    return p_reflection;
  }

  public bool HasConstantBuffer(string _name)
  {
    return p_reflection?.ConstantBuffers.Any(cb => cb.Name == _name) ?? false;
  }

  public bool HasTexture(string _name)
  {
    return p_reflection?.BoundResources.Any(r =>
        r.Name == _name && r.Type == ResourceBindingType.ShaderResource) ?? false;
  }

  public bool HasSampler(string _name)
  {
    return p_reflection?.Samplers.Any(s => s.Name == _name) ?? false;
  }

  public bool HasUnordererAccess(string _name)
  {
    return p_reflection?.UnorderedAccessViews.Any(uav => uav.Name == _name) ?? false;
  }

  public ConstantBufferInfo GetConstantBufferInfo(string _name)
  {
    return p_reflection?.GetConstantBuffer(_name);
  }

  public ResourceBindingInfo GetResourceInfo(string _name)
  {
    return p_reflection?.GetResource(_name);
  }

  public SamplerBindingInfo GetSamplerInfo(string _name)
  {
    return p_reflection?.GetSampler(_name);
  }

  public bool IsCompatibleWith(IShader _otherShader)
  {
    if(_otherShader == null)
      return false;

    var otherReflection = _otherShader.GetReflection();
    return ShaderReflectionUtils.AreStagesCompatible(p_reflection, otherReflection);
  }

  public IntPtr GetNativeHandle()
  {
    // Шейдеры в D3D12 не имеют отдельного handle, они используются через байткод
    throw new NotSupportedException("Shader doesn't have a native handle in D3D12");
  }

  public ulong GetMemorySize()
  {
    return (ulong)p_bytecode.Length;
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    if(p_bytecodeHandle.IsAllocated)
    {
      p_bytecodeHandle.Free();
    }

    p_disposed = true;
  }

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(DX12Shader));
  }

  private unsafe byte[] CompileFromSource(ShaderDescription _desc)
  {
    //D3DCOMPILE_DEBUG               0x00000001  // /Zi
    //D3DCOMPILE_SKIP_OPTIMIZATION   0x00000002  // /Od
    //D3DCOMPILE_OPTIMIZATION_LEVEL3 0x00000020  // /O3
    //D3DCOMPILE_ENABLE_STRICTNESS   0x00000800  // /Ges
    //D3DCOMPILE_DEBUG               0x0001
    //D3DCOMPILE_SKIP_OPTIMIZATION   0x0002
    //D3DCOMPILE_OPTIMIZATION_LEVEL0 0x0004
    //D3DCOMPILE_OPTIMIZATION_LEVEL1 0x0008
    //D3DCOMPILE_OPTIMIZATION_LEVEL2 0x0010
    //D3DCOMPILE_OPTIMIZATION_LEVEL3 0x0020
    //D3DCOMPILE_ENABLE_STRICTNESS   0x0800

    var flags = (uint)(0x0800);

#if DEBUG
    flags |= (uint)(0x00000001 | 0x0002);
#else
        flags |= (uint)0x0020;
#endif

    var target = GetShaderTarget(_desc.Stage, _desc.ShaderModel);
    var entryPoint = _desc.EntryPoint ?? "main";

    ComPtr<ID3D10Blob> codeBlob = default;
    ComPtr<ID3D10Blob> errorBlob = default;

    try
    {
      D3DShaderMacro* macros = null;
      if(_desc.Defines != null && _desc.Defines.Count > 0)
      {
        var macroArray = stackalloc D3DShaderMacro[_desc.Defines.Count + 1];
        for(int i = 0; i < _desc.Defines.Count; i++)
        {
          var namePtr = Marshal.StringToHGlobalAnsi(_desc.Defines[i].Name);
          var defPtr = Marshal.StringToHGlobalAnsi(_desc.Defines[i].Definition);

          macroArray[i] = new D3DShaderMacro
          {
            Name = (byte*)namePtr,
            Definition = (byte*)defPtr
          };
        }
        macroArray[_desc.Defines.Count] = new D3DShaderMacro { Name = null, Definition = null };
        macros = macroArray;
      }

      var sourceBytes = Encoding.UTF8.GetBytes(_desc.SourceCode);
      fixed(byte* pSource = sourceBytes)
      fixed(byte* pEntryPoint = Encoding.UTF8.GetBytes(entryPoint))
      fixed(byte* pTarget = Encoding.UTF8.GetBytes(target))
      fixed(byte* pSourceName = Encoding.UTF8.GetBytes(_desc.Name ?? "shader"))
      {
        HResult hr = s_compiler.Compile( 
            pSource,
            (nuint)sourceBytes.Length,
            pSourceName,
            macros,
            null, // Include handler
            pEntryPoint,
            pTarget,
            flags,
            0,
            codeBlob.GetAddressOf(),
            errorBlob.GetAddressOf());

        if(hr.IsFailure)
        {
          string errorMessage = "Shader compilation failed";
          if(errorBlob.Handle != null)
          {
            var errorSize = errorBlob.GetBufferSize();
            var errorPtr = errorBlob.GetBufferPointer();
            errorMessage = Marshal.PtrToStringAnsi((IntPtr)errorPtr, (int)errorSize);
          }
          throw new InvalidOperationException($"Failed to compile shader '{_desc.Name}': {errorMessage}");
        }
      }

      var codeSize = codeBlob.GetBufferSize();
      var codePtr = codeBlob.GetBufferPointer();
      var bytecode = new byte[codeSize];
      Marshal.Copy((IntPtr)codePtr, bytecode, 0, (int)codeSize);

      return bytecode;
    }
    finally
    {
      if(codeBlob.Handle != null)
        codeBlob.Dispose();
      if(errorBlob.Handle != null)
        errorBlob.Dispose();
    }
  }

  private void CreateReflection()
  {
    if(p_description.CachedReflection != null)
    {
      p_reflection = p_description.CachedReflection;
      return;
    }

    try
    {
      p_reflection = s_reflectionProvider.CreateReflection(p_bytecode, p_stage);
      p_description.CachedReflection = p_reflection;
    }
    catch(Exception ex)
    {
      Console.WriteLine($"Warning: Failed to create reflection for shader '{Name}': {ex.Message}");
      p_reflection = new ShaderReflection();
    }
  }

  private string GetShaderTarget(ShaderStage _stage, string _model)
  {
    var modelVersion = string.IsNullOrEmpty(_model) ? "5_1" : _model.Replace(".", "_");

    return _stage switch
    {
      ShaderStage.Vertex => $"vs_{modelVersion}",
      ShaderStage.Pixel => $"ps_{modelVersion}",
      ShaderStage.Geometry => $"gs_{modelVersion}",
      ShaderStage.Hull => $"hs_{modelVersion}",
      ShaderStage.Domain => $"ds_{modelVersion}",
      ShaderStage.Compute => $"cs_{modelVersion}",
      _ => throw new ArgumentException($"Unsupported shader stage: {_stage}")
    };
  }
}
