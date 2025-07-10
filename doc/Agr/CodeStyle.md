```csharp

class Foo : IDispose
{
// Типы внутренние
class Node {}
// константы
private const byte MaxNodeIndex = 255;
// статические данные
private static readonly ConcurrentDictionary<int,bool> CachedNodes {get;} = new();
// приватные данные
private bool p_started = false;
private bool p_terminated = false;
// статический конструктор
static Node(){
CachedNodes.Add(MaxNodeIndex,false);
}
// публичные статические методы
public static Node Create() => new (-1);
// конструктор
public Node(int _depth){}
// публичные свойства
public bool IsWorking => p_started && !p_terminated;
// не публичные свойства
private bool IsCorrupted => !p_started && p_terminated;
// публичные методы
public void Start()=>p_started = true;
// Dispose всегда в конце
public void Dipose() => Disposing(true);
// protected методы
protected virtual void OnReset(){}
// приватные методы
private void Reset() => OnReset();
// приватные статические методы
private static IEnumerable<Node> Traverse(Node _node, TraverseMode _mode) {}
}

```