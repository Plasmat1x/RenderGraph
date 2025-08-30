using GraphicsAPI.Descriptions;

using Silk.NET.Direct3D12;

using System.Runtime.InteropServices;

namespace Directx12Impl.Extensions;

public static class InputLayoutDescriptionExtensions
{
  /// <summary>
  /// Конвертирует InputLayoutDescription в массив InputElementDesc
  /// ВАЖНО: Возвращает также список указателей на строки, которые должны быть освобождены после использования
  /// </summary>
  public static unsafe (InputElementDesc[] Elements, List<IntPtr> StringPointers) ConvertWithMemory(
      this InputLayoutDescription _layoutDesc)
  {
    if(_layoutDesc?.Elements == null || _layoutDesc.Elements.Count == 0)
      return (Array.Empty<InputElementDesc>(), new List<IntPtr>());

    var elements = new InputElementDesc[_layoutDesc.Elements.Count];
    var stringPointers = new List<IntPtr>();

    for(var i = 0; i < _layoutDesc.Elements.Count; i++)
    {
      var element = _layoutDesc.Elements[i];

      // Проверка что имя не пустое
      if(string.IsNullOrEmpty(element.SemanticName))
      {
        throw new InvalidOperationException($"Semantic name cannot be null or empty for element at index {i}");
      }

      // Выделяем память для строки и сохраняем указатель
      var semanticNamePtr = Marshal.StringToHGlobalAnsi(element.SemanticName);
      stringPointers.Add(semanticNamePtr);

      elements[i] = new InputElementDesc
      {
        SemanticName = (byte*)semanticNamePtr,
        SemanticIndex = element.SemanticIndex,
        Format = element.Format.Convert(),
        InputSlot = element.InputSlot,
        AlignedByteOffset = element.AlignedByteOffset == uint.MaxValue
              ? D3D12.AppendAlignedElement
              : element.AlignedByteOffset,
        InputSlotClass = element.InputSlotClass == GraphicsAPI.Enums.InputClassification.PerVertexData
              ? Silk.NET.Direct3D12.InputClassification.PerVertexData
              : Silk.NET.Direct3D12.InputClassification.PerInstanceData,
        InstanceDataStepRate = element.InstanceDataStepRate
      };
    }

    return (elements, stringPointers);
  }

  /// <summary>
  /// Старый метод для обратной совместимости (НЕ ИСПОЛЬЗОВАТЬ!)
  /// </summary>
  [Obsolete("Use ConvertWithMemory instead. This method has memory management issues.")]
  public static unsafe InputElementDesc[] Convert(this InputLayoutDescription _layoutDesc)
  {
    var (elements, stringPointers) = ConvertWithMemory(_layoutDesc);
    // Это создаст проблему с памятью, но оставляем для совместимости
    // StringPointers будут потеряны и память утечёт
    return elements;
  }

  /// <summary>
  /// Освобождает память выделенную для строк
  /// </summary>
  public static void FreeStringPointers(List<IntPtr> _stringPointers)
  {
    if(_stringPointers == null)
      return;

    foreach(var ptr in _stringPointers)
    {
      if(ptr != IntPtr.Zero)
      {
        Marshal.FreeHGlobal(ptr);
      }
    }
  }
}