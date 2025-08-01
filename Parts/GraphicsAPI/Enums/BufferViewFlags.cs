namespace GraphicsAPI.Enums;

/// <summary>
/// ?????????????? ????? ??? ???????? ?????????????
/// </summary>
[Flags]
public enum BufferViewFlags
{
  None = 0,
  Raw = 1 << 0,           // ByteAddressBuffer/RWByteAddressBuffer
  Counter = 1 << 1,       // StructuredBuffer with counter
  Append = 1 << 2,        // AppendStructuredBuffer
  Consume = 1 << 3        // ConsumeStructuredBuffer
}