using Core.Enums;

namespace Core;

public struct ResourceHandle
{
  public uint Id;
  public ResourceType Type;
  public uint Generation;
  public string Name;

  public const uint INVALID_ID = 0;
  public const uint INVALID_GENERATION = 0;

  public ResourceHandle(uint id, ResourceType type, uint generation, string name)
  {
    Id = id;
    Type = type;
    Generation = generation;
    Name = name ?? string.Empty;
  }

  public static ResourceHandle Invalid => new ResourceHandle(INVALID_ID, ResourceType.Texture2D, INVALID_GENERATION, "Invalid");


  public readonly bool IsValid()
  {
    return Id != INVALID_ID &&
       Generation != INVALID_GENERATION &&
       !string.IsNullOrEmpty(Name);
  }

  public bool Equals(ResourceHandle _other)
  {
    return _other.Id == Id &&
           _other.Type == Type &&
           _other.Generation == Generation &&
           string.Equals(_other.Name, Name, StringComparison.Ordinal);
  }

  public override bool Equals(object obj)
  {
    return obj is ResourceHandle other && Equals(other);
  }

  public int GetHashCode()
  {
    unchecked
    {
      int hash = 17;
      hash = hash * 23 + (int)Id;
      hash = hash * 23 + (int)Type;
      hash = hash * 23 + (int)Generation;
      hash = hash * 23 + (Name?.GetHashCode() ?? 0);
      return hash;
    }
  }

  public string ToString()
  {
    if(!IsValid())
      return "ResourceHandle(Invalid)";

    return $"ResourceHandle(Id: {Id}, Type: {Type}, Gen: {Generation}, Name: '{Name}')";
  }

  public static bool operator ==(ResourceHandle left, ResourceHandle right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(ResourceHandle left, ResourceHandle right)
  {
    return !left.Equals(right);
  }
}
