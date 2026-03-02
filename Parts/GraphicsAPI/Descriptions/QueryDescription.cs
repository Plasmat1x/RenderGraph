using GraphicsAPI.Enums;

namespace GraphicsAPI.Descriptions;

public class QueryDescription
{
  public QueryType Type { get; set; } = QueryType.Occlusion;
  public string Name { get; set; } = string.Empty;
  
  public static QueryDescription Occlusion(string _name = "OcclusionQuery") => new()
  {
    Type = QueryType.Occlusion,
    Name = _name
  };
  
  public static QueryDescription BinaryOcclusion(string _name = "BinaryOcclusionQuery") => new()
  {
    Type = QueryType.BinaryOcclusion,
    Name = _name
  };
  
  public static QueryDescription Timestamp(string _name = "TimestampQuery") => new()
  {
    Type = QueryType.Timestamp,
    Name = _name
  };
  
  public static QueryDescription PipelineStatistics(string _name = "PipelineStatsQuery") => new()
  {
    Type = QueryType.PipelineStatistics,
    Name = _name
  };
}
