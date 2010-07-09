
namespace Sider
{
  public enum RedisType
  {
    None,
    String,
    List,
    Set,
    ZSet,
    Hash,
  }

  public static class RedisTypes
  {
    public static RedisType Parse(string value)
    {
      switch (value) {
        case "string": return RedisType.String;
        case "list": return RedisType.List;
        case "set": return RedisType.Set;
        case "zset": return RedisType.ZSet;
        case "hash": return RedisType.Hash;

        case "none":
        default:
          return RedisType.None;
      }
    }
  }
}
