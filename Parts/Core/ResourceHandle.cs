using Core.Enums;

namespace Core;

public struct ResourceHandle
{
  public uint Id;
  public ResourceType Type;
  public uint Generation;

  public readonly bool IsValid()
  {
    throw new NotImplementedException();
  }

  public bool Equals(ResourceHandle _other)
  {
    return _other.Id == Id 
      && _other.Type == Type
      && _other.Generation == Generation
      && _other.IsValid();
  }

}
