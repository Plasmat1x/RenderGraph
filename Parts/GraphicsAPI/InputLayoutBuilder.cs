using GraphicsAPI.Descriptions;
using GraphicsAPI.Enums;

using Resources.Enums;
using Resources.Extensions;

namespace GraphicsAPI;

public class InputLayoutBuilder
{
  private readonly List<InputElementDescription> p_elements = [];
  private uint p_currentOffset = 0;
  private uint p_currentSlot = 0;

  public InputLayoutBuilder AddElement(
      string _semanticName,
      uint _semanticIndex,
      TextureFormat _format,
      uint? _inputSlot = null,
      uint? _alignedByteOffset = null,
      InputClassification _inputSlotClass = InputClassification.PerVertexData,
      uint _instanceDataStepRate = 0)
  {
    var element = new InputElementDescription
    {
      SemanticName = _semanticName,
      SemanticIndex = _semanticIndex,
      Format = _format,
      AlignedByteOffset = _alignedByteOffset ?? p_currentOffset,
      InputSlotClass = _inputSlotClass,
      InputSlot = _inputSlot ?? p_currentSlot,
      InstanceDataStepRate = _instanceDataStepRate
    };

    p_elements.Add(element);

    if(!_alignedByteOffset.HasValue)
      p_currentOffset += _format.GetFormatSize();

    return this;
  }

  public InputLayoutBuilder NextSlot()
  {
    p_currentSlot++;
    p_currentOffset = 0;
    return this;
  }

  public InputLayoutDescription Build()
  {
    return new InputLayoutDescription { Elements = p_elements };
  }
}