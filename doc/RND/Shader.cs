using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RenderGraph.DirectX12
{
    public class DX12Shader : IShader
    {
        private readonly ShaderDescription _description;
        private readonly byte[] _bytecode;
        private readonly ShaderStage _stage;
        private readonly ComPtr<ID3D12ShaderReflection> _reflection;
        private readonly ShaderReflection _reflectionData;
        private readonly D3D12_SHADER_BYTECODE _d3d12Bytecode;
        private bool _isDisposed;

        public string Name => _description.Name;
        public ResourceType ResourceType => ResourceType.Shader;
        public ShaderStage Stage => _stage;
        public ShaderDescription Description => _description;
        public byte[] Bytecode => _bytecode;
        public bool IsDisposed => _isDisposed;

        public DX12Shader(ShaderDescription description)
        {
            _description = description ?? throw new ArgumentNullException(nameof(description));
            _stage = description.Stage;

            // Загружаем байткод
            if (description.ByteCode != null && description.ByteCode.Length > 0)
            {
                _bytecode = description.ByteCode;
            }
            else if (!string.IsNullOrEmpty(description.FilePath))
            {
                _bytecode = System.IO.File.ReadAllBytes(description.FilePath);
            }
            else if (!string.IsNullOrEmpty(description.SourceCode))
            {
                _bytecode = CompileFromSource(description);
            }
            else
            {
                throw new ArgumentException("Shader description must contain ByteCode, FilePath, or SourceCode");
            }

            // Создаем структуру для D3D12
            _d3d12Bytecode = new D3D12_SHADER_BYTECODE
            {
                pShaderBytecode = Marshal.AllocHGlobal(_bytecode.Length),
                BytecodeLength = (ulong)_bytecode.Length
            };
            Marshal.Copy(_bytecode, 0, _d3d12Bytecode.pShaderBytecode, _bytecode.Length);

            // Создаем reflection
            CreateReflection();
            _reflectionData = ParseReflection();
        }

        private byte[] CompileFromSource(ShaderDescription description)
        {
            // Компиляция шейдера из исходного кода
            var flags = D3DCOMPILE.D3DCOMPILE_ENABLE_STRICTNESS;
            
            #if DEBUG
            flags |= D3DCOMPILE.D3DCOMPILE_DEBUG | D3DCOMPILE.D3DCOMPILE_SKIP_OPTIMIZATION;
            #else
            flags |= D3DCOMPILE.D3DCOMPILE_OPTIMIZATION_LEVEL3;
            #endif

            ID3DBlob codeBlob = null;
            ID3DBlob errorBlob = null;

            try
            {
                var target = GetShaderTarget(description.Stage, description.ShaderModel);
                var entryPoint = description.EntryPoint ?? "main";

                HRESULT hr = D3DCompiler.D3DCompile(
                    description.SourceCode,
                    (uint)description.SourceCode.Length,
                    description.Name,
                    IntPtr.Zero, // defines
                    IntPtr.Zero, // include
                    entryPoint,
                    target,
                    flags,
                    0,
                    out codeBlob,
                    out errorBlob);

                if (FAILED(hr))
                {
                    string errorMessage = "Shader compilation failed";
                    if (errorBlob != null)
                    {
                        var errorPtr = errorBlob.GetBufferPointer();
                        var errorSize = errorBlob.GetBufferSize();
                        errorMessage = Marshal.PtrToStringAnsi(errorPtr, (int)errorSize);
                    }
                    throw new ShaderCompilationException(errorMessage);
                }

                // Копируем байткод
                var bufferPtr = codeBlob.GetBufferPointer();
                var bufferSize = (int)codeBlob.GetBufferSize();
                var bytecode = new byte[bufferSize];
                Marshal.Copy(bufferPtr, bytecode, 0, bufferSize);

                return bytecode;
            }
            finally
            {
                errorBlob?.Release();
                codeBlob?.Release();
            }
        }

        private string GetShaderTarget(ShaderStage stage, string shaderModel)
        {
            var profile = stage switch
            {
                ShaderStage.Vertex => "vs",
                ShaderStage.Hull => "hs",
                ShaderStage.Domain => "ds",
                ShaderStage.Geometry => "gs",
                ShaderStage.Pixel => "ps",
                ShaderStage.Compute => "cs",
                _ => throw new ArgumentException($"Unknown shader stage: {stage}")
            };

            return $"{profile}_{shaderModel ?? "5_1"}";
        }

        private void CreateReflection()
        {
            HRESULT hr = D3DCompiler.D3DReflect(
                _d3d12Bytecode.pShaderBytecode,
                _d3d12Bytecode.BytecodeLength,
                ref typeof(ID3D12ShaderReflection).GUID,
                out _reflection);

            if (FAILED(hr))
                throw new InvalidOperationException("Failed to create shader reflection");
        }

        private ShaderReflection ParseReflection()
        {
            var reflection = new ShaderReflection();
            
            D3D12_SHADER_DESC shaderDesc;
            _reflection.GetDesc(out shaderDesc);

            // Парсим constant buffers
            for (uint i = 0; i < shaderDesc.ConstantBuffers; i++)
            {
                var cb = _reflection.GetConstantBufferByIndex(i);
                D3D12_SHADER_BUFFER_DESC cbDesc;
                cb.GetDesc(out cbDesc);

                var cbInfo = new ConstantBufferInfo
                {
                    Name = cbDesc.Name,
                    Size = cbDesc.Size,
                    BindPoint = uint.MaxValue,
                    BindCount = 1,
                    Variables = new List<ShaderVariableInfo>()
                };

                // Парсим переменные в constant buffer
                for (uint j = 0; j < cbDesc.Variables; j++)
                {
                    var variable = cb.GetVariableByIndex(j);
                    D3D12_SHADER_VARIABLE_DESC varDesc;
                    variable.GetDesc(out varDesc);

                    var varInfo = new ShaderVariableInfo
                    {
                        Name = varDesc.Name,
                        Offset = varDesc.StartOffset,
                        Size = varDesc.Size
                    };

                    cbInfo.Variables.Add(varInfo);
                }

                reflection.ConstantBuffers.Add(cbInfo);
            }

            // Парсим bound resources (textures, samplers, etc.)
            for (uint i = 0; i < shaderDesc.BoundResources; i++)
            {
                D3D12_SHADER_INPUT_BIND_DESC bindDesc;
                _reflection.GetResourceBindingDesc(i, out bindDesc);

                switch (bindDesc.Type)
                {
                    case D3D_SHADER_INPUT_TYPE.D3D_SIT_CBUFFER:
                        // Обновляем bind point для constant buffer
                        var cb = reflection.ConstantBuffers.Find(c => c.Name == bindDesc.Name);
                        if (cb != null)
                        {
                            cb.BindPoint = bindDesc.BindPoint;
                            cb.BindCount = bindDesc.BindCount;
                        }
                        break;

                    case D3D_SHADER_INPUT_TYPE.D3D_SIT_TEXTURE:
                    case D3D_SHADER_INPUT_TYPE.D3D_SIT_STRUCTURED:
                    case D3D_SHADER_INPUT_TYPE.D3D_SIT_BYTEADDRESS:
                        reflection.BoundResources.Add(new ResourceBindingInfo
                        {
                            Name = bindDesc.Name,
                            Type = ConvertResourceType(bindDesc.Type),
                            BindPoint = bindDesc.BindPoint,
                            BindCount = bindDesc.BindCount,
                            Dimension = ConvertResourceDimension(bindDesc.Dimension)
                        });
                        break;

                    case D3D_SHADER_INPUT_TYPE.D3D_SIT_SAMPLER:
                        reflection.Samplers.Add(new SamplerBindingInfo
                        {
                            Name = bindDesc.Name,
                            BindPoint = bindDesc.BindPoint,
                            BindCount = bindDesc.BindCount
                        });
                        break;

                    case D3D_SHADER_INPUT_TYPE.D3D_SIT_UAV_RWTYPED:
                    case D3D_SHADER_INPUT_TYPE.D3D_SIT_UAV_RWSTRUCTURED:
                    case D3D_SHADER_INPUT_TYPE.D3D_SIT_UAV_RWBYTEADDRESS:
                        reflection.UnorderedAccessViews.Add(new ResourceBindingInfo
                        {
                            Name = bindDesc.Name,
                            Type = ConvertResourceType(bindDesc.Type),
                            BindPoint = bindDesc.BindPoint,
                            BindCount = bindDesc.BindCount,
                            Dimension = ConvertResourceDimension(bindDesc.Dimension)
                        });
                        break;
                }
            }

            // Парсим input layout для vertex shader
            if (_stage == ShaderStage.Vertex)
            {
                ParseInputLayout(shaderDesc, reflection);
            }

            // Парсим output для вычислительных шейдеров
            if (_stage == ShaderStage.Compute)
            {
                reflection.ThreadGroupSize = new Vector3(
                    shaderDesc.ThreadGroupSizeX,
                    shaderDesc.ThreadGroupSizeY,
                    shaderDesc.ThreadGroupSizeZ);
            }

            return reflection;
        }

        private void ParseInputLayout(D3D12_SHADER_DESC shaderDesc, ShaderReflection reflection)
        {
            for (uint i = 0; i < shaderDesc.InputParameters; i++)
            {
                D3D12_SIGNATURE_PARAMETER_DESC paramDesc;
                _reflection.GetInputParameterDesc(i, out paramDesc);

                var element = new InputElementInfo
                {
                    SemanticName = paramDesc.SemanticName,
                    SemanticIndex = paramDesc.SemanticIndex,
                    Format = ConvertSignatureElementFormat(paramDesc),
                    InputSlot = 0, // D3D12 reflection не предоставляет эту информацию
                    AlignedByteOffset = uint.MaxValue, // APPEND_ALIGNED_ELEMENT
                    InputSlotClass = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                };

                reflection.InputElements.Add(element);
            }
        }

        private ResourceBindingType ConvertResourceType(D3D_SHADER_INPUT_TYPE type)
        {
            return type switch
            {
                D3D_SHADER_INPUT_TYPE.D3D_SIT_TEXTURE => ResourceBindingType.Texture,
                D3D_SHADER_INPUT_TYPE.D3D_SIT_SAMPLER => ResourceBindingType.Sampler,
                D3D_SHADER_INPUT_TYPE.D3D_SIT_CBUFFER => ResourceBindingType.ConstantBuffer,
                D3D_SHADER_INPUT_TYPE.D3D_SIT_STRUCTURED => ResourceBindingType.StructuredBuffer,
                D3D_SHADER_INPUT_TYPE.D3D_SIT_BYTEADDRESS => ResourceBindingType.ByteAddressBuffer,
                D3D_SHADER_INPUT_TYPE.D3D_SIT_UAV_RWTYPED => ResourceBindingType.UnorderedAccessView,
                D3D_SHADER_INPUT_TYPE.D3D_SIT_UAV_RWSTRUCTURED => ResourceBindingType.RWStructuredBuffer,
                D3D_SHADER_INPUT_TYPE.D3D_SIT_UAV_RWBYTEADDRESS => ResourceBindingType.RWByteAddressBuffer,
                _ => ResourceBindingType.Unknown
            };
        }

        private TextureDimension ConvertResourceDimension(D3D_RESOURCE_DIMENSION dimension)
        {
            return dimension switch
            {
                D3D_RESOURCE_DIMENSION.D3D_RESOURCE_DIMENSION_TEXTURE1D => TextureDimension.Texture1D,
                D3D_RESOURCE_DIMENSION.D3D_RESOURCE_DIMENSION_TEXTURE2D => TextureDimension.Texture2D,
                D3D_RESOURCE_DIMENSION.D3D_RESOURCE_DIMENSION_TEXTURE3D => TextureDimension.Texture3D,
                D3D_RESOURCE_DIMENSION.D3D_RESOURCE_DIMENSION_TEXTURECUBE => TextureDimension.TextureCube,
                _ => TextureDimension.Unknown
            };
        }

        private DXGI_FORMAT ConvertSignatureElementFormat(D3D12_SIGNATURE_PARAMETER_DESC paramDesc)
        {
            // Определяем формат на основе маски компонентов и типа
            var componentCount = 0;
            if ((paramDesc.Mask & 0x01) != 0) componentCount++;
            if ((paramDesc.Mask & 0x02) != 0) componentCount++;
            if ((paramDesc.Mask & 0x04) != 0) componentCount++;
            if ((paramDesc.Mask & 0x08) != 0) componentCount++;

            return paramDesc.ComponentType switch
            {
                D3D_REGISTER_COMPONENT_TYPE.D3D_REGISTER_COMPONENT_FLOAT32 => componentCount switch
                {
                    1 => DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT,
                    2 => DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT,
                    3 => DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT,
                    4 => DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT,
                    _ => DXGI_FORMAT.DXGI_FORMAT_UNKNOWN
                },
                D3D_REGISTER_COMPONENT_TYPE.D3D_REGISTER_COMPONENT_SINT32 => componentCount switch
                {
                    1 => DXGI_FORMAT.DXGI_FORMAT_R32_SINT,
                    2 => DXGI_FORMAT.DXGI_FORMAT_R32G32_SINT,
                    3 => DXGI_FORMAT.DXGI_FORMAT_R32G32B32_SINT,
                    4 => DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_SINT,
                    _ => DXGI_FORMAT.DXGI_FORMAT_UNKNOWN
                },
                D3D_REGISTER_COMPONENT_TYPE.D3D_REGISTER_COMPONENT_UINT32 => componentCount switch
                {
                    1 => DXGI_FORMAT.DXGI_FORMAT_R32_UINT,
                    2 => DXGI_FORMAT.DXGI_FORMAT_R32G32_UINT,
                    3 => DXGI_FORMAT.DXGI_FORMAT_R32G32B32_UINT,
                    4 => DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_UINT,
                    _ => DXGI_FORMAT.DXGI_FORMAT_UNKNOWN
                },
                _ => DXGI_FORMAT.DXGI_FORMAT_UNKNOWN
            };
        }

        public ShaderReflection GetReflection()
        {
            ThrowIfDisposed();
            return _reflectionData;
        }

        public bool HasConstantBuffer(string name)
        {
            ThrowIfDisposed();
            return _reflectionData.ConstantBuffers.Exists(cb => cb.Name == name);
        }

        public bool HasTexture(string name)
        {
            ThrowIfDisposed();
            return _reflectionData.BoundResources.Exists(r => r.Name == name);
        }

        public bool HasSampler(string name)
        {
            ThrowIfDisposed();
            return _reflectionData.Samplers.Exists(s => s.Name == name);
        }

        public D3D12_SHADER_BYTECODE GetD3D12Bytecode()
        {
            ThrowIfDisposed();
            return _d3d12Bytecode;
        }

        public IntPtr GetNativeHandle()
        {
            ThrowIfDisposed();
            return _d3d12Bytecode.pShaderBytecode;
        }

        public ulong GetMemorySize()
        {
            return (ulong)_bytecode.Length;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_d3d12Bytecode.pShaderBytecode != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_d3d12Bytecode.pShaderBytecode);
            }

            _reflection?.Release();

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DX12Shader));
        }

        private static bool FAILED(HRESULT hr) => hr < 0;
    }

    // Исключение для ошибок компиляции шейдеров
    public class ShaderCompilationException : Exception
    {
        public ShaderCompilationException(string message) : base(message) { }
        public ShaderCompilationException(string message, Exception innerException) : base(message, innerException) { }
    }
}