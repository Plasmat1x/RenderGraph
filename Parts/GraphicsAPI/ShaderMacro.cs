namespace GraphicsAPI;

public struct ShaderMacro
{
  public ShaderMacro(string _name, string _definition = "1")
  {
    Name = _name;
    Definition = _definition;
  }

  public string Name { get; set; }
  public string Definition { get; set; }

}