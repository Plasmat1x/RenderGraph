using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RenderGraph.DirectX12
{
    // Описание шейдера (должно быть в основном проекте RenderGraph)
    public class ShaderDescription
    {
        public string Name { get; set; }
        public ShaderStage Stage { get; set; }
        public string EntryPoint { get; set; } = "main";
        public string ShaderModel { get; set; } = "5_1";
        
        // Варианты загрузки шейдера
        public byte[] ByteCode { get; set; }
        public string FilePath { get; set; }
        public string SourceCode { get; set; }
        
        // Дополнительные параметры компиляции
        public List<ShaderMacro> Defines { get; set; } = new List<ShaderMacro>();
        public string[] IncludePaths { get; set; }
    }

    public struct ShaderMacro
    {
        public string Name { get; set; }
        public string Definition { get; set; }
    }

    // Reflection данные
    public class ShaderReflection
    {
        public List<ConstantBufferInfo> ConstantBuffers { get; set; } = new List<ConstantBufferInfo>();
        public List<ResourceBindingInfo> BoundResources { get; set; } = new List<ResourceBindingInfo>();
        public List<SamplerBindingInfo> Samplers { get; set; } = new List<SamplerBindingInfo>();
        public List<ResourceBindingInfo> UnorderedAccessViews { get; set; } = new List<ResourceBindingInfo>();
        public List<InputElementInfo> InputElements { get; set; } = new List<InputElementInfo>();
        public Vector3 ThreadGroupSize { get; set; }
    }

    public class ConstantBufferInfo
    {
        public string Name { get; set; }
        public uint Size { get; set; }
        public uint BindPoint { get; set; }
        public uint BindCount { get; set; }
        public List<ShaderVariableInfo> Variables { get; set; }
    }

    public class ShaderVariableInfo
    {
        public string Name { get; set; }
        public uint Offset { get; set; }
        public uint Size { get; set; }
    }

    public class ResourceBindingInfo
    {
        public string Name { get; set; }
        public ResourceBindingType Type { get; set; }
        public uint BindPoint { get; set; }
        public uint BindCount { get; set; }
        public TextureDimension Dimension { get; set; }
    }

    public class SamplerBindingInfo
    {
        public string Name { get; set; }
        public uint BindPoint { get; set; }
        public uint BindCount { get; set; }
    }

    public class InputElementInfo
    {
        public string SemanticName { get; set; }
        public uint SemanticIndex { get; set; }
        public DXGI_FORMAT Format { get; set; }
        public uint InputSlot { get; set; }
        public uint AlignedByteOffset { get; set; }
        public InputClassification InputSlotClass { get; set; }
        public uint InstanceDataStepRate { get; set; }
    }

    public enum ResourceBindingType
    {
        Unknown,
        Texture,
        Sampler,
        ConstantBuffer,
        StructuredBuffer,
        ByteAddressBuffer,
        UnorderedAccessView,
        RWStructuredBuffer,
        RWByteAddressBuffer
    }

    public enum TextureDimension
    {
        Unknown,
        Texture1D,
        Texture2D,
        Texture3D,
        TextureCube,
        Texture1DArray,
        Texture2DArray,
        TextureCubeArray
    }

    public enum InputClassification
    {
        PerVertexData,
        PerInstanceData
    }

    // D3D12 Shader Bytecode
    [StructLayout(LayoutKind.Sequential)]
    public struct D3D12_SHADER_BYTECODE
    {
        public IntPtr pShaderBytecode;
        public ulong BytecodeLength;
    }

    // Флаги компиляции
    public enum D3DCOMPILE : uint
    {
        D3DCOMPILE_DEBUG = (1 << 0),
        D3DCOMPILE_SKIP_VALIDATION = (1 << 1),
        D3DCOMPILE_SKIP_OPTIMIZATION = (1 << 2),
        D3DCOMPILE_PACK_MATRIX_ROW_MAJOR = (1 << 3),
        D3DCOMPILE_PACK_MATRIX_COLUMN_MAJOR = (1 << 4),
        D3DCOMPILE_PARTIAL_PRECISION = (1 << 5),
        D3DCOMPILE_FORCE_VS_SOFTWARE_NO_OPT = (1 << 6),
        D3DCOMPILE_FORCE_PS_SOFTWARE_NO_OPT = (1 << 7),
        D3DCOMPILE_NO_PRESHADER = (1 << 8),
        D3DCOMPILE_AVOID_FLOW_CONTROL = (1 << 9),
        D3DCOMPILE_PREFER_FLOW_CONTROL = (1 << 10),
        D3DCOMPILE_ENABLE_STRICTNESS = (1 << 11),
        D3DCOMPILE_ENABLE_BACKWARDS_COMPATIBILITY = (1 << 12),
        D3DCOMPILE_IEEE_STRICTNESS = (1 << 13),
        D3DCOMPILE_OPTIMIZATION_LEVEL0 = (1 << 14),
        D3DCOMPILE_OPTIMIZATION_LEVEL1 = 0,
        D3DCOMPILE_OPTIMIZATION_LEVEL2 = ((1 << 14) | (1 << 15)),
        D3DCOMPILE_OPTIMIZATION_LEVEL3 = (1 << 15),
        D3DCOMPILE_WARNINGS_ARE_ERRORS = (1 << 18)
    }

    // Интерфейсы D3D Reflection (упрощенные)
    [ComImport]
    [Guid("8d536ca1-0cca-4956-a837-786963755584")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID3D12ShaderReflection
    {
        HRESULT GetDesc(out D3D12_SHADER_DESC desc);
        ID3D12ShaderReflectionConstantBuffer GetConstantBufferByIndex(uint index);
        ID3D12ShaderReflectionConstantBuffer GetConstantBufferByName(string name);
        HRESULT GetResourceBindingDesc(uint index, out D3D12_SHADER_INPUT_BIND_DESC desc);
        HRESULT GetInputParameterDesc(uint index, out D3D12_SIGNATURE_PARAMETER_DESC desc);
        HRESULT GetOutputParameterDesc(uint index, out D3D12_SIGNATURE_PARAMETER_DESC desc);
        // ... другие методы
    }

    [ComImport]
    [Guid("8e5c260f-c793-4276-a894-d2ff9e23f7e4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID3D12ShaderReflectionConstantBuffer
    {
        HRESULT GetDesc(out D3D12_SHADER_BUFFER_DESC desc);
        ID3D12ShaderReflectionVariable GetVariableByIndex(uint index);
        ID3D12ShaderReflectionVariable GetVariableByName(string name);
    }

    [ComImport]
    [Guid("8337a8a6-a216-444a-b2f4-314733a73aea")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID3D12ShaderReflectionVariable
    {
        HRESULT GetDesc(out D3D12_SHADER_VARIABLE_DESC desc);
    }

    // Структуры для Reflection
    [StructLayout(LayoutKind.Sequential)]
    public struct D3D12_SHADER_DESC
    {
        public uint Version;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Creator;
        public uint Flags;
        public uint ConstantBuffers;
        public uint BoundResources;
        public uint InputParameters;
        public uint OutputParameters;
        public uint InstructionCount;
        public uint TempRegisterCount;
        public uint TempArrayCount;
        public uint DefCount;
        public uint DclCount;
        public uint TextureNormalInstructions;
        public uint TextureLoadInstructions;
        public uint TextureCompInstructions;
        public uint TextureBiasInstructions;
        public uint TextureGradientInstructions;
        public uint FloatInstructionCount;
        public uint IntInstructionCount;
        public uint UintInstructionCount;
        public uint StaticFlowControlCount;
        public uint DynamicFlowControlCount;
        public uint MacroInstructionCount;
        public uint ArrayInstructionCount;
        public uint CutInstructionCount;
        public uint EmitInstructionCount;
        public uint GSOutputTopology;
        public uint GSMaxOutputVertexCount;
        public uint InputPrimitive;
        public uint PatchConstantParameters;
        public uint CGSInstanceCount;
        public uint ControlPoints;
        public uint HSOutputPrimitive;
        public uint HSPartitioning;
        public uint TessellatorDomain;
        public uint BarrierInstructions;
        public uint InterlockedInstructions;
        public uint TextureStoreInstructions;
        public uint ThreadGroupSizeX;
        public uint ThreadGroupSizeY;
        public uint ThreadGroupSizeZ;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3D12_SHADER_BUFFER_DESC
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        public uint Type;
        public uint Variables;
        public uint Size;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3D12_SHADER_VARIABLE_DESC
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        public uint StartOffset;
        public uint Size;
        public uint Flags;
        public IntPtr DefaultValue;
        public uint StartTexture;
        public uint TextureSize;
        public uint StartSampler;
        public uint SamplerSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3D12_SHADER_INPUT_BIND_DESC
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        public D3D_SHADER_INPUT_TYPE Type;
        public uint BindPoint;
        public uint BindCount;
        public uint Flags;
        public D3D_RESOURCE_RETURN_TYPE ReturnType;
        public D3D_RESOURCE_DIMENSION Dimension;
        public uint NumSamples;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3D12_SIGNATURE_PARAMETER_DESC
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string SemanticName;
        public uint SemanticIndex;
        public uint Register;
        public D3D_NAME SystemValueType;
        public D3D_REGISTER_COMPONENT_TYPE ComponentType;
        public byte Mask;
        public byte ReadWriteMask;
        public uint Stream;
        public D3D_MIN_PRECISION MinPrecision;
    }

    // Перечисления для Reflection
    public enum D3D_SHADER_INPUT_TYPE
    {
        D3D_SIT_CBUFFER,
        D3D_SIT_TBUFFER,
        D3D_SIT_TEXTURE,
        D3D_SIT_SAMPLER,
        D3D_SIT_UAV_RWTYPED,
        D3D_SIT_STRUCTURED,
        D3D_SIT_UAV_RWSTRUCTURED,
        D3D_SIT_BYTEADDRESS,
        D3D_SIT_UAV_RWBYTEADDRESS,
        D3D_SIT_UAV_APPEND_STRUCTURED,
        D3D_SIT_UAV_CONSUME_STRUCTURED,
        D3D_SIT_UAV_RWSTRUCTURED_WITH_COUNTER
    }

    public enum D3D_RESOURCE_RETURN_TYPE
    {
        D3D_RETURN_TYPE_UNORM = 1,
        D3D_RETURN_TYPE_SNORM = 2,
        D3D_RETURN_TYPE_SINT = 3,
        D3D_RETURN_TYPE_UINT = 4,
        D3D_RETURN_TYPE_FLOAT = 5,
        D3D_RETURN_TYPE_MIXED = 6,
        D3D_RETURN_TYPE_DOUBLE = 7,
        D3D_RETURN_TYPE_CONTINUED = 8
    }

    public enum D3D_RESOURCE_DIMENSION
    {
        D3D_RESOURCE_DIMENSION_UNKNOWN = 0,
        D3D_RESOURCE_DIMENSION_BUFFER = 1,
        D3D_RESOURCE_DIMENSION_TEXTURE1D = 2,
        D3D_RESOURCE_DIMENSION_TEXTURE2D = 3,
        D3D_RESOURCE_DIMENSION_TEXTURE3D = 4,
        D3D_RESOURCE_DIMENSION_TEXTURECUBE = 5,
        D3D_RESOURCE_DIMENSION_TEXTURE1DARRAY = 6,
        D3D_RESOURCE_DIMENSION_TEXTURE2DARRAY = 7,
        D3D_RESOURCE_DIMENSION_TEXTURECUBEARRAY = 8
    }

    public enum D3D_NAME
    {
        D3D_NAME_UNDEFINED = 0,
        D3D_NAME_POSITION = 1,
        D3D_NAME_CLIP_DISTANCE = 2,
        D3D_NAME_CULL_DISTANCE = 3,
        D3D_NAME_RENDER_TARGET_ARRAY_INDEX = 4,
        D3D_NAME_VIEWPORT_ARRAY_INDEX = 5,
        D3D_NAME_VERTEX_ID = 6,
        D3D_NAME_PRIMITIVE_ID = 7,
        D3D_NAME_INSTANCE_ID = 8,
        D3D_NAME_IS_FRONT_FACE = 9,
        D3D_NAME_SAMPLE_INDEX = 10,
        D3D_NAME_FINAL_QUAD_EDGE_TESSFACTOR = 11,
        D3D_NAME_FINAL_QUAD_INSIDE_TESSFACTOR = 12,
        D3D_NAME_FINAL_TRI_EDGE_TESSFACTOR = 13,
        D3D_NAME_FINAL_TRI_INSIDE_TESSFACTOR = 14,
        D3D_NAME_FINAL_LINE_DETAIL_TESSFACTOR = 15,
        D3D_NAME_FINAL_LINE_DENSITY_TESSFACTOR = 16,
        D3D_NAME_TARGET = 64,
        D3D_NAME_DEPTH = 65,
        D3D_NAME_COVERAGE = 66,
        D3D_NAME_DEPTH_GREATER_EQUAL = 67,
        D3D_NAME_DEPTH_LESS_EQUAL = 68
    }

    public enum D3D_REGISTER_COMPONENT_TYPE
    {
        D3D_REGISTER_COMPONENT_UNKNOWN = 0,
        D3D_REGISTER_COMPONENT_UINT32 = 1,
        D3D_REGISTER_COMPONENT_SINT32 = 2,
        D3D_REGISTER_COMPONENT_FLOAT32 = 3
    }

    public enum D3D_MIN_PRECISION
    {
        D3D_MIN_PRECISION_DEFAULT = 0,
        D3D_MIN_PRECISION_FLOAT_16 = 1,
        D3D_MIN_PRECISION_FLOAT_2_8 = 2,
        D3D_MIN_PRECISION_RESERVED = 3,
        D3D_MIN_PRECISION_SINT_16 = 4,
        D3D_MIN_PRECISION_UINT_16 = 5,
        D3D_MIN_PRECISION_ANY_16 = 0xf0,
        D3D_MIN_PRECISION_ANY_10 = 0xf1
    }

    // COM интерфейсы для компиляции
    [ComImport]
    [Guid("8BA5FB08-5195-40e2-AC58-0D989C3A0102")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID3DBlob
    {
        IntPtr GetBufferPointer();
        ulong GetBufferSize();
        void Release();
    }

    // P/Invoke для D3DCompiler
    public static class D3DCompiler
    {
        [DllImport("d3dcompiler_47.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern HRESULT D3DCompile(
            [MarshalAs(UnmanagedType.LPStr)] string pSrcData,
            uint SrcDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string pSourceName,
            IntPtr pDefines,
            IntPtr pInclude,
            [MarshalAs(UnmanagedType.LPStr)] string pEntrypoint,
            [MarshalAs(UnmanagedType.LPStr)] string pTarget,
            D3DCOMPILE Flags1,
            uint Flags2,
            out ID3DBlob ppCode,
            out ID3DBlob ppErrorMsgs);

        [DllImport("d3dcompiler_47.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern HRESULT D3DReflect(
            IntPtr pSrcData,
            ulong SrcDataSize,
            [In] ref Guid pInterface,
            out ID3D12ShaderReflection ppReflector);

        [DllImport("d3dcompiler_47.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern HRESULT D3DCompileFromFile(
            [MarshalAs(UnmanagedType.LPWStr)] string pFileName,
            IntPtr pDefines,
            IntPtr pInclude,
            [MarshalAs(UnmanagedType.LPStr)] string pEntrypoint,
            [MarshalAs(UnmanagedType.LPStr)] string pTarget,
            D3DCOMPILE Flags1,
            uint Flags2,
            out ID3DBlob ppCode,
            out ID3DBlob ppErrorMsgs);

        [DllImport("d3dcompiler_47.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern HRESULT D3DDisassemble(
            IntPtr pSrcData,
            ulong SrcDataSize,
            uint Flags,
            [MarshalAs(UnmanagedType.LPStr)] string szComments,
            out ID3DBlob ppDisassembly);

        [DllImport("d3dcompiler_47.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern HRESULT D3DStripShader(
            IntPtr pShaderBytecode,
            ulong BytecodeLength,
            uint uStripFlags,
            out ID3DBlob ppStrippedBlob);
    }

    // Вспомогательный класс для создания Input Layout
    public static class InputLayoutHelper
    {
        public static D3D12_INPUT_ELEMENT_DESC[] CreateD3D12InputLayout(List<InputElementInfo> elements)
        {
            var d3d12Elements = new D3D12_INPUT_ELEMENT_DESC[elements.Count];
            
            for (int i = 0; i < elements.Count; i++)
            {
                d3d12Elements[i] = new D3D12_INPUT_ELEMENT_DESC
                {
                    SemanticName = elements[i].SemanticName,
                    SemanticIndex = elements[i].SemanticIndex,
                    Format = elements[i].Format,
                    InputSlot = elements[i].InputSlot,
                    AlignedByteOffset = elements[i].AlignedByteOffset,
                    InputSlotClass = ConvertInputClassification(elements[i].InputSlotClass),
                    InstanceDataStepRate = elements[i].InstanceDataStepRate
                };
            }
            
            return d3d12Elements;
        }

        private static D3D12_INPUT_CLASSIFICATION ConvertInputClassification(InputClassification classification)
        {
            return classification switch
            {
                InputClassification.PerVertexData => D3D12_INPUT_CLASSIFICATION.D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                InputClassification.PerInstanceData => D3D12_INPUT_CLASSIFICATION.D3D12_INPUT_CLASSIFICATION_PER_INSTANCE_DATA,
                _ => D3D12_INPUT_CLASSIFICATION.D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA
            };
        }
    }

    // D3D12 Input Layout структуры
    [StructLayout(LayoutKind.Sequential)]
    public struct D3D12_INPUT_ELEMENT_DESC
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string SemanticName;
        public uint SemanticIndex;
        public DXGI_FORMAT Format;
        public uint InputSlot;
        public uint AlignedByteOffset;
        public D3D12_INPUT_CLASSIFICATION InputSlotClass;
        public uint InstanceDataStepRate;
    }

    public enum D3D12_INPUT_CLASSIFICATION
    {
        D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA = 0,
        D3D12_INPUT_CLASSIFICATION_PER_INSTANCE_DATA = 1
    }

    // Константа для автоматического выравнивания
    public static class D3D12Constants
    {
        public const uint D3D12_APPEND_ALIGNED_ELEMENT = uint.MaxValue;
    }
}