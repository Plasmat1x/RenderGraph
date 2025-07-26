using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Resources.Enums;

namespace GraphicsAPI.Extensions;

public static class InputElementDescriptionExtension
{
  public static InputLayoutDescription Position => new InputLayoutDescription
  {
    Elements = new List<InputElementDescription>
    {
      new InputElementDescription
      {
        SemanticName = "POSITION",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32B32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 0,
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      }
    }
  };

  public static InputLayoutDescription PositionColor => new InputLayoutDescription
  {
    Elements = new List<InputElementDescription>
    {
      new InputElementDescription
      {
        SemanticName = "POSITION",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32B32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 0,
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      },
      new InputElementDescription
      {
        SemanticName = "COLOR",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32B32A32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 12, // sizeof(float3)
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      }
    }
  };

  public static InputLayoutDescription PositionTexture => new InputLayoutDescription
  {
    Elements = new List<InputElementDescription>
    {
      new InputElementDescription
      {
        SemanticName = "POSITION",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32B32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 0,
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      },
      new InputElementDescription
      {
        SemanticName = "TEXCOORD",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 12, // sizeof(float3)
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      }
    }
  };

  public static InputLayoutDescription PositionNormalTexture => new InputLayoutDescription
  {
    Elements = new List<InputElementDescription>
    {
      new InputElementDescription
      {
        SemanticName = "POSITION",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32B32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 0,
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      },
      new InputElementDescription
      {
        SemanticName = "NORMAL",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32B32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 12, // sizeof(float3)
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      },
      new InputElementDescription
      {
        SemanticName = "TEXCOORD",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 24, // sizeof(float3) * 2
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      }
    }
  };

  public static InputLayoutDescription PositionNormalTangentTexture => new InputLayoutDescription
  {
    Elements = new List<InputElementDescription>
    {
      new InputElementDescription
      {
        SemanticName = "POSITION",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32B32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 0,
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      },
      new InputElementDescription
      {
        SemanticName = "NORMAL",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32B32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 12, // sizeof(float3)
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      },
      new InputElementDescription
      {
        SemanticName = "TANGENT",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32B32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 24, // sizeof(float3) * 2
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      },
      new InputElementDescription
      {
        SemanticName = "TEXCOORD",
        SemanticIndex = 0,
        Format = TextureFormat.R32G32_FLOAT,
        InputSlot = 0,
        AlignedByteOffset = 36, // sizeof(float3) * 3
        InputSlotClass = InputClassification.PerVertexData,
        InstanceDataStepRate = 0
      }
    }
  };
}
