using Core.Enums;

namespace Core;

public class ResourceHandleGenerator
{
  private uint p_nextId = 1;
  private readonly Dictionary<uint, uint> p_generationMap = [];

  public ResourceHandle Generate(ResourceType _type, string _name)
  {
    uint id = p_nextId++;
    uint generation = 1;

    if(p_generationMap.ContainsKey(id))

      generation = ++p_generationMap[id];
    else
      p_generationMap[id] = generation;

    return new ResourceHandle(id, _type, generation, _name);
  }

  public void Release(ResourceHandle _handle)
  {
    if(_handle.IsValid() && p_generationMap.ContainsKey(_handle.Id))
      p_generationMap[_handle.Id]++;
  }

  public bool IsHandleValid(ResourceHandle _handle)
  {
    return _handle.IsValid() &&
      p_generationMap.TryGetValue(_handle.Id, out uint currentGeneration) &&
      currentGeneration == _handle.Generation;
  }
}