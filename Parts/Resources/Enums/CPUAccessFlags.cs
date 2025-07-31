/// <summary>
/// Флаги доступа CPU
/// </summary>
[Flags]
public enum CPUAccessFlags
{
  None = 0,
  Write = 1 << 0,
  Read = 1 << 1,
  ReadWrite = Read | Write
}