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

namespace Directx12Impl;
public class DX12Shader: IShader
{
  private readonly ShaderDescription p_description;
  private readonly byte[] p_bytecode;
  private readonly ShaderStage p_stage;
  private readonly ShaderBytecode p_d3d12Bytecode;
  private bool p_disposed;

  public unsafe DX12Shader(ShaderDescription _desc)
  {
    if(_desc.ByteCode != null && _desc.ByteCode.Length > 0)
    {
      p_bytecode = _desc.ByteCode;
    }
    else if(!string.IsNullOrEmpty(_desc.FilePath))
    {
      p_bytecode = File.ReadAllBytes(_desc.FilePath);
    }
    else
    {
      throw new ArgumentException("Shader description must contain ByteCode or FilePath");
    }

    unsafe
    {
      fixed(byte* pBytecode = p_bytecode)
      {
        p_d3d12Bytecode = new ShaderBytecode
        {
          PShaderBytecode = pBytecode,
          BytecodeLength = (nuint)p_bytecode.Length
        };
      }
    }
  }

  public ShaderStage Stage => p_stage;

  public ShaderDescription Description => p_description;

  public byte[] Bytecode => p_bytecode;

  public ResourceType ResourceType => ResourceType.Shader;

  public bool IsDisposed => p_disposed;

  public string Name => p_description.Name;

  public ulong GetMemorySize()
  {
    return (ulong)p_bytecode.Length;
  }

  public IntPtr GetNativeHandle()
  {
    throw new NotSupportedException("Shader doesn't have a native handle");
  }

  public ShaderReflection GetReflection()
  {
    return new ShaderReflection();
  }

  public bool HasConstantBuffer(string _name)
  {
    return false;
  }

  public bool HasSampler(string _name)
  {
    return false;
  }

  public bool HasTexture(string _name)
  {
    return false;
  }

  public unsafe ShaderBytecode GetD3D12Bytecode()
  {
    ThrowIfDisposed();

    fixed(byte* pBytecode = p_bytecode)
    {
      return new ShaderBytecode
      {
        PShaderBytecode = pBytecode,
        BytecodeLength = (nuint)p_bytecode.Length
      };
    }
  }

  public void Dispose()
  {
    if(p_disposed)
      return;

    p_disposed = true;
    GC.SuppressFinalize(this);
  }

  private void ThrowIfDisposed()
  {
    if(p_disposed)
      throw new ObjectDisposedException(nameof(DX12Shader));
  }

  public bool HasUnordererAccess(string _name)
  {
    throw new NotImplementedException();
  }

  public ConstantBufferInfo GetConstantBufferInfo(string _name)
  {
    throw new NotImplementedException();
  }

  public ResourceBindingInfo GetResourceInfo(string _name)
  {
    throw new NotImplementedException();
  }

  public SamplerBindingInfo GetSamplerInfo(string _name)
  {
    throw new NotImplementedException();
  }

  public bool IsCompatibleWith(IShader _otherShader)
  {
    throw new NotImplementedException();
  }
}
