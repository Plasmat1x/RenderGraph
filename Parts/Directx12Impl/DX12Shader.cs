using GraphicsAPI;
using GraphicsAPI.Enums;
using GraphicsAPI.Interfaces;

using Resources.Enums;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Directx12Impl;
public class DX12Shader: IShader
{
  private ComPtr<ID3D10Blob> p_shaderByteCode;
  private ShaderStage p_stage;
  private ShaderDescription p_shaderDescription;
  private ComPtr<ID3D12ShaderReflection> p_reflcetion;
  private InputElementDesc[] p_inputLayout;
  private ComPtr<ID3D12RootSignature> p_rootSignature;

  public DX12Shader()
  {

  }

  public ShaderStage Stage => p_stage;

  public ShaderDescription Description => p_shaderDescription;

  public byte[] Bytecode { get; private set; }

  public ResourceType ResourceType { get; private set; }

  public bool IsDisposed { get; private set; }

  public string Name { get; set; }

  public ulong GetMemorySize()
  {
    throw new NotImplementedException();
  }

  public nint GetNativeHandle()
  {
    throw new NotImplementedException();
  }

  public ShaderReflection GetReflection()
  {
    throw new NotImplementedException();
  }

  public bool HasConstantBuffer(string _name)
  {
    throw new NotImplementedException();
  }

  public bool HasSampler(string _name)
  {
    throw new NotImplementedException();
  }

  public bool HasTexture(string _name)
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }
}
