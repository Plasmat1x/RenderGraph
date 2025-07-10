using Resources.Enums;

namespace Core;

public struct ResourceHandle
{
  public const uint INVALID_ID = 0;
  public const uint INVALID_GENERATION = 0;

  public static bool operator ==(ResourceHandle _left, ResourceHandle _right) => _left.Equals(_right);

  public static bool operator !=(ResourceHandle _left, ResourceHandle _right) => !_left.Equals(_right);

  public ResourceHandle(uint _id, ResourceType _type, uint _generation, string _name)
  {
    Id = _id;
    Type = _type;
    Generation = _generation;
    Name = _name ?? string.Empty;
  }

  public uint Id { get; set; }
  public ResourceType Type { get; set; }
  public uint Generation { get; set; }
  public string Name { get; set; }
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

  public override bool Equals(object _obj) => _obj is ResourceHandle other && Equals(other);

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
}
