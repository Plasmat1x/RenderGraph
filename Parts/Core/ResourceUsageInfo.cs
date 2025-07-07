using Core.Enums;

using GraphicsAPI.Enums;

using Resources.Enums;

namespace Core;
public class ResourceUsageInfo
{
  public ResourceHandle Handle { get; set; }
  public ResourceAccessType AccessType { get; set; }
  public ResourceUsage Usage { get; set; }
  public ResourceState State { get; set; }
  public string PassName { get; set; } = string.Empty;

  public bool IsRead()
  {
    return AccessType == ResourceAccessType.Read || AccessType == ResourceAccessType.ReadWrite;
  }

  public bool IsWrite()
  {
    return AccessType == ResourceAccessType.Write || AccessType == ResourceAccessType.ReadWrite;
  }

  public bool ConflictsWith(ResourceUsageInfo _other)
  {
    if(_other == null || Handle != _other.Handle)
      return false;

    if(PassName == _other.PassName)
      return false;

    if(IsWrite() && _other.IsWrite())
      return true;

    if((IsWrite() && _other.IsRead()) || (IsRead() && _other.IsWrite()))
      return true;

    return false;
  }

  public override string ToString()
  {
    return $"ResourceUsage(Handle: {Handle}, Access: {AccessType}, State: {State}, Pass: '{PassName}')";
  }

  public override bool Equals(object? _obj)
  {
    return _obj is ResourceUsageInfo other &&
      Handle == other.Handle &&
      AccessType == other.AccessType &&
      State == other.State &&
      PassName == other.PassName;
  }

  public override int GetHashCode()
  {
    unchecked
    {
      int hash = 17;
      hash = hash * 23 + Handle.GetHashCode();
      hash = hash * 23 + AccessType.GetHashCode();
      hash = hash * 23 + State.GetHashCode();
      hash = hash * 23 + (PassName?.GetHashCode() ?? 0);
      return hash;
    }
  }
}
